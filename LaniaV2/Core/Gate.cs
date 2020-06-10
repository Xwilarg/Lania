using Newtonsoft.Json;
using System.Collections.Generic;

namespace LaniaV2.Core
{
    public class Gate
    {
        public Gate()
        {
            _images = new Dictionary<ulong, Image>();
        }

        [JsonProperty]
        private Dictionary<ulong, Image> _images; // All images sent on this game
    }
}
