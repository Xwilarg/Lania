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
            client.ReactionAdded += ReactionAdded;
            client.ReactionRemoved += ReactionRemoved;

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

        private async Task ManageReaction(bool addReaction, ISocketMessageChannel chan, SocketReaction react)
        {
            ulong guildId = (chan as ITextChannel).GuildId;
            if (Directory.Exists("Saves/Guilds/" + guildId) && File.Exists("Saves/Guilds/" + guildId + "/" + react.MessageId + ".dat"))
            {
                string[] content = File.ReadAllLines("Saves/Guilds/" + guildId + "/" + react.MessageId + ".dat");
                IUserMessage msg = (await client.GetGuild(Convert.ToUInt64(content[0])).GetTextChannel(Convert.ToUInt64(content[1])).GetMessageAsync(Convert.ToUInt64(content[2]))) as IUserMessage;
                string[] guilds = msg.Content.Split('#');
                int id = Convert.ToInt32(content[3]);
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Description = msg.Embeds.ToArray()[0].Description
                };
                int counter = 0;
                string emoteName = (react.Emote.ToString().Length < 4) ? (react.Emote.Name) : (":" + react.Emote.Name + ":");
                foreach (EmbedField field in msg.Embeds.ToArray()[0].Fields)
                {
                    if (counter == id)
                    {
                        bool found = false;
                        string finalStr = "";
                        foreach (string s in field.Value.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                        {
                            if (s == "(Nothing yet)")
                                continue;
                            string[] emote = s.Split(' ');
                            if (emote[0] == emoteName)
                            {
                                int newNb = Convert.ToInt32(emote[1].Substring(1, emote[1].Length - 1)) + ((addReaction) ? (1) : (-1));
                                if (newNb > 0)
                                    finalStr += emoteName + " x" + newNb + Environment.NewLine;
                                found = true;
                            }
                            else
                                finalStr += s + Environment.NewLine;
                        }
                        if (!found)
                            finalStr += emoteName + " x1" + Environment.NewLine;
                        if (finalStr == "")
                            finalStr = "(Nothing yet)";
                        embed.AddField(field.Name, finalStr);
                    }
                    else
                        embed.AddField(field.Name, field.Value);
                    counter++;
                }
                await msg.ModifyAsync(x => x.Embed = embed.Build());
                if (!Directory.Exists("Saves/Emotes"))
                    Directory.CreateDirectory("Saves/Emotes");
                if (!Directory.Exists("Saves/Emotes/" + content[0]))
                    Directory.CreateDirectory("Saves/Emotes/" + content[0]);
                if (File.Exists("Saves/Emotes/" + content[0] + "/" + emoteName.Replace(':', '-') + ".dat"))
                {
                    string nbStr = (Convert.ToInt32(File.ReadAllText("Saves/Emotes/" + content[0] + "/" + emoteName.Replace(':', '-') + ".dat")) + ((addReaction) ? (1) : (-1))).ToString();
                    File.WriteAllText("Saves/Emotes/" + content[0] + "/" + emoteName.Replace(':', '-') + ".dat", nbStr);
                }
                else
                    File.WriteAllText("Saves/Emotes/" + content[0] + "/" + emoteName.Replace(':', '-') + ".dat", "1");
            }
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cach, ISocketMessageChannel chan, SocketReaction react)
        {
            await ManageReaction(true, chan, react);
        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> cach, ISocketMessageChannel chan, SocketReaction react)
        {
            await ManageReaction(false, chan, react);
        }

        private async Task LeaveGuild(SocketGuild guild)
        {
            GateModule.Close(guild.Id);
        }

        public struct ImageData
        {
            public ImageData(ulong hostGuild, ulong hostChannel, ulong hostMessage, ulong destGuild, ulong destMessage)
            {
                this.hostGuild = hostGuild;
                this.hostChannel = hostChannel;
                this.hostMessage = hostMessage;
                this.destGuild = destGuild;
                this.destMessage = destMessage;
            }

            public ulong hostGuild;
            public ulong hostChannel;
            public ulong hostMessage;
            public ulong destGuild;
            public ulong destMessage;
        }

        private bool IsImage(string fileName)
        {
            string[] file = fileName.Split('.');
            string extension = file[file.Length - 1];
            return (extension == "jpg" || extension == "jpeg" || extension == "png"
                || extension == "gif");
        }

        public string GetFirstImage(string text)
        {
            string[] words = text.Split(' ');
            foreach (string s in words)
            {
                if (s.StartsWith("http://") || s.StartsWith("https://"))
                {
                    try
                    {
                        WebRequest request = WebRequest.Create(s);
                        request.Method = "HEAD";
                        request.GetResponse();
                        return (s);
                    }
                    catch (WebException)
                    { }
                }
            }
            return (null);
        }


        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null || arg.Author.Id == Sentences.myId) return;
            if (File.Exists("Saves/Guilds/" + (arg.Channel as ITextChannel).GuildId + ".dat")
                && File.ReadAllText("Saves/Guilds/" + (arg.Channel as ITextChannel).GuildId + ".dat") == arg.Channel.Id.ToString())
            {
                string url = null;
                if (msg.Attachments.Count > 0 && IsImage(msg.Attachments.ToArray()[0].Filename))
                    url = msg.Attachments.ToArray()[0].Url;
                if (url == null)
                {
                    url = GetFirstImage(msg.Content);
                    if (url != null && !IsImage(url))
                        url = null;
                    else
                        url = url.Substring(0, 8) + url.Substring(8, url.Length - 8).Replace("//", "/");
                }
                if (url != null)
                {
                    List<string> ids = new List<string>();
                    foreach (string f in Directory.GetFiles("Saves/Guilds"))
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
                    int i = 0;
                    for (; i < 3 && ids.Count > 0; i++)
                    {
                        int nb = rand.Next(ids.Count);
                        chans.Add(client.GetGuild(Convert.ToUInt64(ids[nb])).GetChannel(Convert.ToUInt64(File.ReadAllText("Saves/Guilds/" + ids[nb] + ".dat"))) as ITextChannel);
                        ids.RemoveAt(nb);
                    }
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Description = "Your file was sent to " + i + " guilds"
                    };
                    for (int y = 0; y < i; y++)
                        embed.AddField("#" + (y + 1), "(Nothing yet)");
                    ulong msgId = (await arg.Channel.SendMessageAsync("", false, embed.Build())).Id;
                    List<ImageData> datas = new List<ImageData>();
                    foreach (ITextChannel chan in chans)
                    {
                        datas.Add(new ImageData((arg.Channel as ITextChannel).GuildId, arg.Channel.Id, msgId, chan.GuildId, (await chan.SendMessageAsync("", false, new EmbedBuilder() { ImageUrl = url, Description = "You received an image though the gate." }.Build())).Id));
                    }
                    int counter = 0;
                    foreach (ImageData data in datas)
                    {
                        if (!Directory.Exists("Saves/Guilds/" + data.destGuild))
                            Directory.CreateDirectory("Saves/Guilds/" + data.destGuild);
                        File.WriteAllText("Saves/Guilds/" + data.destGuild + "/" + data.destMessage + ".dat", data.hostGuild + Environment.NewLine + data.hostChannel + Environment.NewLine + data.hostMessage + Environment.NewLine + counter);
                        counter++;
                    }
                }
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
