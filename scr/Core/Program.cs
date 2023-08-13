using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Fredags_Bot.scr.config;
using Fredags_Bot.scr.Core.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Fredags_Bot
{
    internal class Program
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }

        static async Task Main(string[] args)
        {
            var jsonReader = new JSONReader();
            InitializeBot(jsonReader);
            await ConnectAsync(jsonReader);
            DebugPrintRegisteredCommands();
            await Task.Delay(-1);
        }

        private static void InitializeBot(JSONReader jsonReader)
        {
            var config = new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug,
                AutoReconnect = true
            };

            Client = new DiscordClient(config);
            UserEvents.Register(Client); // Register user events
            UserLogging.Register(Client);

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { jsonReader.Prefix },
                EnableMentionPrefix = true,
                EnableDms = false,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<Commands>();
        }

        private static async Task ConnectAsync(JSONReader jsonReader)
        {
            var lavalink = Client.UseLavalink();
            await Client.ConnectAsync();
            await lavalink.ConnectAsync(CreateLavalinkConfiguration(jsonReader));
        }

        private static LavalinkConfiguration CreateLavalinkConfiguration(JSONReader jsonReader)
        {
            if (!int.TryParse(jsonReader.Port, out int port) || port < 1 || port > 65535)
            {
                throw new ArgumentException("Invalid port number.");
            }

            var endpoint = new ConnectionEndpoint
            {
                Hostname = jsonReader.Hostname,
                Port = port
            };

            return new LavalinkConfiguration
            {
                Password = jsonReader.Password,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };
        }

        private static void DebugPrintRegisteredCommands()
        {
            foreach (var command in Commands.RegisteredCommands.Values)
            {
                Console.WriteLine($"Registered command: {command.Name}");
            }
        }
    }
}