using System.Collections.Generic;
using Newtonsoft.Json;

namespace WDLT.PoESpy.Models
{
    public class ExileWebSocketMessage
    {
        [JsonProperty("auth")]
        public bool? Auth { get; set; }

        [JsonProperty("new")]
        public List<string> New { get; set; }
    }
}