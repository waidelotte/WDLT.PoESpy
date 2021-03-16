using Stylet;
using WDLT.Clients.POE.Models;

namespace WDLT.PoESpy.Models
{
    public class MoneywayItem : PropertyChangedBase
    {
        public MoneywayItem(POEFetchAccount account)
        {
            Account = account;
        }

        public POEFetchAccount Account { get; }

        public long MirrorCount { get; set; }
        public long ExaltCount { get; set; }
    }
}