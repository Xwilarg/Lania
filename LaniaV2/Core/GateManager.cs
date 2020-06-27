using Discord.WebSocket;
using Google.Cloud.Vision.V1;
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
            imageClient = ImageAnnotatorClient.Create();
        }

        public void SendToRandomGates(Guild guild, SocketUserMessage msg)
        {
            string url = GetImageUrl(msg);
            if (!IsUrlImage(url)) // URL is not an image
                return;
        }

        private string GetImageUrl(SocketUserMessage msg)
        {
            if (msg.Attachments.Count > 0)
                return msg.Attachments.ToArray()[0].Url;
            return msg.Content;
        }

        private bool IsUrlImage(string url)
        {
            var req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "HEAD";
            using (var resp = req.GetResponse())
            {
                return resp.ContentType.ToLower().StartsWith("image/");
            }
        }

        private ImageAnnotatorClient imageClient;

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
    }
}
