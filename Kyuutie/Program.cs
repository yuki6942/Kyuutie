using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kyuutie.Database;
using Kyuutie.Events;
using System.Reflection;
using DSharpPlus.Commands;

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

            ServiceCollection collection = new();
            collection.AddDbContextPool<KyuutieContext>(db =>
                db.UseNpgsql(config["DB_STRING"]));
            IServiceProvider services = collection.BuildServiceProvider();

            IReadOnlyDictionary<int, CommandsExtension> command =
                await client.UseCommandsAsync(
                    new CommandsConfiguration()
                    {
                        ServiceProvider = services
                    }
                );
            _services = services;
            
            

            KyuutieEventHandler kyuutieEventHandler = new(services, client, config);

            foreach ((int _, CommandsExtension commandsExtension) in command)
            {
                commandsExtension.AddCommands(Assembly.GetExecutingAssembly(), 1128276411845726282);
                commandsExtension.CommandErrored += kyuutieEventHandler.OnErrorAsync;

            }

            await client.StartAsync();
            await Task.Delay(-1);

        }
        
        
    }
}

