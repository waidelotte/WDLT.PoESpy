namespace WDLT.PoESpy.Events
{
    public class InspectEvent
    {
        public string Account { get; }

        public InspectEvent(string account)
        {
            Account = account;
        }
    }
}