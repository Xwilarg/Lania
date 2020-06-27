using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public bool HaveAnyGate()
            => _gates.Count > 0;

        /// <summary>
        /// Get all the guilds we can send messages to
        /// </summary>
        public List<(SocketTextChannel, Gate)> GetGates(bool isNsfw, out int totalCount)
        {
            totalCount = _gates.Count; // Number of gates we can read from
            if (isNsfw) // If our channel is NSFW we can send everywhere
            {
                return _gates.Select(x => (Program.P.client.GetGuild(ulong.Parse(id)).GetTextChannel(x.Key), x.Value)).ToList();
            }
            // If our channel is not NSFW we only get safe channels
            return _gates.Where(x =>
            {
                var textChan = Program.P.client.GetGuild(ulong.Parse(id)).GetTextChannel(x.Key);
                return !textChan.IsNsfw;
            }).Select(x => (Program.P.client.GetGuild(ulong.Parse(id)).GetTextChannel(x.Key), x.Value)).ToList();
        }


        public bool DoesGateExist(ulong chanId)
            => _gates.ContainsKey(chanId);

        public bool DidReachMaxLimitGate()
            => _gates.Count == 3;

        public Dictionary<ulong, Gate> GetGates()
            => _gates;

        public int GetMaxLimitGate()
            => nbMax;

        public string GetLanguage()
            => _language;

        public void AddGate(ulong chanId)
            => _gates.Add(chanId, new Gate());

        public void RemoveGate(ulong chanId)
            => _gates.Remove(chanId);

        public bool CanSend()
            => _lastSent.AddSeconds(secondsBetweenSend).Subtract(DateTime.Now).TotalSeconds < 0;

        public void UpdateSendTime()
            => _lastSent = DateTime.Now;

        /// <summary>
        /// Close gates that are in deleted channels
        /// </summary>
        public async Task CleanGates()
        {
            List<ulong> toRemove = new List<ulong>();
            foreach (var gate in _gates)
            {
                if (Program.P.client.GetGuild(ulong.Parse(id)).GetTextChannel(gate.Key) == null)
                {
                    toRemove.Add(gate.Key);
                }
            }
            foreach (var id in toRemove)
            {
                _gates.Remove(id);
            }
            if (toRemove.Count != 0)
            {
                await Program.P.LaniaDb.UpdateGuild(this);
            }
        }

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

        private DateTime _lastSent = DateTime.MinValue;
        private const int nbMax = 3;
        private const int secondsBetweenSend = 2; // We must wait X seconds between each images (so the users don't spam too much)
        public int GetSecondsBetweenSend() => secondsBetweenSend;
    }
}
