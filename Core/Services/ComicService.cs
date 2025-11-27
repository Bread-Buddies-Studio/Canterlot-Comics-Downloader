using QuickEPUB;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Extensions;
using System.Security.AccessControl;
using System.IO.Compression;

namespace Core.Services;
public static class ComicService
{
    /// <summary>
    /// Creates an <see cref="Epub"/>.
    /// </summary>
    /// <param name="title">The title of the EPUB.</param>
    /// <param name="authors">The authors of the EPUB.</param>
    /// <param name="panelPaths">The URLS of the Pages.</param>
    /// <returns>The <see cref="Epub"/> object.</returns>
    /// <exception cref="InvalidOperationException">Invalid Image Type.</exception>
    public static Epub CreateComic(string title, string[] authors, string[] panelPaths)
    {
        Epub doc = new(title, string.Join(", ", authors));
        // Add Pages //
        foreach ((int index, string path) in panelPaths.Where(path => path is not null).Index())
        {
            Console.WriteLine("Loading: " + path);
            // Get File Type //
            EpubResourceType fileType = Path.GetExtension(path) switch
            {
                ".png" => EpubResourceType.PNG,
                ".gif" or ".gifv" => EpubResourceType.GIF,
                ".jpg" => EpubResourceType.JPEG,
                _ => EpubResourceType.PNG
            };
            // Import Image //
            doc.AddResource(path, fileType);
            // Create Page //
            doc.AddSection($"Page {index}", $"<img src=\"{Path.GetFileName(path)}\" alt=\"page\" style=\"width:auto;height:auto;\"/>");
            Console.WriteLine("Added " + Path.GetFileName(path));
        }
        // Finish //
        return doc;
    }
}
