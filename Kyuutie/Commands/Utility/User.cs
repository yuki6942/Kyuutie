using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace Kyuutie.Commands.Utility;

[Command("user")]
[Description("User related commands")]
public class User
{
    [Command("avatar")]
    [Description("Get a user's avatar")]
    
    public async Task AvatarCommandAsync(
        CommandContext ctx,
        [Parameter("user")]
        [Description("The user to get the avatar from")] DiscordUser? user = null)
    {
        user ??= ctx.User;

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Magenta)
            .WithTitle($"{user.GlobalName ?? user.Username}'s avatar")
            .WithImageUrl(user.AvatarUrl);


        DiscordMessageBuilder builder = new DiscordMessageBuilder()
            .AddEmbed(embed)
            .AddComponents(new DiscordLinkButtonComponent(
                user.AvatarUrl,
                "Open in Browser"));

        await ctx.RespondAsync(builder);
        
    }
}
