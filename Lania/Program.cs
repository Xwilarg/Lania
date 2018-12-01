using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Google.Cloud.Vision.V1;
using SharpRaven;
using SharpRaven.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lania
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public readonly DiscordSocketClient client;
        private readonly CommandService commands = new CommandService();
        public static Program p;
        public Random rand;

        public DateTime startTime;

        private Dictionary<ulong, DateTime> timeLastSent;
        private const int minutesBetweenSend = 2;

        private RavenClient ravenClient;
        private ImageAnnotatorClient imageClient;

        private Db db;

        private Dictionary<string, Dictionary<string, string>> translations;
        private Dictionary<string, List<string>> translationKeyAlternate;

        public Dictionary<string, Dictionary<string, string>> GetTranslations()
        {
            return (translations);
        }

        public Dictionary<string, List<string>> GetTranslationKeyAlternate()
        {
            return (translationKeyAlternate);
        }

        public Dictionary<ulong, string> GetLanguages()
        {
            return (languages);
        }

        private Dictionary<ulong, string> languages;

        public Db GetDb()
        {
            return (db);
        }

        private Program()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });
            client.Log += Log;
            commands.Log += LogError;
        }

        private async Task MainAsync()
        {
            db = new Db();
            await db.InitAsync();

            translationKeyAlternate = new Dictionary<string, List<string>>();
            translations = new Dictionary<string, Dictionary<string, string>>();
            languages = new Dictionary<ulong, string>();
            Translation.Init(translations, translationKeyAlternate);

            p = this;
            rand = new Random();

            if (File.Exists("Keys/raven.dat"))
                ravenClient = new RavenClient(File.ReadAllText("Keys/raven.dat"));
            else
                ravenClient = null;
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Keys/imageAPI.json");
            imageClient = ImageAnnotatorClient.Create();

            timeLastSent = new Dictionary<ulong, DateTime>();

            await commands.AddModuleAsync<CommunicationModule>();
            await commands.AddModuleAsync<GateModule>();

            client.MessageReceived += HandleCommandAsync;
            client.LeftGuild += LeaveGuild;
            client.ReactionAdded += ReactionAdded;
            client.ReactionRemoved += ReactionRemoved;
            client.GuildAvailable += GuildAvailable;
            client.JoinedGuild += GuildAvailable;

            startTime = DateTime.Now;
            await client.LoginAsync(TokenType.Bot, File.ReadAllText("Keys/token.dat"));
            await client.StartAsync();

            if (File.Exists("Keys/websiteToken.dat"))
            {
                var task = Task.Run(async () => {
                    for (;;)
                    {
                        await Task.Delay(60000);
                        UpdateStatus();
                    }
                });
            }

            await Task.Delay(-1);
        }

        private async Task GuildAvailable(SocketGuild arg)
        {
            await db.InitGuild(arg.Id);
            languages.Add(arg.Id, await db.GetLanguage(arg.Id));
        }

        private async Task ManageReaction(bool addReaction, ISocketMessageChannel chan, SocketReaction react)
        {
            ulong guildId = (chan as ITextChannel).GuildId;
            string message = await db.GetImage(guildId, react.MessageId);
            if (message != null)
            {
                string[] content = message.Split('|');
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
                    embed.AddField(EditField(field, emoteName, addReaction, counter, id));
                    counter++;
                }
                await msg.ModifyAsync(x => x.Embed = embed.Build());
                await db.AddReaction(ulong.Parse(content[0]), emoteName, ((addReaction) ? (1) : (-1)));
            }
        }

        private EmbedFieldBuilder EditField(EmbedField field, string emoteName, bool addReaction, int counter, int id)
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
                return (new EmbedFieldBuilder()
                {
                    Name = field.Name,
                    Value = finalStr
                });
            }
            return (new EmbedFieldBuilder()
            {
                Name = field.Name,
                Value = field.Value
            });
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
            await GateModule.Close(guild.Id);
        }

        public struct ImageData
        {
            public ImageData(ulong hostGuild, ulong hostChannel, ulong hostMessage, ulong destGuild, ulong destChannel, ulong destMessage, bool isChanNsfw)
            {
                this.hostGuild = hostGuild;
                this.hostChannel = hostChannel;
                this.hostMessage = hostMessage;
                this.destGuild = destGuild;
                this.destChannel = destChannel;
                this.destMessage = destMessage;
                this.isChanNsfw = isChanNsfw;
            }

            public ulong hostGuild;
            public ulong hostChannel;
            public ulong hostMessage;
            public ulong destGuild;
            public ulong destChannel;
            public ulong destMessage;
            public bool isChanNsfw;
        }

        /// <summary>
        /// Send the image to the guilds
        /// </summary>
        /// <param name="ids">ids of all guilds available</param>
        private List<ITextChannel> SendImages(List<string> ids, bool isNsfw, ulong guildId, out bool isLast)
        {
            List<ITextChannel> chans = new List<ITextChannel>();
            string lastcontent = db.GetLast(guildId).GetAwaiter().GetResult();
            if (lastcontent != null)
            {
                string[] content = lastcontent.Split('|');
                if (content.Length > 5)
                {
                    string guild = content[5];
                    ulong destGuildId = ulong.Parse(guild);
                    if (db.DoesGuildExist(destGuildId).GetAwaiter().GetResult())
                    {
                        chans.Add(client.GetGuild(destGuildId).GetTextChannel(Convert.ToUInt64(db.GetGateChan(destGuildId).GetAwaiter().GetResult())));
                        ids.Remove(guild);
                        isLast = true;
                    }
                    else
                        isLast = false;
                }
                else
                    isLast = false;
            }
            else
                isLast = false;
            for (int i = chans.Count; i < 3 && ids.Count > 0; i++)
            {
                int nb = rand.Next(ids.Count);
                ulong destGuildId = ulong.Parse(ids[nb]);
                chans.Add(client.GetGuild(destGuildId).GetTextChannel(Convert.ToUInt64(db.GetGateChan(destGuildId).GetAwaiter().GetResult())));
                ids.RemoveAt(nb);
            }
            return (chans);
        }

        /// <summary>
        /// Send image to 3 random guilds and write emote status in channel
        /// </summary>
        /// <param name="ids">ids of all guilds available</param>
        /// <param name="arg">SocketMessage got by HandleCommandAsync</param>
        /// <param name="url">url to image</param>
        private async Task SendImageToServer(List<string> ids, SocketMessage arg, string url, ulong guildId, RestUserMessage waitMsg)
        {
            bool isLast;
            List<ITextChannel> chans = SendImages(ids, (arg.Channel as ITextChannel).IsNsfw, guildId, out isLast);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Description = Sentences.FileSent(chans.Count)
            };
            for (int y = 0; y < chans.Count; y++)
                embed.AddField("#" + (y + 1) + ((y == 0 && isLast) ? (" " + Sentences.LastImage(guildId)) : ("")), Sentences.NothingYet(guildId));
            ulong msgId = waitMsg.Id;
            await waitMsg.ModifyAsync((x) => { x.Content = ""; x.Embed = embed.Build(); });
            List<ImageData> datas = new List<ImageData>();
            foreach (ITextChannel chan in chans)
            {
                ulong msgDest = (await chan.SendMessageAsync("", false, new EmbedBuilder() {
                    ImageUrl = url,
                    Description = Sentences.ImageReceived(guildId),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = Sentences.EmoteHelp(guildId)
                    }
                }.Build())).Id;
                ITextChannel textChan = (arg.Channel as ITextChannel);
                datas.Add(new ImageData(textChan.GuildId, arg.Channel.Id, msgId, chan.GuildId, chan.Id, msgDest, textChan.IsNsfw));
            }
            int counter = 0;
            foreach (ImageData data in datas)
            {
                await db.SendImage(data, counter, url, guildId);
                counter++;
            }
        }

        /// <summary>
        /// Check if a file is an image by it extension
        /// </summary>
        /// <param name="fileName">file name to check</param>
        public static bool IsImage(string fileName)
        {
            string[] file = fileName.Split('.');
            string extension = file[file.Length - 1];
            return (extension == "jpg" || extension == "jpeg" || extension == "png"
                || extension == "gif");
        }

        /// <summary>
        /// Get first url in a text
        /// </summary>
        /// <param name="text">text to search in</param>
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

        /// <summary>
        /// Get url contained in user message
        /// </summary>
        /// <param name="msg">User message</param>
        private string GetImageUrl(SocketUserMessage msg)
        {
            string url = null;
            if (msg.Attachments.Count > 0 && IsImage(msg.Attachments.ToArray()[0].Filename))
                url = msg.Attachments.ToArray()[0].Url;
            if (url == null)
            {
                url = GetFirstImage(msg.Content);
                if (url != null && !IsImage(url))
                    url = null;
                else if (url != null)
                    url = url.Substring(0, 8) + url.Substring(8, url.Length - 8).Replace("//", "/");
            }
            return (url);
        }

        /// <summary>
        /// Check images in message and send them in gate if necessary
        /// </summary>
        /// <param name="arg">SocketMessage received from HandleCommandAsync</param>
        /// <param name="msg">User message</param>
        private async Task SendMessageGate(SocketMessage arg, SocketUserMessage msg)
        {
            try
            {
                string url = GetImageUrl(msg);
                if (url != null)
                {
                    ulong guildId = (arg.Channel as ITextChannel).GuildId;
                    RestUserMessage waitMsg = await arg.Channel.SendMessageAsync(Sentences.WaitMsg(guildId));
                    if (!await db.IsBan(arg.Author.Id.ToString()))
                    {
                        bool isNsfw = (arg.Channel as ITextChannel).IsNsfw;
                        if (await IsSfw(url, isNsfw))
                        {
                            TimeSpan? waitValue = CanSendImage(guildId);
                            if (waitValue == null || waitValue.Value.TotalSeconds < 0)
                            {
                                if (timeLastSent.ContainsKey(guildId))
                                    timeLastSent[guildId] = DateTime.Now;
                                else
                                    timeLastSent.Add(guildId, DateTime.Now);
                                List<string> ids = db.GetAllGuilds(guildId, isNsfw, out _, out _);
                                if (ids.Count == 0)
                                    await waitMsg.ModifyAsync(x => x.Content = Sentences.NoChan(guildId));
                                else
                                    await SendImageToServer(ids, arg, url, guildId, waitMsg);
                            }
                            else
                                await waitMsg.ModifyAsync(x => x.Content = Sentences.WaitImage(guildId, TimeSpanToString(waitValue.Value)));
                        }
                        else
                            await waitMsg.ModifyAsync(x => x.Content = Sentences.NsfwImage(guildId) + ((isNsfw) ? (" " + Sentences.WrongNsfw(guildId)) : ("")));
                    }
                    else
                        await waitMsg.ModifyAsync(x => x.Content = Sentences.IsBannedImage(guildId));
                    await IncreaseCommandReceived();
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async Task<bool> IsSfw(string url, bool isChanNsfw)
        {
            var image = await Google.Cloud.Vision.V1.Image.FetchFromUriAsync(url);
            SafeSearchAnnotation response = await imageClient.DetectSafeSearchAsync(image);
            if (isChanNsfw)
                return ((int)response.Medical < 3 && (int)response.Violence < 3);
            return ((int)response.Adult < 3 && (int)response.Medical < 3 && (int)response.Violence < 3);
        }

        private TimeSpan? CanSendImage(ulong guildId)
        {
            if (timeLastSent.ContainsKey(guildId))
                return (timeLastSent[guildId].AddMinutes(minutesBetweenSend).Subtract(DateTime.Now));
            return (null);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage msg = arg as SocketUserMessage;
            if (msg == null || arg.Author.Id == Sentences.myId || arg.Author.IsBot) return;
            if (await db.CompareChannel((arg.Channel as ITextChannel).GuildId, arg.Channel.Id))
                _ = Task.Run(async delegate () { await SendMessageGate(arg, msg); });
            int pos = 0;
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos) || msg.HasStringPrefix("l.", ref pos))
            {
                var context = new SocketCommandContext(client, msg);
                IResult result = await commands.ExecuteAsync(context, pos);
                if (result.IsSuccess)
                    await IncreaseCommandReceived();
            }
        }

        private async Task IncreaseCommandReceived()
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("nbMsgs", "1") });
        }

        /// <summary>
        /// Return a string given a TimeSpan
        /// </summary>
        /// <param name="ts">The TimeSpan to transform</param>
        /// <returns>The string wanted</returns>
        public static string TimeSpanToString(TimeSpan ts)
        {
            string finalStr = ts.Seconds + " seconds";
            if (ts.Days > 0)
                finalStr = ts.Days.ToString() + " days, " + ts.Hours.ToString() + " hours, " + ts.Minutes.ToString() + " minutes and " + finalStr;
            else if (ts.Hours > 0)
                finalStr = ts.Hours.ToString() + " hours, " +  ts.Minutes.ToString() + " minutes and " + finalStr;
            else if (ts.Minutes > 0)
                finalStr = ts.Minutes.ToString() + " minutes and " + finalStr;
            return (finalStr);
        }

        private async Task UpdateElement(Tuple<string, string>[] elems)
        {
            HttpClient httpClient = new HttpClient();
            var values = new Dictionary<string, string> {
                           { "token", File.ReadAllLines("Keys/websiteToken.dat")[1] },
                           { "action", "add" },
                           { "name", "Lania" }
                        };
            foreach (var elem in elems)
            {
                values.Add(elem.Item1, elem.Item2);
            }
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, File.ReadAllLines("Keys/websiteToken.dat")[0]);
            msg.Content = new FormUrlEncodedContent(values);

            try
            {
                await httpClient.SendAsync(msg);
            }
            catch (HttpRequestException)
            { }
            catch (TaskCanceledException)
            { }
        }

        private async void UpdateStatus()
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("serverCount", client.Guilds.Count.ToString()) });
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

        private Task LogError(LogMessage msg)
        {
            if (ravenClient == null)
                Log(msg);
            else
                ravenClient.Capture(new SentryEvent(msg.Exception));
            CommandException ce = msg.Exception as CommandException;
            if (ce != null)
            {
                ce.Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = msg.Exception.InnerException.GetType().ToString(),
                    Description = Sentences.Error(ce.Context.Guild.Id, msg.Exception.InnerException.Message)
                }.Build());
            }
            return Task.CompletedTask;
        }
    }
}
