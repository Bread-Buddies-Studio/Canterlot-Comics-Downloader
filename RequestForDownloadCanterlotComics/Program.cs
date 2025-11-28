namespace RequestForDownloadCanterlotComics;
// Imports //
using System;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.Numerics;
using Core.Services;
using Upscaler.Services;
using Core.Extensions;
using System.Drawing.Printing;

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
    static readonly string downloadLocation = AppData; // KnownFolders.GetPath(KnownFolder.Downloads);
    static string comicLink = "https://www.canterlotcomics.com/comic/en/alien_twilight_signing_off-1959";
    // Don't Touch //
    static string comicName = comicLink[(comicLink.LastIndexOf('/') + 1)..comicLink.LastIndexOf('-')];
    static int gatheredURLS = 0;
    /// <summary>
    /// Finds patterns in text.
    /// </summary>
    /// <param name="responseBody">
    /// Usually the HTML Source Code.
    /// </param>
    /// <param name="lookingFor">
    /// The pattern to look for.
    /// </param>
    /// <param name="prefix">
    /// A string to add before the strings which are found.
    /// </param>
    /// <param name="suffix">
    /// A string to add after the strings which are found.
    /// </param>
    /// <param name="downloadText">
    /// The download text to be displayed.
    /// </param>
    /// <returns></returns>
    static HashSet<string> LookForInfo(string responseBody, string lookingFor, string prefix = "", string suffix = "", string downloadText = "")
    {
        HashSet<string> information = [];
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
                bool alreadyPresent = !information.Add(prefix + URL + suffix);
                // Already Present //
                if (alreadyPresent)
                    continue;
                // Debug //
                gatheredURLS++;
                Console.ReplaceLine($"{downloadText}: x{gatheredURLS}");
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
    public static HashSet<string> GetChapters(string responseBody)
    {
        // Get Chapters //
        HashSet<string> chapters = LookForInfo(responseBody: responseBody, lookingFor: $"/chap/", prefix: baseURL, downloadText: "Discovered Chapter");
        // Remove chapters that aren't part of the comic... //
        chapters.RemoveWhere((chapter) => !chapter.Contains(comicName));
        // Return Findings //
        return chapters;
        
    }
    static async Task<string> GetURLInfo(string sourceURL)
    {
        try
        {
            string responseBody = await NetworkService.Client.GetStringAsync(sourceURL);
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
            string responseBody = await NetworkService.Client.GetStringAsync(sourceURL);
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
        // Extract Name //
        string fileName = string.Format("{0,8:D8}", panelIndex);
        string fileExtension = Path.GetExtension(sourceURL);
        string downloadedTo = @$"{PanelsTemp}\{fileName}{fileExtension}";
        // Debug //
        //Console.WriteLine("Downloading: " + sourceURL + '\n' + "Extension: " + fileExtension + '\n');
        // Attempt Download //
        bool success = await NetworkService.DownloadFilesAsync(new Uri(sourceURL), downloadedTo);
        // Return Success //
        return success ? downloadedTo : string.Empty;
    }
    public static void UpdateComicName()
    {
        comicName = comicLink[(comicLink.LastIndexOf('/') + 1)..comicLink.LastIndexOf('-')];
    }
    public static string QueryForComicLink()
    {
        string? input;
        while (true)
        {
            // Query //
            Console.WriteLine("Example: https://www.canterlotcomics.com/comic/en/alien_twilight_signing_off-1959");
            Console.Write("Paste comic link here: ");
            // Inpuut //
            input = Console.ReadLine();
            // Conditions //
            if (input is null
                || input.Length <= 38
                || input[0..38] is not "https://www.canterlotcomics.com/comic/")
            {
                Console.WriteLine("Invalid Link!\r\n");
                continue;
            }
            // Successfully got Input //
            break;
        }
        return input;
    }
    public static string[] GetCredits(string responseBody)
    {
        // Authors //
        HashSet<string> authors = LookForInfo(responseBody, "/author/", downloadText: "Finding Authors");
        // Get Authors //
        Console.WriteLine();
        foreach (string author in authors)
            Console.WriteLine($"Found Author: {author}");
        // Return Authors //
        return [.. authors.Select(author => author[8..])];
    }
    static async Task Main(string[] args)
    {
        // Prepare Directories //
        Directory.CreateDirectory(AppData);
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

            Console.WriteLine($"Selected: {chosenComic}");
            // Request Cover Overwrite //
            bool overwriteCover = InputService.YesOrNo("Overwrite Cover?");
            // Open Zip File //
            using FileStream stream = new(chosenComic, FileMode.Open);
            using ZipArchive archive = new(stream, ZipArchiveMode.Update);
            // Get Current Cover //
            ZipArchiveEntry coverEntry = archive.Entries.OrderBy(name => name.Name).First();
            // Add New Cover //
            while (true)
            {
                // Overwrite Cover? //
                if (overwriteCover)
                {
                    // Delete Previous Cover //
                    coverEntry.Delete();
                    // Add new Cover //
                    archive.CreateEntryFromFile(coverPath, coverEntry.Name, CompressionLevel.NoCompression);
                    break;
                }
                // Don't Overwrite //
                // Move Pages //
                foreach ((int index, ZipArchiveEntry entry) in archive.Entries.Reverse().Index()) 
                {
                    string entryName = Path.GetFileNameWithoutExtension(entry.FullName);
                    string entryNameWithExtension = Path.GetFileName(entry.FullName);
                    int pageIndex = int.Parse(entryName[1..]);
                    string fileName = string.Format("-{0,8:D8}", pageIndex + 1);


                    var newEntry = archive.CreateEntry(string.Concat(fileName, entryNameWithExtension.AsSpan(entryNameWithExtension.IndexOf('.'))));
                    // Copy data to new file //
                    using (Stream oldStream = entry.Open())
                        using (Stream newStream = newEntry.Open())
                            oldStream.CopyTo(newStream);
                    // Delete Old Page //
                    entry.Delete();
                    Console.ReplaceLine("Moving Files | " + (100f / archive.Entries.Count * index));
                };
                // Add Cover at Beginning //
                archive.CreateEntryFromFile(coverPath, string.Format("-{0,8:D8}", 1) + Path.GetExtension(coverPath), CompressionLevel.NoCompression);
                break;
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
            // Set Comic Name //
            UpdateComicName();
            // Arrays //
            HashSet<string> panelURLS = [];
            HashSet<string> chapterURLS;
            // Get Main Comic Page //
            string URLInfo = await GetURLInfo(comicLink);
            // Get Credits //
            string[] authors = GetCredits(URLInfo);
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
            // Completed Tasks //
            int completedTasks = 0;
            // Fetch Panels (Pages) //
            IEnumerable<Task> panelFetchTasks = panelURLS.Select(async (URL, i) =>
            {
                // Data //
                string imagePath = await DownloadImageFromURL(URL, i + 1);
                double percentage = Math.Clamp(100f / panelURLS.Count * ++completedTasks, 0, 100);
                // Checks //
                if (imagePath != string.Empty) // SAVE PANEL FILE //
                {
                    // Fix path if broken. //
                    string newPath = PanelsTemp + '\\' + Path.GetFileNameWithoutExtension(imagePath) + Path.GetExtension(imagePath) switch
                    {
                        ".png" => ".png",
                        ".gif" or ".gifv" => ".gif",
                        ".jpg" => ".jpg",
                        _ => ".png"
                    };
                    // Rename File //
                    File.Move(imagePath, newPath, overwrite: true);
                    // Save Reference //
                    panelFiles[i] = newPath;
                }
                // Percentage //
                Console.ReplaceLine($"Installing: {percentage}%");
            });
            // Wait to finish fetching pages //
            await Task.WhenAll(panelFetchTasks);
            // Upscale //
            bool upscalingComic;
            if (upscalingComic = InputService.YesOrNo("Upscale Comic?"))
                panelFiles = await UpscalerService.Upscale(
                    InputService.RequestByte("Scale Factor?", minimum: 2, maximum: 4),
                    panelFiles
                );
            // Downloads Folder //
            Directory.CreateDirectory($"{downloadLocation}\\Downloads");
            // Create Epub //
            string exportPath = $"{downloadLocation}\\Downloads\\{comicName}.epub";
            await ComicService.CreateComicAsync(comicName, authors, coverURL == string.Empty, upscalingComic ? UpscaledTemp : PanelsTemp, panelFiles, exportPath);
            // Export //
            
            Console.WriteLine($"\nComic Path: {exportPath}\n\t{panelFiles.Length} Pages");
            // Cleanup //
            completedTasks = 0;
            IEnumerable<string> filesToDelete = Directory.GetFiles(PanelsTemp).Concat(panelFiles);
            Console.WriteLine();
            foreach (string panelFile in filesToDelete)
            {
                // Calculate //
                double percentage = Math.Clamp(100f / panelURLS.Count * ++completedTasks, 0, 100);
                // Delete File //
                File.Delete(panelFile);
                // Output //
                Console.ReplaceLine($"Cleanup: {percentage}%");
            }
            // Open Downloads Folder //
            Process.Start("explorer.exe", $"{downloadLocation}\\Downloads");
            // Add New Line! //
            Console.WriteLine("\r\n");
            
        }
    }
}