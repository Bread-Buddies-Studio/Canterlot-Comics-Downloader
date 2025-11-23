using QuickEPUB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Extensions;

public static class EpubExtensions
{
    extension(Epub doc)
    {
        /// <inheritdoc cref="Epub.AddResource(string, EpubResourceType, Stream, bool)"/>
        public void AddResource(string path, EpubResourceType resourceType, bool isCover = false)
        {
            // Create Resource Stream //
            using var resourceStream = File.OpenRead(path);
            resourceStream.Position = 0;
            // Add Resource //
            doc.AddResource(Path.GetFileName(path), resourceType, resourceStream, isCover);
        }

        /// <inheritdoc cref="Epub.Export(Stream)"/>
        /// <param name="path">
        /// Path of the exported file.
        /// </param>
        public void Export(string path)
        {
            // Create Resource Stream //
            using var fs = File.Create(path);
            fs.Position = 0;
            // Export Document // (EPUB)
            doc.Export(fs);
        }
    }
}
