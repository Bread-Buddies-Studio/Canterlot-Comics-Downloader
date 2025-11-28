using Core.Extensions;
using CPubLib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.Marshalling;
using System.Security.AccessControl;
using System.Text;

namespace Core.Services;
public static class ComicService
{
    public static async Task CreateComicAsync(string title, string[] authors, bool isCoverIncluded, string panelDirectory, string[] panelPaths, string exportPath)
    {
        // Create Epub Process //
        using Process process = new()
        {
            StartInfo = new ProcessStartInfo(EpubEXE, string.Join(' ',
            [
                "--ignore-cover-aspect-ratio",
                $"{(isCoverIncluded ? "--cover " + panelPaths.First() : string.Empty)}",
                "--directory", panelDirectory,
                "--output", exportPath,

                "--title", title,
                "--author", string.Join(',', authors)
            ])),
        };
        // Start Process //
        process.Start();
        // Get Output //
        process.OutputDataReceived += (sender, eventData) => 
        {
            string? line = eventData.Data;
            // Conditions //
            if (line is null)
                return;
            // Output //
            Console.WriteLine(line);
        };
        // Wait for exit. //
        await process.WaitForExitAsync();
    }
}