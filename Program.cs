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

        /**
         * The entry point of the program.
         */
        static void Main(string[] args)
            => new Program().MainAsyc().GetAwaiter().GetResult();

        public async Task MainAsyc()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig {
                WebSocketProvider = WS4NetProvider.Instance,
            });
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            /* Get the token of the bot from the external file */
            string token;
            using (StreamReader reader = new StreamReader("../../bot_token.txt")) {
                token = reader.ReadLine();
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage msg)
        {
            if (msg.Content == "!Ping") {
                await msg.Channel.SendMessageAsync("Pong!");
            }
        }
    }
}
