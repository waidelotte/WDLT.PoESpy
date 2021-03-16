using System.Collections.Generic;

namespace WDLT.PoESpy.Models
{
    public class TradeSpyResult
    {
        public List<string> Messages { get; }

        public TradeSpyResult()
        {
            Messages = new List<string>();
        }
    }
}