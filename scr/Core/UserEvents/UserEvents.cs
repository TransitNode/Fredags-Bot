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
        public static void Register(DiscordClient client)
        {
            client.GuildMemberAdded += OnGuildMemberAdded;
            client.GuildMemberRemoved += OnGuildMemberRemoved; // This line registers the method for the event
        }

        private static string FormatAccountAge(TimeSpan accountAge)
        {
            string formattedAge = string.Empty;

            if (accountAge.Days > 365)
            {
                int years = accountAge.Days / 365;
                formattedAge += years > 1 ? $"{years} years " : "1 year ";
                accountAge = accountAge.Subtract(new TimeSpan(365 * years, 0, 0, 0));
            }

            if (accountAge.Days > 30)
            {
                int months = accountAge.Days / 30;
                formattedAge += months > 1 ? $"{months} months " : "1 month ";
                accountAge = accountAge.Subtract(new TimeSpan(30 * months, 0, 0, 0));
            }

            if (accountAge.Days > 0)
            {
                formattedAge += accountAge.Days > 1 ? $"{accountAge.Days} days " : "1 day ";
            }

            return formattedAge.Trim();
        }

        private static string FormatOrdinal(int number)
        {
            string suffix;

            if (number % 100 >= 11 && number % 100 <= 13)
                suffix = "th";
            else if (number % 10 == 1)
                suffix = "st";
            else if (number % 10 == 2)
                suffix = "nd";
            else if (number % 10 == 3)
                suffix = "rd";
            else
                suffix = "th";

            return $"{number}{suffix}";
        }

        private static async Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
        {
            // Get the discord servers total member count
            int memberCount = args.Guild.MemberCount;

            // Calculate the age of the user's discord account
            TimeSpan accountAge = DateTime.UtcNow - args.Member.CreationTimestamp.UtcDateTime;
            string formattedAge = FormatAccountAge(accountAge);

            // Create the embed
            var embed = new DiscordEmbedBuilder
            {
                Title = $"🎉 Welcome to Fredags Mys, {args.Member.Username}!",
                Description = $"Hey {args.Member.Mention}, vibe with us! You're the {FormatOrdinal(memberCount)} member to chill in our musical haven. 🎵\n\nFeel the rhythm, enjoy the company, and let the good times roll. Any requests? Just hit us up <@&686014081596915719>. Cheers to unforgettable tunes! 🎧🍻",
                Color = new DiscordColor("#cd84f1"), // side color for the ribbon
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = args.Member.AvatarUrl }, // Thumbnail
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Joined on {DateTime.UtcNow.AddHours(2).ToString("yyyy-MM-dd HH:mm:ss")} (GMT+02)", IconUrl = "https://cdn-icons-png.flaticon.com/128/1985/1985782.png" } // Footer with icon
            };

            // Add warning triangles if the account is new (less than 1 day old)
            string accountAgeField = accountAge.TotalDays < 1 ? $"⚠️ NEW ACCOUNT ⚠️\nYour account is {formattedAge} old. Please be cautious!" : $"🕰️ Your account is {formattedAge} old. They say time flies, but let's make it dance!";
            embed.AddField("Account Age", accountAgeField, false);

            // Get the channel and send the embed
            var channel = args.Guild.GetChannel(686019095010738405);
            await channel.SendMessageAsync(embed: embed);

            // Assign a role
            var role = args.Guild.GetRole(686014107375501390);
            await args.Member.GrantRoleAsync(role);
        }

        private static async Task OnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs args)
        {
            // Log the leaving member's details
            Console.WriteLine($"User {args.Member.Username} has left the server {args.Guild.Name}");

            // Calculate the duration of membership
            TimeSpan membershipDuration = DateTime.UtcNow - args.Member.JoinedAt.UtcDateTime;
            string formattedDuration = FormatAccountAge(membershipDuration);

            // Collect the roles
            string roles = string.Join(", ", args.Member.Roles.Select(r => r.Mention));
            string rolesField = string.IsNullOrEmpty(roles) ? "No specific roles were assigned." : $"During your stay, you held these roles: {roles}.";

            // Create the embed
            var embed = new DiscordEmbedBuilder
            {
                Title = $"🎶 Farewell, {args.Member.Username}!",
                Description = $"Thanks for the vibes, {args.Member.Username}. You were with us for {formattedDuration} and contributed to the melody of our community. 🎵\n\n{rolesField}\n\nWe hope you carry the rhythm wherever you go!",
                Color = new DiscordColor("#D35400"), // side ribbon color
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = args.Member.AvatarUrl }, // User's avatar
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Left on {DateTime.UtcNow.AddHours(2).ToString("yyyy-MM-dd HH:mm:ss")} (GMT+02)", IconUrl = "https://cdn-icons-png.flaticon.com/128/1985/1985782.png" } // Footer with icon
            };

            // Get the channel and send the embed
            var channel = args.Guild.GetChannel(686019095010738405);
            await channel.SendMessageAsync(embed: embed);
        }

    }
}