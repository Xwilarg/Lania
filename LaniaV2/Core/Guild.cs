using Newtonsoft.Json;
using System.Collections.Generic;

namespace LaniaV2.Core
{
    public class Guild
    {
        public Guild()
        {
            _prefix = "l.";
            _language = "en";
            _lastReceived = new Guild[2] { null, null };
            _gates = new List<Gate>();
        }

        public string GetLanguage()
            => _language;

        [JsonProperty]
        private string _prefix;
        [JsonProperty]
        private string _language;
        [JsonProperty]
        private Guild[] _lastReceived; // Guild from where the last image was received
        [JsonProperty]
        private List<Gate> _gates;
    }
}
