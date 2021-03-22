namespace WDLT.PoESpy.Events
{
    public class POESESSIDChangedEvent
    {
        public string NewValue { get; }

        public POESESSIDChangedEvent(string newValue)
        {
            NewValue = newValue;
        }
    }
}