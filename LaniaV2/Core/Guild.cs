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

        public int GetNumberGates()
            => _gates.Count;

        /// <summary>
        /// Get all the guilds we can send messages to
        /// </summary>
        public List<(SocketTextChannel, Gate)> GetGates(bool isNsfw, out int totalRead, out int totalSend)
        {
            var safeGates = _gates.Where(x => !Program.P.client.GetGuild(ulong.Parse(id)).GetTextChannel(x.Key).IsNsfw).Select(x => (Program.P.client.GetGuild(ulong.Parse(id)).GetTextChannel(x.Key), x.Value)).ToList();
            var notSafeGates = _gates.Select(x => (Program.P.client.GetGuild(ulong.Parse(id)).GetTextChannel(x.Key), x.Value)).ToList();
            totalRead = isNsfw ? safeGates.Count : notSafeGates.Count;
            totalSend = isNsfw ? notSafeGates.Count : safeGates.Count;
            return isNsfw ? notSafeGates : safeGates;
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
        {
            _gates.Add(chanId, new Gate(id));
            Program.P.LaniaDb.UpdateGuild(this);
        }

        public void RemoveGate(ulong chanId)
        {
            _gates.Remove(chanId);
            Program.P.LaniaDb.UpdateGuild(this);
        }

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

        public Guild[] GetLastReceived()
        {
            List<Guild> lasts = new List<Guild>();
            foreach (var elem in _lastReceived)
            {
                if (elem != null)
                    lasts.Add(elem);
            }
            return lasts.ToArray();
        }

        /// <summary>
        /// Remove the id of a guild if it's in _lastReceived
        /// </summary>
        public void CleanLastReceived(string id)
        {
            for (int i = 0; i < _lastReceived.Length; i++)
            {
                if (_lastReceived[i].GetID() == id)
                {
                    _lastReceived[i] = null;
                    break;
                }
            }
        }

        public void AddLastReceived(Guild guild)
        {
            for (int i = 1; i < _lastReceived.Length; i++)
                _lastReceived[i - 1] = _lastReceived[i];
            _lastReceived[_lastReceived.Length - 1] = guild;
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

        public string GetID() => id;

        private DateTime _lastSent = DateTime.MinValue;
        private const int nbMax = 3;
        private const int secondsBetweenSend = 2; // We must wait X seconds between each images (so the users don't spam too much)
        public int GetSecondsBetweenSend() => secondsBetweenSend;
    }
}
