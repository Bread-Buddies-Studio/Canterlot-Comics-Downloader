namespace MainSpace;
// Imports //
using System;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;

using Global;
using System.Collections.ObjectModel;
using RequestForDownloadCanterlotComics;
using System.Numerics;

public static class ZipFileCreator
{
    /// <summary>
    /// Create a ZIP file of the files provided.
    /// </summary>
    /// <param name="fileName">The full path and name to store the ZIP file at.</param>
    /// <param name="files">The list of files to be added.</param>
    public static void CreateZipFile(string fileName, IEnumerable<string> files)
    {
        // Check if CBZ already exists, if it does then delete it to prevent page duplication //
        if (File.Exists(fileName))
            File.Delete(fileName);
        // CBZ zip creation // Create and open a new ZIP file
        var zip = ZipFile.Open(fileName, ZipArchiveMode.Create);
        // Add pages to CBZ file //
        foreach (var file in files.OrderBy(name => name))
        {
            // Download path for the file //
            string downloadPath = Path.GetFileName(file);
            // Does CBZ file exist already? If yes then delete //
            // Add the entry for each file
            zip.CreateEntryFromFile(sourceFileName: file, entryName: Path.GetFileName(file), CompressionLevel.NoCompression);
        }
        // Dispose of the object when we are done
        zip.Dispose();
    }
}

internal static class Program
{
    // Settings //
    static readonly string downloadLocation = Universal.ProgramFiles; // KnownFolders.GetPath(KnownFolder.Downloads);
    static string comicLink = "https://www.canterlotcomics.com/comic/en/alien_twilight_signing_off-1959";
    // Don't Touch //
    static string comicName = comicLink[(comicLink.LastIndexOf('/') + 1)..(comicLink.LastIndexOf('-'))];
    static int gatheredURLS = 0;
    // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
    public static readonly HttpClient client = new();
    static List<string> LookForInfo(string responseBody, string lookingFor, string prefix = "", string suffix = "", string downloadText = "")
    {
        List<string> information = [];
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            int startingIndex = 0;
            while (true)
            {
                int index = responseBody.IndexOf(lookingFor, startingIndex);
                // Conditions //
                if (index is -1)
                    break;
                // Find End of URL //
                int endOfURLIndex = responseBody.IndexOf('"', index);
                // Switch up starting index! //
                startingIndex = endOfURLIndex;
                // Conditions //
                if (endOfURLIndex is -1)
                    break;
                // Success //
                string URL = responseBody[index..endOfURLIndex];
                // Save //
                information.Add(prefix + URL + suffix);
                // Debug //
                gatheredURLS++;
                ConsoleExtensions.ReplaceLine($"{downloadText}: x{gatheredURLS}");
            }
            // Console.WriteLine(responseBody);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }

        return information;
    }
    public static string GetCoverURL(string responseBody)
    {
        // Find Cover URL //
        int beginIndex = responseBody.IndexOf("//img.canterlotcomics.com/maincomic");
        // Conditions //
        if (beginIndex is -1)
            return string.Empty;
        // Find End of Cover URL //
        int endOfURLIndex = responseBody.IndexOf('"', beginIndex);
        // Construct URL //
        string URL = "https:" + responseBody[beginIndex..endOfURLIndex];
        //img.canterlotcomics.com/maincomic
        return URL;
    }
    public static List<string> GetChapters(string responseBody) => 
        LookForInfo(responseBody: responseBody, lookingFor:$"/chap/en/{comicName}", prefix: baseURL, downloadText: "Discovered Chapter");
    static async Task<string> GetURLInfo(string sourceURL)
    {
        try
        {
            string responseBody = await client.GetStringAsync(sourceURL);
            //Console.WriteLine(responseBody);
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }

        return string.Empty;
    }
    static async Task<List<string>> GetPanelURLS(string sourceURL)
    {
        List<string> comicPanels = [];
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            // Have to get new response body because the panels are within their own chapters //
            string responseBody = await client.GetStringAsync(sourceURL);
            // Above three lines can be replaced with new helper method below
            // string responseBody = await client.GetStringAsync(uri);
            string lookingFor = "//img.canterlotcomics.com/maincomic";
            comicPanels.AddRange(LookForInfo(responseBody: responseBody, lookingFor: lookingFor, prefix: "https:", downloadText: "Downloaded Panel"));
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }

        return comicPanels;
    }
    public static async Task<string> DownloadImageFromURL(string sourceURL, int panelIndex)
    {
        // If you want it as PNG
        string fileName = string.Format("-{0,8:D8}", panelIndex);
        string downloadedTo = @$"{PanelsTemp}\{fileName}.png";
        // Attempt Download //
        bool success = await Downloader.DownloadFilesAsync(new Uri(sourceURL), downloadedTo);
        // Return Success //
        return success ? downloadedTo : string.Empty;
    }
    public static void UpdateComicName()
    {
        comicName = comicLink[(comicLink.LastIndexOf('/') + 1)..(comicLink.LastIndexOf('-'))];
    }
    public static string QueryForComicLink()
    {
        Console.WriteLine("\nRemember to run as Administrator!\r\n");
        string? input;
        while (true)
        {
            // Query //
            Console.WriteLine("Example: https://www.canterlotcomics.com/comic/en/alien_twilight_signing_off-1959");
            Console.Write("Paste comic link here: ");
            // Inpuut //
            input = Console.ReadLine();
            // Conditions //
            if (input is null)
            {
                Console.WriteLine("Invalid Link!\r\n");
                continue;
            }
            else if (input.Length <= 41)
            {
                Console.WriteLine("Invalid Link!\r\n");
                continue;
            }
            else if (input[0..41] is not "https://www.canterlotcomics.com/comic/en/")
            {
                Console.WriteLine("Invalid Link!\r\n");
                continue;
            }
            else
                break;
            // Warn //
        }
        return input;
    }
    static async Task Main(string[] args)
    {
        // Cover Update //
        if (args.Length is not 0)
        {
            string coverPath = args[0];
            // Make Sure Download Folder Exists //
            Directory.CreateDirectory(@$"{downloadLocation}\Downloads");
            // Enumerator //
            IEnumerable<string> comicFiles = Directory.EnumerateFiles($@"{downloadLocation}\Downloads");
            // Log All Comics //
            foreach ((int index, string comicPath) in comicFiles.Index())
            {
                string fileName = Path.GetFileNameWithoutExtension(comicPath);
                Console.WriteLine($"{index}. {fileName}");
            }
            // Request Comic //
            string chosenComic = string.Empty;
            while (true)
            {
                Console.WriteLine("Pick a Comic");
                string? input = Console.ReadLine();
                // Pre-Conditions //
                if (input is null)
                {
                    Console.WriteLine("Empty input detected! Please input a number from the list or the name!");
                    continue;
                }
                // Break-Conditions //
                if (int.TryParse(input, out int comicIndex))
                {
                    chosenComic = comicFiles.ElementAt(comicIndex);
                    break;
                }
                if (comicFiles.Any(comicPath => Path.GetFileNameWithoutExtension(comicPath) == input))
                {
                    chosenComic = comicFiles.First(comicPath => Path.GetFileNameWithoutExtension(comicPath) == input);
                    break;
                }
            }
            // Request Cover Overwrite //
            bool overwriteCover = false;
            while (true)
            {
                // Request //
                Console.WriteLine("Overwrite Cover? (y/n)");
                // Get //
                ConsoleKeyInfo key = Console.ReadKey();
                // Conditions //
                if (key.KeyChar is not 'y' and not 'n')
                {
                    Console.WriteLine("Invalid Input!\n");
                    continue;
                }
                // Success //
                overwriteCover = key.KeyChar is 'y';
                break;
            }
            // Open Zip File //
            using FileStream stream = new(chosenComic, FileMode.Open);
            using ZipArchive archive = new(stream, ZipArchiveMode.Update);
            // Get Current Cover //
            ZipArchiveEntry coverEntry = archive.Entries.OrderBy(name => name.Name).First();
            // Add New Cover //
            int failCount = 0;
            while (true)
            {
                try
                {
                    // Overwrite Cover? //
                    if (overwriteCover)
                    {
                        archive.CreateEntryFromFile(coverPath, coverEntry.Name, CompressionLevel.NoCompression);
                        break;
                    }
                    // Don't Overwrite //
                    foreach (var entry in archive.Entries.Reverse())
                    {
                        string entryName = Path.GetFileNameWithoutExtension(entry.FullName);
                        string entryNameWithExtension = Path.GetFileName(entry.FullName);
                        int pageIndex = int.Parse(entryName[1..]);
                        string fileName = string.Format("-{0,8:D8}", pageIndex + 1);

                        var newEntry = archive.CreateEntry(string.Concat(fileName, entryNameWithExtension.AsSpan(entryNameWithExtension.IndexOf('.'))));

                        using (Stream oldStream = entry.Open())
                        using (Stream newStream = newEntry.Open())
                            oldStream.CopyTo(newStream);

                        entry.Delete();
                    }
                    // Create cover when not overwriting //
                    archive.CreateEntryFromFile(coverPath, string.Format("-{0,8:D8}", 1) + Path.GetExtension(coverPath), CompressionLevel.NoCompression);
                    break;
                }

                catch (IOException exception)
                {
                    failCount++;
                    Console.WriteLine($"Failed To Update Cover x{failCount}, trying again...");
                    Console.WriteLine(exception.Message + "\n");
                    await Task.Delay(500);
                }
            }
            // Success //
            Console.WriteLine("\nSuccessfully Added Cover!\n");
        }
        // Main //
        while (true)
        {
            // Reset Counter //
            gatheredURLS = 0;
            // Get Comic Link! //
            comicLink = QueryForComicLink();
            UpdateComicName();
            // Arrays //
            HashSet<string> panelURLS = [];
            List<string> chapterURLS;
            // Get Main Comic Page //
            string URLInfo = await GetURLInfo(comicLink);
            // Download Cover //
            string coverURL = GetCoverURL(URLInfo);
            // Download Chapters //
            chapterURLS = GetChapters(URLInfo);
            // Checks //
            if (URLInfo == string.Empty)
            {
                Console.WriteLine("No info at such URL!\r\n");
                continue;
            }
            if (coverURL != string.Empty)
                panelURLS.Add(GetCoverURL(URLInfo));
            // Add Panels to Download List //
            foreach (string URL in chapterURLS)
            {
                // Get Panels //
                List<string> panelsFound = await GetPanelURLS(URL);
                // Store Panels //
                foreach (string panel in panelsFound)
                    panelURLS.Add(panel);
            }
            // Initialize Panel File Path Array //
            string[] panelFiles = new string[panelURLS.Count];
            // Downloads Folder //
            Directory.CreateDirectory(PanelsTemp);
            // Download Images //
            Console.WriteLine();

            int completedTasks = 1;

            IEnumerable<Task> panelFetchTasks = panelURLS.Select(async (URL, i) =>
                {
                    string imagePath = await DownloadImageFromURL(URL, i + 1);
                    // Checks //
                    if (imagePath != string.Empty) // SAVE PANEL FILE //
                        panelFiles[i] = imagePath;
                    // Percentage //
                    ConsoleExtensions.ReplaceLine("Installing: " + (100f / panelURLS.Count * completedTasks++));
                }
            );

            await Task.WhenAll(panelFetchTasks);
            // Downloads Folder //
            Directory.CreateDirectory(@$"{downloadLocation}\Downloads");
            // Create Zip File //
            ZipFileCreator.CreateZipFile(@$"{downloadLocation}\Downloads\{comicName}.cbz", panelFiles);
            // Cleanup //
            foreach (string panelFile in panelFiles)
                File.Delete(panelFile);
            // Open Downloads Folder //
            Process.Start("explorer.exe", @$"{downloadLocation}\Downloads");
            // Add New Line! //
            Console.WriteLine("\r\n");
            // Upscale //
            if (Input.YesOrNo("Upscale Comic?"))
                await Upscaler.UpscaleComic(@$"{downloadLocation}\Downloads\{comicName}.cbz", Input.RequestByte("Scale Factor?"));
        }
    }
}