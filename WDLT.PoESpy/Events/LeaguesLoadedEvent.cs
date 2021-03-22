using System.Collections.Generic;
using WDLT.Clients.POE.Models;

namespace WDLT.PoESpy.Events
{
    public class LeaguesLoadedEvent
    {
        public List<POELeague> Leagues { get; }

        public LeaguesLoadedEvent(List<POELeague> leagues)
        {
            Leagues = leagues;
        }
    }
}