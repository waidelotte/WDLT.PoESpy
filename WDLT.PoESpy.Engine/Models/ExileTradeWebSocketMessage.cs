using System.Collections.Generic;
using Newtonsoft.Json;

namespace WDLT.PoESpy.Engine.Models
{
    public class ExileTradeWebSocketMessage
    {
        [JsonProperty("auth")]
        public bool? Auth { get; set; }

        [JsonProperty("new")]
        public List<string> New { get; set; }
    }
}