// Imports //
using System;
using System.Net;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Net.Http;

using System.Threading.Tasks;
using System.Security.Policy;
using System.Drawing;
using System.IO.Compression;
using System.Windows.Media.Animation;
using System.Reflection;
using System.Runtime.InteropServices;
public enum KnownFolder
{
    Contacts,
    Downloads,
    Favorites,
    Links,
    SavedGames,
    SavedSearches
}

public static class KnownFolders
{
    private static readonly Dictionary<KnownFolder, Guid> _guids = new()
    {
        [KnownFolder.Contacts] = new("56784854-C6CB-462B-8169-88E350ACB882"),
        [KnownFolder.Downloads] = new("374DE290-123F-4565-9164-39C4925E467B"),
        [KnownFolder.Favorites] = new("1777F761-68AD-4D8A-87BD-30B759FA33DD"),
        [KnownFolder.Links] = new("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968"),
        [KnownFolder.SavedGames] = new("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"),
        [KnownFolder.SavedSearches] = new("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA")
    };

    public static string GetPath(KnownFolder knownFolder)
    {
        return SHGetKnownFolderPath(_guids[knownFolder], 0);
    }

    [DllImport("shell32",
        CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
    private static extern string SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags,
        nint hToken = 0);
}

public static class ZipFileCreator
{
    /// <summary>
    /// Create a ZIP file of the files provided.
    /// </summary>
    /// <param name="fileName">The full path and name to store the ZIP file at.</param>
    /// <param name="files">The list of files to be added.</param>
    public static void CreateZipFile(string fileName, IEnumerable<string> files)
    {
        // Create and open a new ZIP file
        var zip = ZipFile.Open(fileName, ZipArchiveMode.Create);
        foreach (var file in files.OrderBy(name => name))
        {
            // Add the entry for each file
            zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
        }
        // Dispose of the object when we are done
        
        zip.Dispose();
    }
}

internal static class Program
{
    // Constants //
    const string baseURL = "https://www.canterlotcomics.com"; // Canterlot Comics Website //
    // Settings //
    static readonly string downloadLocation = KnownFolders.GetPath(KnownFolder.Downloads);
    static string comicLink = "https://www.canterlotcomics.com/comic/en/alien_twilight_signing_off-1959";
    // Don't Touch //
    static string comicName = comicLink[(comicLink.LastIndexOf('/') + 1)..(comicLink.LastIndexOf('-'))];
    static int gatheredURLS = 0;
    // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
    static readonly HttpClient client = new();
    static List<string> LookForInfo(string responseBody, string lookingFor, string prefix = "", string suffix = "", string downloadText = "")
    {
        List<string> information = [];
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            int startingIndex = 0;
            //Console.WriteLine(responseBody);
            while (true)
            {
                int index = responseBody.IndexOf(lookingFor, startingIndex);
                // Conditions //
                if (index is -1)
                {
                    break;
                }
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
                Console.WriteLine($"{downloadText}: x{gatheredURLS}");
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
    public static async Task DownloadFileTaskAsync(HttpClient client, Uri uri, string FileName)
    {
        using (var s = await client.GetStreamAsync(uri))
        {
            using (var fs = new FileStream(FileName, FileMode.CreateNew))
            {
                await s.CopyToAsync(fs);
            }
        }
    }
    public static string DownloadImageFromURL(string sourceURL, int panelIndex)
    {
        using (WebClient webClient = new WebClient())
        {
            byte[] data = webClient.DownloadData(new Uri(sourceURL));

            using (MemoryStream mem = new MemoryStream(data))
            {
                using (var yourImage = Image.FromStream(mem))
                {
                    // If you want it as Jpeg
                    string fileName = string.Format("-{0,8:D8}", panelIndex);
                    string downloadedTo = @$"{downloadLocation}\{fileName}.jpg";
                    yourImage.Save(downloadedTo, ImageFormat.Jpeg);
                    return downloadedTo;
                }
            }
        }
    }
    public static void UpdateComicName()
    {
        comicName = comicLink[(comicLink.LastIndexOf('/') + 1)..(comicLink.LastIndexOf('-'))];
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

    static async Task Main()
    {
        while (true)
        {
            // Get Comic Link! //
            comicLink = QueryForComicLink();
            UpdateComicName();
            // Arrays //
            List<string> panelURLS = [];
            List<string> chapterURLS;
            // Get Main Comic Page //
            string URLInfo = await GetURLInfo(comicLink);
            // Download Cover //
            string coverURL = GetCoverURL(URLInfo);
            // Download Chapters //
            chapterURLS = GetChapters(URLInfo);
            // Checks //
            if (coverURL != string.Empty)
                panelURLS.Add(GetCoverURL(URLInfo));
            // Add Panels to Download List //
            foreach (string URL in chapterURLS)
                panelURLS.AddRange(await GetPanelURLS(URL));
            // Initialize Panel File Path Array //
            string[] panelFiles = new string[panelURLS.Count];
            // Download Images //
            for (int i = 0; i < panelURLS.Count; i++)
            {
                string URL = panelURLS[i];

                panelFiles[i] = DownloadImageFromURL(URL, i + 1);
                // Percentage //
                Console.WriteLine("Installing: " + (100f / panelURLS.Count * i));
            }
            // Create Zip File //
            ZipFileCreator.CreateZipFile($"{downloadLocation}/{comicName}.cbz", panelFiles);
            // Cleanup //
            foreach (string panelFile in panelFiles)
            {
                System.IO.File.Delete(panelFile);
            }
        }
    }
}