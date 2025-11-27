using Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upscaler.Services;

public static class UpscalerService
{
    public static async Task<string> Upscale(string imagePath, Process process, byte scale = 4)
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
    public static async Task<string[]> Upscale(byte scale = 4, params string[] imagePaths)
    {
        // Allocate //
        string[] upscaledPaths = new string[imagePaths.Length];
        // Create Process //
        using Process upscalerProcess = new();
        upscalerProcess.StartInfo.FileName = UpscalerEXE;
        // Upscale //
        foreach ((int index, string path) in imagePaths.Index())
        {
            // Conditions //
            if (path == string.Empty)
                continue;
            // Upscale //
            upscaledPaths[index] = await Upscale(path, upscalerProcess, scale);
            // Output //
            ConsoleExtensions.ReplaceLine($"Finished Upscaling Page {index} | {100f / imagePaths.Length * index:.000}%");
        }
        // Return Upscaled //
        return upscaledPaths;
    }
    public static async Task UpscaleComic(string comicPath, byte scale = 4)
    {
        using FileStream stream = new(comicPath, FileMode.Open);
        using ZipArchive archive = new(stream, ZipArchiveMode.Update);
        using Process upscalerProcess = new();

        upscalerProcess.StartInfo.FileName = UpscalerEXE;
        // Don't Overwrite //
        int pageAmount = archive.Entries.Count;

        Console.WriteLine();
        foreach ((int index, var entry) in archive.Entries.Reverse().Index())
        {
            // Entry Data //
            string entryNameWithExtension = Path.GetFileName(entry.FullName);
            // Path Data //
            string tempPagePath = @$"{PanelsTemp}\{entryNameWithExtension}";
            // Extract Page to Upscale //
            entry.ExtractToFile(tempPagePath);
            // Upscale Page //
            string upscaledPagePath = await Upscale(tempPagePath, upscalerProcess, scale);
            // Overwrite Page //
            entry.Delete();
            archive.CreateEntryFromFile(upscaledPagePath, entryNameWithExtension, CompressionLevel.NoCompression);
            // Clean Up //
            File.Delete(tempPagePath);
            File.Delete(upscaledPagePath);
            // Debug //
            ConsoleExtensions.ReplaceLine($"Finished Upscaling Page {index} | {100f / pageAmount * index:.000}%");
        }
    }
}
