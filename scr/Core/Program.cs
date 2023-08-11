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
            var configuration = new JSONReader(); // Read the json configuration
            InitializeBot(configuration); // Set up the bot

            await ConnectAsync(configuration); // Pass the json configuration here
            DebugPrintRegisteredCommands(); // Print commands for debugging
            await Task.Delay(-1); // Keep the program running indefinitely
        }

        private static void InitializeBot(JSONReader configuration)
        {
            Client = InitializeDiscordClient(configuration);
            Commands = InitializeCommands(configuration);
        }

        private static async Task ConnectAsync(JSONReader jsonReader) 
        {
            var lavalink = InitializeLavalink();
            await Client.ConnectAsync();
            await lavalink.ConnectAsync(CreateLavalinkConfiguration(jsonReader)); 
        }

        private static DiscordClient InitializeDiscordClient(JSONReader jsonReader)
        {
            var config = new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug,
                AutoReconnect = true
            };

            var client = new DiscordClient(config);
            return client;
        }

        private static CommandsNextExtension InitializeCommands(JSONReader jsonReader)
        {
            var config = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { jsonReader.Prefix },
                EnableMentionPrefix = true,
                EnableDms = false,
                EnableDefaultHelp = false
            };

            var commands = Client.UseCommandsNext(config);
            commands.RegisterCommands<Commands>();
            return commands;
        }

        private static LavalinkExtension InitializeLavalink()
        {
            return Client.UseLavalink(); // Set up Lavalink for audio
        }

        private static LavalinkConfiguration CreateLavalinkConfiguration(JSONReader jsonReader)
        {
            // Try to parse the port and validate its value
            if (!int.TryParse(jsonReader.Port, out int port) || port < 1 || port > 65535)
            {
                throw new ArgumentException("Invalid port number.");
            }

            // Configure the Lavalink connection with the valid port
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
            // Loop through and print all registered commands
            foreach (var command in Commands.RegisteredCommands.Values)
            {
                Console.WriteLine($"Registered command: {command.Name}");
            }
        }
    }
}