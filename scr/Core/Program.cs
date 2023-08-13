using DSharpPlus;
using DSharpPlus.Lavalink;
using Fredags_Bot.scr.config;
using Fredags_Bot.scr.Core.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Fredags_Bot
{
    internal class Program
    {
        private static Lazy<DiscordClient> Client;
        private static CommandHandler CommandHandler { get; set; }

        static async Task Main(string[] args)
        {
            var jsonReader = new JSONReader();
            Client = new Lazy<DiscordClient>(() => InitializeClient(jsonReader)); // Pass jsonReader to InitializeClient
            InitializeBot(jsonReader);
            await ConnectAsync(jsonReader);
            CommandHandler.DebugPrintRegisteredCommands(); // Call DebugPrintRegisteredCommands from CommandHandler
            await Task.Delay(-1);
        }

        private static DiscordClient InitializeClient(JSONReader jsonReader) // Accept JSONReader instance
        {
            var config = new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.Token, // Access Token using jsonReader instance
                TokenType = TokenType.Bot,
               // MinimumLogLevel = LogLevel.Debug,
                AutoReconnect = true
            };

            return new DiscordClient(config);
        }

        private static void InitializeBot(JSONReader jsonReader)
        {
            UserEvents.Register(Client.Value); // Register user events
            UserLogging.Register(Client.Value);

            // Initialize CommandHandler with prefixes
            CommandHandler = new CommandHandler(Client.Value, new[] { jsonReader.Prefix });

            // Register commands in CommandHandler
            CommandHandler.RegisterCommands<Commands>();
        }

        private static async Task ConnectAsync(JSONReader jsonReader)
        {
            var lavalink = Client.Value.UseLavalink();
            await Client.Value.ConnectAsync();
            await lavalink.ConnectAsync(CreateLavalinkConfiguration(jsonReader));
        }

        private static LavalinkConfiguration CreateLavalinkConfiguration(JSONReader jsonReader)
        {
            return new LavalinkConfiguration
            {
                Password = jsonReader.Password,
                RestEndpoint = jsonReader.Endpoint,
                SocketEndpoint = jsonReader.Endpoint
            };
        }
    }
}