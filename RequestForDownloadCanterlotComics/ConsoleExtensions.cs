using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestForDownloadCanterlotComics;

public static class ConsoleExtensions
{
    public static void ReplaceLine(string message)
    {
        Console.Write('\r' + new string(' ', Console.BufferWidth));
        Console.Write('\r' + message);
    }
}
