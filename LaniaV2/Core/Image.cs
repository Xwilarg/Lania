using Newtonsoft.Json;
using System.Collections.Generic;

namespace LaniaV2.Core
{
    public class Image
    {
        public Image(Gate owner)
        {
            _owner = owner;
            _emotes = new Dictionary<string, int>();
        }

        [JsonProperty]
        private Gate _owner;
        [JsonProperty]
        private Dictionary<string, int> _emotes;
    }
}
