using DSharpPlus;
using DSharpPlus.Lavalink;
using Fredags_Bot.scr.config;
using Fredags_Bot.scr.Core.Commands;
using System;
using System.Threading.Tasks;

namespace Fredags_Bot
{
    internal class Program
    {
        private static DiscordClient Client { get; set; }
        private static CommandHandler CommandHandler { get; set; }
        private static JSONReader JsonReader { get; set; }

        static async Task Main(string[] args)
        {
            try
            {
                InitializeJsonReader();
                InitializeClient();
                InitializeBot();
                await ConnectAsync();
                CommandHandler.DebugPrintRegisteredCommands();
                await Task.Delay(-1); // Keeps the bot running indefinitely
            }
            catch (Exception ex)
            {
                // exception handler
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void InitializeJsonReader()
        {
            JsonReader = new JSONReader();
        }

        private static void InitializeClient()
        {
            var config = new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = JsonReader.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(config);
        }

        private static void InitializeBot()
        {
            UserEvents.Register(Client); // Register user events
            UserLogging.Register(Client);

            // Initialize CommandHandler with prefixes
            CommandHandler = new CommandHandler(Client, new[] { JsonReader.Prefix });
            CommandHandler.RegisterCommands<Commands>();
        }

        private static async Task ConnectAsync()
        {
            var lavalink = Client.UseLavalink();
            await Client.ConnectAsync();
            await lavalink.ConnectAsync(new LavalinkConfiguration
            {
                Password = JsonReader.Password,
                RestEndpoint = JsonReader.Endpoint,
                SocketEndpoint = JsonReader.Endpoint
            });
        }
    }
}
