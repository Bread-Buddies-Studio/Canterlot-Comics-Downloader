using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services;

public static class InputService
{
    public static bool YesOrNo(string query)
    {
        // Request Input //
        while (true)
        {
            // Query //
            Console.Write(query + " (y/n): ");
            // Input //
            char input = Console.ReadKey().KeyChar;
            // Conditions //
            if (input is not 'y' and not 'n' and not 'Y' and not 'N')
            {
                Console.WriteLine("Invalid Input!\n");
                continue;
            }
            // Make New Line //
            Console.WriteLine();
            // Checks //
            return input is 'y' or 'Y';
        }
    }

    public static int RequestInt(string query)
    {
        // Request Input //
        while (true)
        {

            // Query //
            Console.Write($"{query}: ");
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
        // Request Input //
        while (true)
        {
            // Query //
            Console.Write(query + " (0-255): ");
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
