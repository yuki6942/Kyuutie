using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Kyuutie.api.twitch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kyuutie.Events;

public class KyuutieEventHandler
{
    private readonly IServiceProvider _services;
    private readonly DiscordShardedClient _client;
    private readonly IConfiguration _config;

    public KyuutieEventHandler(IServiceProvider services, DiscordShardedClient client, IConfiguration config)
    {
        this._services = services;
        this._client = client;
        this._config = config;
        this._client.GuildDownloadCompleted += OnGuildDownloadCompletedAsync;
    }

#pragma warning disable CA1822
    // ReSharper disable MemberCanBeMadeStatic.Global
    public async Task OnErrorAsync(CommandsExtension commands, CommandErroredEventArgs e)
        // ReSharper restore MemberCanBeMadeStatic.Global
#pragma warning restore CA1822
    {
        commands.Client.Logger.LogError("An error has occurred: {Exception}", e.Exception);
        if (e.Exception is ChecksFailedException)
        {
            
            await e.Context.RespondAsync("Only the bot owner can use these commands!");
            await e.Context.DeferResponseAsync();
        }
        else
        {
            await e.Context.RespondAsync(
                $"An error has occurred, please report the following error on our discord:\n{e.Exception}");
            await e.Context.DeferResponseAsync();
        }
        
    }

    public async Task OnGuildDownloadCompletedAsync(DiscordClient client, GuildDownloadCompletedEventArgs e)
    {
        TwitchApi api = new($"{this._config["twitch-clientid"]}", $"{this._config["twitch-authtoken"]}");

        bool messageSent = false;
        
        Func<Task> checkStreams = async () =>
        {
            while (true)
            {
                SearchChannels channel = await api.SearchChannelsAsync("MikyuuVT");
               // Console.WriteLine($"Name: {channel.display_name}");
               // Console.WriteLine($"Stream title: {channel.title}");
               // Console.WriteLine($"Streamer is live: {channel.is_live}");
            
                if (channel.is_live  && messageSent is false)
                {
                                    
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithAuthor($"{channel.display_name} is now live on Twitch!", $"https://www.twitch.tv/{channel.display_name}", $"{channel.thumbnail_url}")
                        .WithTitle($"{channel.title}")
                        .WithUrl($"https://twitch.tv/{channel.display_name}")
                        .WithColor(DiscordColor.Magenta)
                        .AddField("Game", $"{channel.game_name}");

                    DiscordMessageBuilder builder = new DiscordMessageBuilder()
                        .AddEmbed(embed)
                        .AddComponents(new DiscordLinkButtonComponent(
                            $"https://www.twitch.tv/{channel.display_name}",
                            "Watch Stream"));
                    
                    DiscordChannel streamChannel = await client.GetChannelAsync(1128276415155028104);

                    await streamChannel.SendMessageAsync("<@&1128276411845726283>");
                    await streamChannel.SendMessageAsync(builder);

                    messageSent = true;
                }
                
                if (!channel.is_live && messageSent)
                {
                    messageSent = false;
                }
                
                await Task.Delay(3000);
            }
        };

        _ = checkStreams();

    }
    

    
}
