using System;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Kyuutie.api.twitch;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Kyuutie.Events
{
    public class KyuutieEventHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly TwitchApi _twitchApi;
        private readonly IConfiguration _config;
        private bool _messageSent = false;
        private bool _isCheckingStreams = false;
        private readonly SemaphoreSlim _checkStreamSemaphore = new SemaphoreSlim(1, 1);

        public KyuutieEventHandler(DiscordShardedClient client, IConfiguration config)
        {
            this._client = client;
            this._config = config;
            this._twitchApi = new TwitchApi($"{this._config["twitch-clientid"]}",
                $"{this._config["twitch-authtoken"]}");

            this._client.SessionCreated += OnSessionReadyAsync;
            this._client.SessionResumed += OnSessionReadyAsync;
        }

        private Task OnSessionReadyAsync(DiscordClient client, SessionReadyEventArgs e)
        {
            _ = StartStreamCheckAsync(client);
            return Task.CompletedTask;
        }

        private async Task StartStreamCheckAsync(DiscordClient client)
        {
            await _checkStreamSemaphore.WaitAsync();
            try
            {
                if (!_isCheckingStreams)
                {
                    _isCheckingStreams = true;
                    _ = CheckStreamsAsync(client);
                }
            }
            finally
            {
                _checkStreamSemaphore.Release();
            }
        }

        private async Task CheckStreamsAsync(DiscordClient client)
        {
            while (true)
            {
                SearchChannels channel = await this._twitchApi.SearchChannelsAsync("MikyuuVT");
                Console.WriteLine($"Name: {channel.display_name}");
                Console.WriteLine($"Stream title: {channel.title}");
                Console.WriteLine($"Streamer is live: {channel.is_live}");

                if (channel.is_live && !_messageSent)
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithAuthor($"{channel.display_name} is now live on Twitch!",
                            $"https://www.twitch.tv/{channel.display_name}",
                            $"{channel.thumbnail_url}")
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

                    _messageSent = true;
                }

                if (!channel.is_live && _messageSent)
                {
                    _messageSent = false;
                }

                await Task.Delay(10000); // Adjust the delay as needed
            }
        }
    }
}
