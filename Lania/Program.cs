﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Cloud.Vision.V1;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceCollection map = new ServiceCollection();
        private readonly CommandService commands = new CommandService();
        public static Program p;
        public Random rand;

        public DateTime startTime;

        private int commandReceived;
        private string lastHourSent;

        private Dictionary<ulong, DateTime> timeLastSent;
        private const int minutesBetweenSend = 2;

        private RavenClient ravenClient;
        private ImageAnnotatorClient imageClient;

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

            ravenClient = new RavenClient(File.ReadAllText("Keys/raven.dat"));
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Keys/imageAPI.json");
            imageClient = ImageAnnotatorClient.Create();

            timeLastSent = new Dictionary<ulong, DateTime>();

            await commands.AddModuleAsync<CommunicationModule>();
            await commands.AddModuleAsync<GateModule>();

            client.MessageReceived += HandleCommandAsync;
            client.LeftGuild += LeaveGuild;
            client.ReactionAdded += ReactionAdded;
            client.ReactionRemoved += ReactionRemoved;

            startTime = DateTime.Now;
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
                    embed.AddField(EditField(field, emoteName, addReaction, counter, id));
                    counter++;
                }
                await msg.ModifyAsync(x => x.Embed = embed.Build());
                SaveEmoteToFile(content[0], emoteName, addReaction);
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

        /// <summary>
        /// Save emote progress in file
        /// </summary>
        /// <param name="guildId">sender guild id</param>
        /// <param name="emoteName">name of the emote</param>
        /// <param name="addReaction">should add or remove emote ?</param>
        private void SaveEmoteToFile(string guildId, string emoteName, bool addReaction)
        {
            if (!Directory.Exists("Saves/Emotes"))
                Directory.CreateDirectory("Saves/Emotes");
            if (!Directory.Exists("Saves/Emotes/" + guildId))
                Directory.CreateDirectory("Saves/Emotes/" + guildId);
            if (File.Exists("Saves/Emotes/" + guildId + "/" + emoteName.Replace(':', '-') + ".dat"))
            {
                string nbStr = (Convert.ToInt32(File.ReadAllText("Saves/Emotes/" + guildId + "/" + emoteName.Replace(':', '-') + ".dat")) + ((addReaction) ? (1) : (-1))).ToString();
                File.WriteAllText("Saves/Emotes/" + guildId + "/" + emoteName.Replace(':', '-') + ".dat", nbStr);
            }
            else
                File.WriteAllText("Saves/Emotes/" + guildId + "/" + emoteName.Replace(':', '-') + ".dat", "1");
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
        private List<ITextChannel> SendImages(List<string> ids, bool isNsfw)
        {
            List<ITextChannel> chans = new List<ITextChannel>();
            for (int i = 0; i < 3 && ids.Count > 0; i++)
            {
                int nb = rand.Next(ids.Count);
                chans.Add(client.GetGuild(Convert.ToUInt64(ids[nb])).GetChannel(Convert.ToUInt64(File.ReadAllText("Saves/Guilds/" + ids[nb] + ".dat"))) as ITextChannel);
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
        private async Task SendImageToServer(List<string> ids, SocketMessage arg, string url)
        {
            List<ITextChannel> chans = SendImages(ids, (arg.Channel as ITextChannel).IsNsfw);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Description = "Your file was sent to " + chans.Count + " guilds"
            };
            for (int y = 0; y < chans.Count; y++)
                embed.AddField("#" + (y + 1), "(Nothing yet)");
            ulong msgId = (await arg.Channel.SendMessageAsync("", false, embed.Build())).Id;
            List<ImageData> datas = new List<ImageData>();
            foreach (ITextChannel chan in chans)
            {
                ulong msgDest = (await chan.SendMessageAsync("", false, new EmbedBuilder() { ImageUrl = url, Description = "You received an image though the gate." }.Build())).Id;
                ITextChannel textChan = (arg.Channel as ITextChannel);
                datas.Add(new ImageData(textChan.GuildId, arg.Channel.Id, msgId, chan.GuildId, chan.Id, msgDest, textChan.IsNsfw));
            }
            int counter = 0;
            foreach (ImageData data in datas)
            {
                if (!Directory.Exists("Saves/Guilds/" + data.destGuild))
                    Directory.CreateDirectory("Saves/Guilds/" + data.destGuild);
                File.WriteAllText("Saves/Guilds/" + data.destGuild + "/" + data.destMessage + ".dat", data.hostGuild + Environment.NewLine + data.hostChannel + Environment.NewLine + data.hostMessage + Environment.NewLine + counter);
                File.WriteAllText("Saves/Guilds/" + data.destGuild + "/last.dat", arg.Author.Id + Environment.NewLine + url
                    + Environment.NewLine + data.destChannel + Environment.NewLine + data.destMessage + Environment.NewLine + data.isChanNsfw);
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

        public static List<string> GetNbChans(ulong guildId, bool isNsfw, out int total, out int readAvailable)
        {
            readAvailable = 0;
            List<string> ids = new List<string>();
            foreach (string f in Directory.GetFiles("Saves/Guilds"))
            {
                FileInfo fi = new FileInfo(f);
                SocketGuild guild = Program.p.client.Guilds.ToList().Find(x => x.Id == Convert.ToUInt64(fi.Name.Split('.')[0]));
                ITextChannel chan = (guild != null) ? (guild.GetTextChannel(Convert.ToUInt64(File.ReadAllText(f)))) : (null);
                if (fi.Name.Split('.')[0] == guildId.ToString())
                { }
                else if (guild != null && chan != null && ((isNsfw && chan.IsNsfw) || !isNsfw))
                    ids.Add(fi.Name.Split('.')[0]);
                else if (guild == null)
                    File.Delete(f);
                else if (guild != null && chan != null && isNsfw)
                    readAvailable++;
            }
            readAvailable += ids.Count;
            total = Directory.GetFiles("Saves/Guilds").Length;
            return (ids);
        }

        /// <summary>
        /// Check images in message and send them in gate if necessary
        /// </summary>
        /// <param name="arg">SocketMessage received from HandleCommandAsync</param>
        /// <param name="msg">User message</param>
        private async Task SendMessageGate(SocketMessage arg, SocketUserMessage msg)
        {
            string url = GetImageUrl(msg);
            if (url != null)
            {
                if (!GateModule.IsBanned(arg.Author.Id))
                {
                    bool isNsfw = (arg.Channel as ITextChannel).IsNsfw;
                    if (await IsSfw(url, isNsfw))
                    {
                        ulong guildId = (arg.Channel as ITextChannel).GuildId;
                        TimeSpan? waitValue = CanSendImage(guildId);
                        if (waitValue == null || waitValue.Value.TotalSeconds < 0)
                        {
                            if (timeLastSent.ContainsKey(guildId))
                                timeLastSent[guildId] = DateTime.Now;
                            else
                                timeLastSent.Add(guildId, DateTime.Now);
                            List<string> ids = GetNbChans(guildId, isNsfw, out _, out _);
                            if (ids.Count == 0)
                                await arg.Channel.SendMessageAsync(Sentences.noChan);
                            else
                                await SendImageToServer(ids, arg, url);
                        }
                        else
                            await arg.Channel.SendMessageAsync(Sentences.WaitImage(TimeSpanToString(waitValue.Value)));
                    }
                    else
                        await arg.Channel.SendMessageAsync(Sentences.nsfwImage + ((isNsfw) ? (" " + Sentences.wrongNsfw) : ("")));
                }
                else
                    await arg.Channel.SendMessageAsync(Sentences.isBannedImage);
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
            if (File.Exists("Saves/Guilds/" + (arg.Channel as ITextChannel).GuildId + ".dat")
                && File.ReadAllText("Saves/Guilds/" + (arg.Channel as ITextChannel).GuildId + ".dat") == arg.Channel.Id.ToString())
                _ = Task.Run(async delegate () { await SendMessageGate(arg, msg); });
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

        private Task LogError(LogMessage msg)
        {
            ravenClient.Capture(new SentryEvent(msg.Exception));
            return Task.CompletedTask;
        }
    }
}
