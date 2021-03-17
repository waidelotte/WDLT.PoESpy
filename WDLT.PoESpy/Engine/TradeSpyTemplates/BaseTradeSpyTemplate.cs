using WDLT.Clients.POE.Models;

namespace WDLT.PoESpy.Engine.TradeSpyTemplates
{
    public abstract class BaseTradeSpyTemplate
    {
        public string Name { get; }

        protected BaseTradeSpyTemplate(string name)
        {
            Name = name;
        }

        public abstract POESearchPayload CreatePayload();

        public abstract bool IsGood(POEFetchResult fetch);
    }
}