using Newtonsoft.Json;
using System.Collections.Generic;

namespace LaniaV2.Core
{
    public class Guild
    {
        public Guild(ulong guildId)
        {
            _prefix = "l.";
            _language = "en";
            _lastReceived = new Guild[2] { null, null };
            _gates = new Dictionary<ulong, Gate>();

            id = guildId.ToString();
        }

        public bool DoesGateExist(ulong id)
            => _gates.ContainsKey(id);

        public bool DidReachMaxLimitGate()
            => _gates.Count == 3;

        public int GetMaxLimitGate()
            => nbMax;

        public string GetLanguage()
            => _language;

        public void AddGate(ulong chanId)
            => _gates.Add(chanId, new Gate());

        public void RemoveGame(ulong chanId)
            => _gates.Remove(chanId);

        [JsonProperty]
        private string _prefix;
        [JsonProperty]
        private string _language;
        [JsonProperty]
        private Guild[] _lastReceived; // Guild from where the last image was received
        [JsonProperty]
        private Dictionary<ulong, Gate> _gates; // Chan id / Gate

        [JsonProperty]
        private string id; // Key for the db

        private const int nbMax = 3;
    }
}
