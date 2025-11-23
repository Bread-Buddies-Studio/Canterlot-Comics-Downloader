using QuickEPUB;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Extensions;
using System.Security.AccessControl;

namespace Core.Services;
public static class ComicService
{
    public static Epub CreateComic(string title, string[] authors, string[] panelPaths)
    {
        Epub doc = new(title, string.Join(", ", authors));
        // Add Pages //
        foreach ((int index, string path) in panelPaths.Index())
        {
            Console.WriteLine("Loading: " + path);
            // Get File Type //
            EpubResourceType fileType = Path.GetExtension(path) switch
            {
                ".png" => EpubResourceType.PNG,
                ".gif" => EpubResourceType.GIF,
                ".jpg" => EpubResourceType.JPEG,
                _ => throw new InvalidOperationException("Invalid Image Type")

            };
            // Import Image //
            doc.AddResource(path, fileType);
            // Create Page //
            doc.AddSection($"Page {index}", $"<img src=\"{Path.GetFileName(path)}\" alt=\"page\"/>");
            Console.WriteLine("Added " + Path.GetFileName(path));
        }
        // Finish //
        return doc;
    }
}
