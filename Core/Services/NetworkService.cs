using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services;

public static class NetworkService
{
    public static readonly HttpClient Client = new();

    /// <summary>
    /// Downloads files through a URL across the internet
    /// </summary>
    /// 
    /// <param name="url">
    /// URL of the online file
    /// </param>
    /// 
    /// <param name="outputPath">
    /// Path to save the file to
    /// </param>
    /// 
    /// <returns>Whether the download was successful</returns>
    public static async Task<bool> DownloadFilesAsync(Uri url, string outputPath)
    {
        try
        {
            using Stream networkStream = await Client.GetStreamAsync(url);
            using FileStream fileStream = new(outputPath, FileMode.Create);
            // Create File //
            await networkStream.CopyToAsync(fileStream);
            // Successful //
            return true;
        }
        catch (Exception)
        {
            // Fail //
            return false;
        }
    }
}
