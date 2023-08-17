using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;

namespace Fredags_Bot
{
    internal class CommandHandler
    {
        private readonly CommandsNextExtension _commands;

        public CommandHandler(DiscordClient client, IEnumerable<string> prefixes)
        {
            _commands = client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = prefixes,
                EnableMentionPrefix = true,
                EnableDms = false,
                EnableDefaultHelp = false
            });
        }

        public void RegisterCommands<T>() where T : BaseCommandModule
        {
            _commands.RegisterCommands<T>();
        }

        public void DebugPrintRegisteredCommands()
        {
            foreach (var kvp in _commands.RegisteredCommands)
            {
                Console.WriteLine($"Registered command: {kvp.Value.Name}");
            }
        }
    }
}