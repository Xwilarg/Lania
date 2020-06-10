using System.Collections.Generic;

namespace LaniaV2.Core
{
    public class GateManager
    {
        public GateManager()
        {
            _guilds = new Dictionary<ulong, Guild>();
            _bans = new Dictionary<ulong, string>();
        }

        public void AddGuild(ulong id, Guild guild)
            => _guilds.Add(id, guild);

        public void AddBan(ulong id, string reason)
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
