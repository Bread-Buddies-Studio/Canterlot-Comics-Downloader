using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestForDownloadCanterlotComics;

public static class Input
{
    public static bool YesOrNo(string query)
    {
        // Query //
        Console.WriteLine(query + " (y/n)");
        // Request Input //
        while (true)
        {
            // Input //
            char input = Console.ReadKey().KeyChar;
            // Conditions //
            if (input is not 'y' and not 'n' and not 'Y' and not 'N')
            {
                Console.WriteLine("Invalid Input!\n");
                continue;
            }
            // Checks //
            return input is 'y' or 'Y';
        }
    }

    public static int RequestInt(string query)
    {
        // Query //
        Console.WriteLine(query);
        // Request Input //
        while (true)
        {
            // Input //
            string? input = Console.ReadLine();
            // Conditions //
            if (input is null)
            {
                Console.WriteLine("Invalid Input!\n");
                continue;
            }
            if (!int.TryParse(input, out int chosenInt))
            {
                Console.WriteLine("Invalid Input!\n");
                continue;
            }
            // Checks //
            return chosenInt;
        }
    }

    public static byte RequestByte(string query)
    {
        // Query //
        Console.WriteLine(query + " (0-255)");
        // Request Input //
        while (true)
        {
            // Input //
            string? input = Console.ReadLine();
            // Conditions //
            if (input is null)
            {
                Console.WriteLine("Invalid Input!\n");
                continue;
            }
            if (!byte.TryParse(input, out byte chosenInt))
            {
                Console.WriteLine("Invalid Input!\n");
                continue;
            }
            // Checks //
            return chosenInt;
        }
    }
}
