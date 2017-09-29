using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using System;
using System.IO;
using System.Threading.Tasks;

namespace discordTRPGHelper
{
    class Program
    {
        private DiscordSocketClient _client;
        private Dice _dice;

        /*
         * The entry point of the program.
         */
        static void Main(string[] args)
            => new Program().MainAsyc().GetAwaiter().GetResult();

        public async Task MainAsyc()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig {
                WebSocketProvider = WS4NetProvider.Instance,
            });
            _client.Log += Logger;
            _client.MessageReceived += MessageReceived;

            _dice = new Dice();

            /* Get the token of the bot from the external file */
            string token;
            using (StreamReader reader = new StreamReader("../../bot_token.txt")) {
                token = reader.ReadLine();
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        /*
         * @brief The logging handler
         */
        private static Task Logger(LogMessage msg)
        {
            ConsoleColor orginalColor = Console.ForegroundColor;

            // Color the message
            switch (msg.Severity) {
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

            Console.WriteLine($"{DateTime.Now:tt hh:mm:dd} [{msg.Severity, 8}] {msg.Source} : {msg.Message}");
            Console.ForegroundColor = orginalColor;

            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage msg)
        {
            if (msg.Content.Contains("D") || msg.Content.Contains("d")) {
                await msg.Channel.SendMessageAsync(_dice.GetDiceResult(msg.Content));
            }
        }
    }
}
