using WDLT.PoESpy.Enums;

namespace WDLT.PoESpy.Events
{
    public class ActivateTabEvent
    {
        public ETab Tab { get; }

        public ActivateTabEvent(ETab tab)
        {
            Tab = tab;
        }
    }
}