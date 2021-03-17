using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stylet;
using WDLT.Clients.POE.Enums;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;
using WDLT.PoESpy.Models;
using WDLT.Utils.Extensions;

namespace WDLT.PoESpy.ViewModels
{
    public class MoneywayViewModel : BaseTabViewModel, IHandle<LeagueChangedEvent>
    {
        public BindableCollection<MoneywayItem> Items { get; }
        public List<EPOEOnlineStatus> OnlineStatus { get; }
        public EPOEOnlineStatus SelectedOnline { get; set; }

        private readonly ExileEngine _exileEngine;

        public MoneywayViewModel(IEventAggregator eventAggregator, ExileEngine exileEngine) : base(ETab.Moneyway, eventAggregator)
        {
            _exileEngine = exileEngine;
            Items = new BindableCollection<MoneywayItem>();

            OnlineStatus = new List<EPOEOnlineStatus>
            {
                EPOEOnlineStatus.Any,
                EPOEOnlineStatus.Online
            };
        }

        public Task Scan()
        {
            return LoadingTask(ScanTask);
        }

        public async Task ScanTask()
        {
            Items.Clear();

            await Search(true);
            await Search(false);
        }

        public void Inspect(MoneywayItem item)
        {
            EventAggregator.Publish(new InspectEvent(item.Account.Name));
        }

        public void Handle(LeagueChangedEvent message)
        {
            Items.Clear();
        }

        private async Task Search(bool useMirror)
        {
            var mirror = "Mirror of Kalandra";
            var exalt = "Exalted Orb";

            var search = await _exileEngine.SearchAsync(new POESearchPayload
            {
                Sort = new POESearchSort
                {
                    StackSize = EPOESort.Desc
                },
                Query = { Type = useMirror ? mirror : exalt, Status = { Option = SelectedOnline } }
            });

            if (search == null) return;
            
            foreach (var ids in search.Result.Take(50).Chunk(10))
            {
                var fetch = await _exileEngine.FetchAsync(ids);
                if (fetch == null) break;

                foreach (var sr in fetch.Result)
                {
                    var item = Items.FirstOrDefault(f => f.Account.Name == sr.Listing.Account.Name);

                    if (item == null)
                    {
                        item = new MoneywayItem(sr.Listing.Account);
                        Items.Add(item);
                    }

                    if (sr.Item.TypeLine == mirror)
                    {
                        item.MirrorCount += sr.Item.StackSize;
                    }
                    else if (sr.Item.TypeLine == exalt)
                    {
                        item.ExaltCount += sr.Item.StackSize;
                    }

                }

                await Task.Delay(1000);
            }
        }
    }
}