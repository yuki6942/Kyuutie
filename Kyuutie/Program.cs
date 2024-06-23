using System;
using System.Collections.Generic;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kyuutie.Database;
using Kyuutie.Events;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace Kyuutie
{
    public class Program
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        // ReSharper disable once NotAccessedField.Local
        private static IServiceProvider _services;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        // ReSharper disable once InconsistentNaming
        public static async Task Main()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            ConfigurationBuilder builder = new();
            builder.AddUserSecrets<Program>();
            builder.AddEnvironmentVariables();
            IConfiguration config = builder.Build();

            DiscordShardedClient client = new(new DiscordConfiguration()
            {
                Token = config["DISCORD_TOKEN"]!,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true
            });

            await client.UseInteractivityAsync(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromSeconds(120)
            });

            ServiceCollection collection = new();
            collection.AddDbContextPool<KyuutieContext>(db =>
                db.UseNpgsql(config["DB_STRING"]));
            IServiceProvider services = collection.BuildServiceProvider();

            IReadOnlyDictionary<int, CommandsExtension> command =
                await client.UseCommandsAsync(
                    new CommandsConfiguration()
                    {
                        ServiceProvider = services,
                        RegisterDefaultCommandProcessors = false
                    }
                );
            _services = services;
            
            
            KyuutieEventHandler kyuutieEventHandler = new(client, config);
            
            foreach ((int _, CommandsExtension commandsExtension) in command)
            {
                
                commandsExtension.AddCommands(Assembly.GetExecutingAssembly(), 1128276411845726282);
                SlashCommandProcessor slashCommandProcessor = new();

                await commandsExtension.AddProcessorsAsync(slashCommandProcessor);
            }

            await client.StartAsync();
            await Task.Delay(-1);

        }
        
        
    }
}

