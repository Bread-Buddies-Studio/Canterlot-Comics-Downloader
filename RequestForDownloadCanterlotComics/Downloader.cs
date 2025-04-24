using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MainSpace;

class Downloader
{
    private static readonly HttpClient client = Program.client;
    public static async Task<bool> DownloadFilesAsync(Uri url, string filePath)
    {
        try
        {
            using var stream = await client.GetStreamAsync(url);
            using var fs = new FileStream(filePath, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fs);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
