using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RequestForDownloadCanterlotComics.Extensions;

namespace RequestForDownloadCanterlotComics;

class Upscaler
{
    public static async Task<string> Upscale(string imagePath, byte scale = 4)
    {
        // Original Data //
        string imageName = Path.GetFileNameWithoutExtension(imagePath);
        string imageNameWithExtension = Path.GetFileName(imagePath);
        // Make Sure Folder Exists //
        Directory.CreateDirectory(UpscaledTemp);
        // Paths //
        string downloadPath = $@"{UpscaledTemp}\{imageNameWithExtension}";
        //Console.WriteLine(imagePath);
        //Console.WriteLine(downloadPath);
        // Begin Upscale //
        using Process process = new();
        process.StartInfo.FileName = UpscalerEXE;
        process.StartInfo.Arguments = string.Join(' ',
        [
            "-i", // input-path
            $"\"{imagePath}\"",

            "-o", // output-path
            $"\"{downloadPath}\"",

            "-s", // scale
            scale.ToString()
        ]);
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.Verb = "runas";
        
        process.Start();
        await process.WaitForExitAsync();

        return downloadPath;
    }
    public static async Task<string[]> Upscale(string[] imagePaths, byte scale = 4)
    {
        string[] downloadPaths = new string[imagePaths.Length];
        foreach ((int index, string imagePath) in imagePaths.Index())
        {
            downloadPaths[index] = await Upscale(imagePath, scale);
            Console.WriteLine($"Finished Upscaling Page x{index}: {100 / imagePaths.Length * index}");
        }

        return downloadPaths;
    }
    public static async Task UpscaleComic(string comicPath, byte scale = 4)
    {
        using FileStream stream = new(comicPath, FileMode.Open);
        using ZipArchive archive = new(stream, ZipArchiveMode.Update);
        // Don't Overwrite //
        int pageAmount = archive.Entries.Count;
        foreach ((int index, var entry) in archive.Entries.Reverse().Index())
        {
            // Entry Data //
            string entryNameWithExtension = Path.GetFileName(entry.FullName);
            // Path Data //
            string tempPagePath = @$"{PanelsTemp}\{entryNameWithExtension}";
            // Extract Page to Upscale //
            entry.ExtractToFile(tempPagePath);
            // Upscale Page //
            string upscaledPagePath = await Upscale(tempPagePath, scale);
            // Overwrite Page //
            entry.Delete();
            archive.CreateEntryFromFile(upscaledPagePath, entryNameWithExtension, CompressionLevel.NoCompression);
            // Clean Up //
            File.Delete(tempPagePath);
            File.Delete(upscaledPagePath);
            // Debug //
            Console.Write($"\x000DFinished Upscaling Page {index} | {100f / pageAmount * index:.000}%");
        }
    }
}
