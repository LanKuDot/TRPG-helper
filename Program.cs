using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace discordTRPGHelper
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

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

            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            /* Get the token of the bot from the external file */
            string token;
            using (StreamReader reader = new StreamReader("../../bot_token.txt")) {
                token = reader.ReadLine();
            }

            await InstalCommandAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        /*
         * @brief Load all the commands in this assembly.
         */
        private async Task InstalCommandAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
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
            /* Only handle the user message. */
            var message = msg as SocketUserMessage;
            if (message == null)
                return;

            int argPos = 0;
            // If the message starts with '!' or the robot is mentioned,
            // that is the command
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;
            // Create a Command context
            var context = new SocketCommandContext(_client, message);
            // Execute the command
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
