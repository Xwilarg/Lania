using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordUtils;
using Google.Cloud.Vision.V1;
using LaniaV2.Gate;
using LaniaV2.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LaniaV2
{
    public class Program
    {
        public static async Task Main()
               => await new Program().MainAsync();

        public readonly DiscordSocketClient client;
        private readonly CommandService commands = new CommandService();

        public DateTime StartTime { private set; get; }
        public static Program P { private set; get; }
        public Db.Db LaniaDb { private set; get; }

        public Core.GateManager Manager { private set; get; }

        private GateManager gate;

        // Translations
        public Dictionary<string, Dictionary<string, string>> Translations { private set; get; }
        public Dictionary<string, List<string>> TranslationKeyAlternate { private set; get; }

        private Program()
        {
            P = this;
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });
            client.Log += Utils.Log;
            commands.Log += Utils.LogError;
        }

        private async Task MainAsync()
        {
            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText("Keys/Credentials.json"));
            if (json.botToken == null)
                throw new NullReferenceException("Your Credentials.json is missing mandatory information, it must at least contains botToken");

            client.MessageReceived += HandleCommandAsync;
            client.GuildAvailable += GuildJoin;
            client.JoinedGuild += GuildJoin;

            await commands.AddModuleAsync<CommunicationModule>(null);
            await commands.AddModuleAsync<GateModule>(null);

            Manager = new Core.GateManager();

            LaniaDb = new Db.Db();
            await LaniaDb.InitAsync();

            Translations = new Dictionary<string, Dictionary<string, string>>();
            TranslationKeyAlternate = new Dictionary<string, List<string>>();
            Utils.InitTranslations(Translations, TranslationKeyAlternate, "../../../Lania-translations/Translations");

            if (!File.Exists("Keys/imageAPI.json"))
                throw new FileNotFoundException("Missing Keys/imageAPI.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Keys/imageAPI.json");
            gate = new GateManager();

            await client.LoginAsync(TokenType.Bot, (string)json.botToken);
            StartTime = DateTime.Now;
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task GuildJoin(SocketGuild guild)
        {
            await LaniaDb.InitGuildAsync(guild.Id);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage msg = arg as SocketUserMessage;
            if (msg == null || arg.Author.IsBot) return;
            int pos = 0;
            ITextChannel chan = msg.Channel as ITextChannel;
            if (chan != null)
            {
                var guild = Manager.GetGuild(chan.GuildId);
                if (guild.DoesGateExist(chan.Id))
                    _ = Task.Run(() => gate.SendToRandomGates(guild, msg));
            }
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos) || msg.HasStringPrefix("l.", ref pos))
            {
                SocketCommandContext context = new SocketCommandContext(client, msg);
                await commands.ExecuteAsync(context, pos, null);
            }
        }
    }
}