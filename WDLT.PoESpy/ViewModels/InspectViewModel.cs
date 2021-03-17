using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ControlzEx.Standard;
using MaterialDesignThemes.Wpf;
using Stylet;
using WDLT.Clients.POE;
using WDLT.Clients.POE.Enums;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;
using WDLT.PoESpy.Helpers;
using WDLT.PoESpy.Properties;

namespace WDLT.PoESpy.ViewModels
{
    public class InspectViewModel : BaseTabViewModel, IHandle<InspectEvent>, IHandle<LeagueChangedEvent>
    {
        public POESearchResult TradeResult { get; private set; }
        public BindableCollection<POECharacter> Characters { get; }

        private string _accountName;
        public string AccountName
        {
            get => _accountName;
            set
            {
                SetAndNotify(ref _accountName, value);
                NotifyOfPropertyChange(nameof(CanInspect));
                Reset();
            }
        }

        private readonly AppWindowManager _windowManager;
        private readonly ISnackbarMessageQueue _snackQueue;
        private readonly ExileEngine _exileEngine;

        public InspectViewModel(IEventAggregator eventAggregator, AppWindowManager windowManager, ISnackbarMessageQueue snackQueue, ExileEngine exileEngine) : base(ETab.Inspect, eventAggregator)
        {
            _windowManager = windowManager;
            _snackQueue = snackQueue;
            _exileEngine = exileEngine;

            Characters = new BindableCollection<POECharacter>();
        }

        public void OpenTrade()
        {
            if(TradeResult == null) return;

            _windowManager.OpenTradeWindow(TradeResult, Settings.Default.League);
        }

        public bool CanOpenProfile => !string.IsNullOrWhiteSpace(AccountName);
        public void OpenProfile()
        {
            Process.Start("cmd", $"/C start {POEClient.BASE}/account/view-profile/{AccountName}");
        }

        public bool CanInspect => !string.IsNullOrWhiteSpace(AccountName);
        public Task Inspect()
        {
            return LoadingTask(InspectTask);
        }

        public async void Handle(InspectEvent message)
        {
            if (IsLoading) return;

            ActivateTab();

            AccountName = message.Account;
            await LoadingTask(InspectTask);
        }

        public void Handle(LeagueChangedEvent message)
        {
            Reset();
        }

        private void Reset()
        {
            TradeResult = null;
            Characters.Clear();
        }

        private async Task InspectTask()
        {
            Characters.Clear();

            var acc = AccountName.Trim();

            if (await _exileEngine.AccountExistAsync(acc))
            {
                TradeResult = await _exileEngine.SearchByAccountAsync(acc, EPOESort.Desc, EPOEOnlineStatus.Any);

                var chars = await _exileEngine.Characters(acc);
                if (chars == null) return;

                Characters.AddRange(chars.OrderBy(o => o.League).ThenByDescending(b => b.Level));
            }
            else
            {
                _snackQueue.Enqueue($"Account [{acc}] does not exist");
            }
        }
    }
}