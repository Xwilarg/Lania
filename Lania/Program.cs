using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lania
{
    class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public readonly DiscordSocketClient client;
        private readonly IServiceCollection map = new ServiceCollection();
        private readonly CommandService commands = new CommandService();
        public static Program p;
        public Random rand;

        private int commandReceived;
        private string lastHourSent;

        private Program()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });
            client.Log += Log;
            commands.Log += Log;
        }

        private async Task MainAsync()
        {
            p = this;
            rand = new Random();

            lastHourSent = DateTime.Now.ToString("HH");
            if (File.Exists("Saves/CommandReceived.dat"))
            {
                string[] content = File.ReadAllLines("Saves/CommandReceived.dat");
                if (content[1] == lastHourSent)
                    commandReceived = Convert.ToInt32(content[0]);
                else
                    commandReceived = 0;
            }
            else
                commandReceived = 0;

            await commands.AddModuleAsync<CommunicationModule>();
            await commands.AddModuleAsync<GateModule>();

            client.MessageReceived += HandleCommandAsync;
            client.LeftGuild += LeaveGuild;

            await client.LoginAsync(TokenType.Bot, File.ReadAllText("Keys/token.dat"));
            await client.StartAsync();

            var task = Task.Run(async () => {
                for (;;)
                {
                    await Task.Delay(60000);
                    UpdateStatus();
                }
            });

            await Task.Delay(-1);
        }

        private async Task LeaveGuild(SocketGuild guild)
        {
            GateModule.Close(guild.Id);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null || arg.Author.Id == Sentences.myId) return;
            if (msg.Attachments.Count > 0 && File.Exists("Saves/" + (arg.Channel as ITextChannel).GuildId + ".dat")
                && File.ReadAllText("Saves/" + (arg.Channel as ITextChannel).GuildId + ".dat") == arg.Channel.Id.ToString())
            {
                string url = msg.Attachments.ToArray()[0].Url;
                string[] fileCut = msg.Attachments.ToArray()[0].Filename.Split('.');
                string fileName = "Image" + DateTime.Now.ToString("hhmmssff") + rand.Next(int.MinValue, int.MaxValue) + "." + fileCut[fileCut.Length - 1];
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(url, fileName);
                }
                List<string> ids = new List<string>();
                foreach (string f in Directory.GetFiles("Saves"))
                {
                    FileInfo fi = new FileInfo(f);
                    if (fi.Name.Split('.')[0] == (arg.Channel as ITextChannel).GuildId.ToString())
                        ;
                    else if (client.Guilds.ToList().Any(x => x.Id == Convert.ToUInt64(fi.Name.Split('.')[0])))
                        ids.Add(fi.Name.Split('.')[0]);
                    else
                        File.Delete(f);
                }
                List<ITextChannel> chans = new List<ITextChannel>();
                for (int i = 0; i < 3 && ids.Count > 0; i++)
                {
                    int nb = rand.Next(ids.Count);
                    chans.Add(client.GetGuild(Convert.ToUInt64(ids[nb])).GetChannel(Convert.ToUInt64(File.ReadAllText("Saves/" + ids[nb] + ".dat"))) as ITextChannel);
                    ids.RemoveAt(nb);
                }
                foreach (ITextChannel chan in chans)
                {
                    await chan.SendFileAsync(fileName);
                }
                File.Delete(fileName);
            }
            int pos = 0;
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos) || msg.HasStringPrefix("l.", ref pos))
            {
                var context = new SocketCommandContext(client, msg);
                IResult result = await commands.ExecuteAsync(context, pos);
                if (result.IsSuccess && !context.User.IsBot)
                {
                    commandReceived++;
                    File.WriteAllText("Saves/CommandReceived.dat", commandReceived + Environment.NewLine + lastHourSent);
                }
            }
        }

        private async void UpdateStatus()
        {
            HttpClient httpClient = new HttpClient();
            var values = new Dictionary<string, string> {
                           { "token", File.ReadAllLines("Keys/websiteToken.dat")[1] },
                           { "name", "Lania" }
                        };
            if (lastHourSent != DateTime.Now.ToString("HH"))
            {
                lastHourSent = DateTime.Now.ToString("HH");
                commandReceived = 0;
            }
            values.Add("serverCount", client.Guilds.Count.ToString());
            values.Add("nbMsgs", commandReceived.ToString());
            FormUrlEncodedContent content = new FormUrlEncodedContent(values);

            try
            {
                await httpClient.PostAsync(File.ReadAllLines("Keys/websiteToken.dat")[0], content);
            }
            catch (HttpRequestException)
            { }
            catch (TaskCanceledException)
            { }
        }

        private Task Log(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine(msg);
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }
    }
}
