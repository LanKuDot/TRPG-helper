/* Manage the message sent to the application console.
 */
using System;
using Discord;

namespace discordTRPGHelper
{
    public class ConsoleManager
    {
        public static void Message(LogSeverity severity, string source, string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            // Color the message
            switch (severity) {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Debug:
            case LogSeverity.Verbose:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
            default:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            }

            Console.WriteLine($"{DateTime.Now:HH:mm:dd} [{severity,8}] {source} : {message}");
            Console.ForegroundColor = originalColor;
        }
    }
}
