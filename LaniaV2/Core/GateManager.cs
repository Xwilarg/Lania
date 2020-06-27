using Discord;
using Discord.WebSocket;
using Google.Cloud.Vision.V1;
using LaniaV2.Translations;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LaniaV2.Core
{
    public class GateManager
    {
        public GateManager()
        {
            _guilds = new Dictionary<ulong, Guild>();
            _bans = new Dictionary<ulong, string>();
            _imageClient = ImageAnnotatorClient.Create();
        }

        public async Task SendToRandomGates(Guild guild, ITextChannel chan, SocketUserMessage msg)
        {
            string url = GetImageUrl(msg);
            if (!IsUrlImage(url)) // URL is not an image
                return;
            var waitMsg = await chan.SendMessageAsync(Sentences.WaitMsg(chan.GuildId));
            if (IsBanned(msg.Author.Id)) // Sender is banned from using gates
                await waitMsg.ModifyAsync(x => x.Content = Sentences.IsBannedImage(chan.GuildId));
            else
            {
                bool isNsfw = chan.IsNsfw;
                if (!await IsSfw(url, isNsfw))
                    await waitMsg.ModifyAsync(x => x.Content = Sentences.NsfwImage(chan.GuildId) + (isNsfw ? " " + Sentences.WrongNsfw(chan.GuildId) : ""));
                else if (!guild.CanSend())
                    await waitMsg.ModifyAsync(x => x.Content = Sentences.WaitImage(chan.GuildId, guild.GetSecondsBetweenSend()));
                else
                {
                    guild.UpdateSendTime();
                }
            }
        }

        /// <summary>
        /// Get 5 random gates
        /// </summary>
        private List<(SocketTextChannel, Gate)> GetRandomGates(ulong guildId, bool isNsfw, out int readAvailables, out int sendAvailable, out int chanReadAvailable, out int chanSendAvailable)
        {
            readAvailables = 0;
            sendAvailable = 0;
            chanReadAvailable = 0;
            chanSendAvailable = 0;
            List<(SocketTextChannel, Gate)> allGates = new List<(SocketTextChannel, Gate)>();
            foreach (var g in _guilds)
            {
                if (g.Key == guildId) // We ignore our guild
                    continue;
                g.Value.CleanGates().GetAwaiter().GetResult();
                if (g.Value.HaveAnyGate()) // If there is any gate opened in the guild
                {
                    // TODO: FIX
                    readAvailables++; // + 1 guild avalable
                    var gates = g.Value.GetGates(isNsfw, out int totalCount);
                    chanReadAvailable += totalCount; // Add all the gates count
                    chanSendAvailable += gates.Count; // GetGates returns all the gates we can send to
                    if (gates.Count > 1)
                    {
                        sendAvailable++; // If there is any gate we can send to, that means we can send to this guild
                    }
                    allGates.Add(gates[Program.P.Rand.Next(0, gates.Count)]);
                }
            }
            List<(SocketTextChannel, Gate)> finalGuilds = new List<(SocketTextChannel, Gate)>();
            while (finalGuilds.Count < 5)
            {
                var tmp = allGates[Program.P.Rand.Next(0, allGates.Count];
                allGates.Remove(tmp);
                finalGuilds.Add(tmp);
            }
            return finalGuilds;
        }

        private async Task<bool> IsSfw(string url, bool isChanNsfw)
        {
            var image = await Google.Cloud.Vision.V1.Image.FetchFromUriAsync(url);
            SafeSearchAnnotation response = await _imageClient.DetectSafeSearchAsync(image);
            if (isChanNsfw)
                return (int)response.Medical < 3 && (int)response.Violence < 3;
            return (int)response.Adult < 3 && (int)response.Medical < 3 && (int)response.Violence < 3;
        }

        private string GetImageUrl(SocketUserMessage msg)
        {
            if (msg.Attachments.Count > 0)
                return msg.Attachments.ToArray()[0].Url;
            return msg.Content;
        }

        private bool IsUrlImage(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "HEAD";
            using (var resp = req.GetResponse())
            {
                return resp.ContentType.ToLower().StartsWith("image/");
            }
        }

        public void AddGuild(ulong id, Guild guild)
            => _guilds.Add(id, guild);

        public async Task AddBan(ulong id, string reason)
        {
            await Program.P.LaniaDb.AddBan(id, reason);
            _bans.Add(id, reason);
        }

        /// <summary>
        /// Called from db to init ban list
        /// </summary>
        public void AddBanFromDb(ulong id, string reason)
            => _bans.Add(id, reason);

        public string GetLanguage(ulong id)
            => _guilds[id].GetLanguage();

        public bool IsBanned(ulong id)
            => _bans.ContainsKey(id);

        public Guild GetGuild(ulong id)
            => _guilds[id];

        private Dictionary<ulong, Guild> _guilds;
        private Dictionary<ulong, string> _bans; // List of people banned with the reason of their ban
        private ImageAnnotatorClient _imageClient;
    }
}
