using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;

namespace Fredags_Bot
{
    internal class CommandHandler
    {
        private CommandsNextExtension Commands { get; }

        public CommandHandler(DiscordClient client, IEnumerable<string> prefixes)
        {
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = prefixes,
                EnableMentionPrefix = true,
                EnableDms = false,
                EnableDefaultHelp = false
            };

            Commands = client.UseCommandsNext(commandsConfig);
        }

        public void RegisterCommands<T>() where T : BaseCommandModule 
        {
            Commands.RegisterCommands<T>();
        }

        public void DebugPrintRegisteredCommands()
        {
            foreach (var command in Commands.RegisteredCommands.Values)
            {
                Console.WriteLine($"Registered command: {command.Name}");
            }
        }
    }
}