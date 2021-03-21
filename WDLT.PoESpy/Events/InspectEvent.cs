namespace WDLT.PoESpy.Events
{
    public class InspectEvent
    {
        public string Account { get; }
        public string League { get; }

        public InspectEvent(string account, string league)
        {
            Account = account;
            League = league;
        }
    }
}