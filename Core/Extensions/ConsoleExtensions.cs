using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions;

public static class ConsoleExtensions
{
    extension(Console)
    {
        public static void EraseLine() => Console.Write('\r' + new string(' ', Console.BufferWidth));
        public static void ReplaceLine(string message)
        {
            Console.Write($"\r{message}{new string(' ', Console.BufferWidth - message.Length)}");
        }
    }
}
