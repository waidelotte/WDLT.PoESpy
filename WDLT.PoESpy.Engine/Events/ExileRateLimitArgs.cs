using System;
using WDLT.Clients.POE.Models;

namespace WDLT.PoESpy.Engine.Events
{
    public class ExileRateLimitArgs : EventArgs
    {
        public POERateLimit RateLimit { get; }

        public string Endpoint { get; }

        public ExileRateLimitArgs(POERateLimit rateLimit, string endpoint)
        {
            RateLimit = rateLimit;
            Endpoint = endpoint;
        }
    }
}