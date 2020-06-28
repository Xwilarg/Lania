using Newtonsoft.Json;
using System.Collections.Generic;

namespace LaniaV2.Core
{
    public class Gate
    {
        public Gate(string guildId)
        {
            _images = new Dictionary<ulong, Image>();
            _guildId = guildId;
        }

        [JsonProperty]
        private Dictionary<ulong, Image> _images; // All images sent on this game

        [JsonProperty]
        private string _guildId;

        public ulong GetGuildId() => ulong.Parse(_guildId);
    }
}
