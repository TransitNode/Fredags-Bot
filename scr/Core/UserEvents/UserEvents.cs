using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fredags_Bot
{
    public class UserEvents
    {
        private const ulong WELCOME_CHANNEL_ID = 686019095010738405;
        private const ulong ROLE_ID = 686014107375501390;

        public static void Register(DiscordClient client)
        {
            client.GuildMemberAdded += OnGuildMemberAdded;
            client.GuildMemberRemoved += OnGuildMemberRemoved;
        }

        private static string FormatAccountAge(TimeSpan accountAge)
        {
            string formattedAge = string.Empty;

            int years = accountAge.Days / 365;
            if (years > 0) formattedAge += years > 1 ? $"{years} years " : "1 year ";

            int months = (accountAge.Days % 365) / 30;
            if (months > 0) formattedAge += months > 1 ? $"{months} months " : "1 month ";

            int days = accountAge.Days % 30;
            if (days > 0) formattedAge += days > 1 ? $"{days} days " : "1 day ";

            return formattedAge.Trim();
        }

        private static string FormatOrdinal(int number)
        {
            int tens = number % 100;

            if (tens >= 11 && tens <= 13)
                return $"{number}th";

            switch (number % 10)
            {
                case 1:
                    return $"{number}st";
                case 2:
                    return $"{number}nd";
                case 3:
                    return $"{number}rd";
                default:
                    return $"{number}th";
            }
        }


        private static async Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
        {
            int memberCount = args.Guild.MemberCount;

            TimeSpan accountAge = DateTime.UtcNow - args.Member.CreationTimestamp.UtcDateTime;
            string formattedAge = FormatAccountAge(accountAge);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"🎉 Welcome to Fredags Mys, {args.Member.Username}!",
                Description = $"Hey {args.Member.Mention}, vibe with us! You're the {FormatOrdinal(memberCount)} member to chill in our musical haven. 🎵\n\nFeel the rhythm, enjoy the company, and let the good times roll. Any requests? Just hit us up <@&686014081596915719>. Cheers to unforgettable tunes! 🎧🍻",
                Color = new DiscordColor("#cd84f1"),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = args.Member.AvatarUrl },
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Joined on {DateTime.UtcNow.AddHours(2).ToString("yyyy-MM-dd HH:mm:ss")} (GMT+02)", IconUrl = "https://cdn-icons-png.flaticon.com/128/1985/1985782.png" }
            };

            string accountAgeField = accountAge.TotalDays < 1 ? $"⚠️ NEW ACCOUNT ⚠️\nYour account is {formattedAge} old. Please be cautious!" : $"🕰️ Your account is {formattedAge} old. They say time flies, but let's make it dance!";
            embed.AddField("Account Age", accountAgeField, false);

            var channel = args.Guild.GetChannel(WELCOME_CHANNEL_ID);
            await channel.SendMessageAsync(embed: embed);

            var role = args.Guild.GetRole(ROLE_ID);
            await args.Member.GrantRoleAsync(role);
        }

        private static async Task OnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs args)
        {
            Console.WriteLine($"User {args.Member.Username} has left the server {args.Guild.Name}");

            TimeSpan membershipDuration = DateTime.UtcNow - args.Member.JoinedAt.UtcDateTime;
            string formattedDuration = FormatAccountAge(membershipDuration);

            string roles = string.Join(", ", args.Member.Roles.Select(r => r.Mention));
            string rolesField = string.IsNullOrEmpty(roles) ? "No specific roles were assigned." : $"During your stay, you held these roles: {roles}.";

            var embed = new DiscordEmbedBuilder
            {
                Title = $"🎶 Farewell, {args.Member.Username}!",
                Description = $"Thanks for the vibes, {args.Member.Username}. You were with us for {formattedDuration} and contributed to the melody of our community. 🎵\n\n{rolesField}\n\nWe hope you carry the rhythm wherever you go!",
                Color = new DiscordColor("#D35400"),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = args.Member.AvatarUrl },
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Left on {DateTime.UtcNow.AddHours(2).ToString("yyyy-MM-dd HH:mm:ss")} (GMT+02)", IconUrl = "https://cdn-icons-png.flaticon.com/128/1985/1985782.png" }
            };

            var channel = args.Guild.GetChannel(WELCOME_CHANNEL_ID);
            await channel.SendMessageAsync(embed: embed);
        }
    }
}