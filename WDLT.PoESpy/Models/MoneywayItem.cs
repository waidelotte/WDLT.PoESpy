using Stylet;
using WDLT.Clients.POE.Models;

namespace WDLT.PoESpy.Models
{
    public class MoneywayItem : PropertyChangedBase
    {
        public MoneywayItem(POEFetchAccount account, string league)
        {
            Account = account;
            League = league;
        }

        public POEFetchAccount Account { get; }

        public string League { get; }
        public long MirrorCount { get; set; }
        public long ExaltCount { get; set; }
    }
}