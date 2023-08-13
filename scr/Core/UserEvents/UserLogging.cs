using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Linq;
using System.Threading.Tasks;

namespace Fredags_Bot
{
    public class UserLogging
    {
        private static readonly ulong logChannelId = 686022816700694538;
        private static readonly ulong[] excludedChannelIds = { 686022816700694538, 1094295537253109770, 1044275476832714784 };

        public static void Register(DiscordClient client)
        {
            client.MessageDeleted += OnMessageDeleted;
            client.MessageUpdated += OnMessageUpdated;
            client.UserUpdated += OnUserUpdated;
            client.GuildMemberUpdated += OnGuildMemberUpdated;
        }

        private static async Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs args)
        {
            if (excludedChannelIds.Contains(args.Channel.Id) || args.Message.Author.IsBot) return; // Don't log events from the excluded channels or messages by the bot.

            var logChannel = await sender.GetChannelAsync(logChannelId);

            var embed = new DiscordEmbedBuilder
            {
                Title = "🗑️ Message Deleted",
                Description = $"Message by {args.Message.Author.Mention} in {args.Channel.Mention} was deleted.",
                Color = DiscordColor.Red,
                Timestamp = System.DateTimeOffset.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(args.Message.Content))
                embed.AddField("Content", args.Message.Content);

            await logChannel.SendMessageAsync(embed: embed);
        }

        private static async Task OnMessageUpdated(DiscordClient sender, MessageUpdateEventArgs args)
        {
            if (excludedChannelIds.Contains(args.Channel.Id) || args.Message.Author.IsBot) return; // Don't log events from the excluded channels or messages by the bot.

            if (args.Message.Content == args.MessageBefore.Content)
                return;

            var logChannel = await sender.GetChannelAsync(logChannelId);

            var embed = new DiscordEmbedBuilder
            {
                Title = "✏️ Message Edited",
                Description = $"Message by {args.Message.Author.Mention} in {args.Channel.Mention} was edited.",
                Color = DiscordColor.Gold,
                Timestamp = System.DateTimeOffset.UtcNow
            };

            embed.AddField("Before", args.MessageBefore.Content ?? "No Content");
            embed.AddField("After", args.Message.Content ?? "No Content");

            await logChannel.SendMessageAsync(embed: embed);
        }

        private static async Task OnUserUpdated(DiscordClient sender, UserUpdateEventArgs args)
        {
            if (args.UserBefore.AvatarUrl == args.UserAfter.AvatarUrl)
                return;

            var logChannel = await sender.GetChannelAsync(logChannelId);

            var embed = new DiscordEmbedBuilder
            {
                Title = "🖼️ Avatar Updated",
                Description = $"{args.UserAfter.Mention} has updated their avatar.",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = args.UserAfter.AvatarUrl },
                Color = DiscordColor.Cyan,
                Timestamp = System.DateTimeOffset.UtcNow
            };

            await logChannel.SendMessageAsync(embed: embed);
        }

        private static async Task OnGuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs args)
        {
            var addedRoles = args.RolesAfter.Except(args.RolesBefore).ToList();
            var removedRoles = args.RolesBefore.Except(args.RolesAfter).ToList();

            if (addedRoles.Count == 0 && removedRoles.Count == 0) return;

            var logChannel = await sender.GetChannelAsync(logChannelId);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"🎭 Role Update: {args.Member.Username}",
                Color = new DiscordColor("#9B59B6"),
                Timestamp = System.DateTimeOffset.UtcNow
            };

            if (addedRoles.Count > 0)
            {
                string roles = string.Join(", ", addedRoles.Select(r => r.Mention));
                embed.AddField("Added Roles", roles, false);
            }

            if (removedRoles.Count > 0)
            {
                string roles = string.Join(", ", removedRoles.Select(r => r.Mention));
                embed.AddField("Removed Roles", roles, false);
            }

            await logChannel.SendMessageAsync(embed: embed);
        }
    }
}