using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Stylet;
using WDLT.Clients.POE;
using WDLT.Clients.POE.Enums;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;
using WDLT.PoESpy.Helpers;

namespace WDLT.PoESpy.ViewModels
{
    public class InspectViewModel : BaseTabViewModel, IHandle<InspectEvent>
    {
        public ExileEngine ExileEngine { get; }
        public POESearchResult TradeResult { get; private set; }
        public BindableCollection<POECharacter> Characters { get; }
        public string AccountName { get; set; }

        private string _selectedLeague;
        public string SelectedLeague
        {
            get => _selectedLeague;
            set
            {
                SetAndNotify(ref _selectedLeague, value);
                TradeResult = null;
            }
        }

        private readonly AppWindowManager _windowManager;
        private readonly ISnackbarMessageQueue _snackQueue;

        public InspectViewModel(IEventAggregator eventAggregator, AppWindowManager windowManager, ISnackbarMessageQueue snackQueue, ExileEngine exileEngine) : base(ETab.Inspect, eventAggregator)
        {
            _windowManager = windowManager;
            _snackQueue = snackQueue;
            ExileEngine = exileEngine;

            Characters = new BindableCollection<POECharacter>();
        }

        public bool CanOpenTrade => !string.IsNullOrWhiteSpace(SelectedLeague) && TradeResult?.Total > 0;
        public void OpenTrade()
        {
            if(TradeResult == null) return;

            _windowManager.OpenTradeWindow(TradeResult, SelectedLeague);
        }

        public bool CanOpenProfile => !string.IsNullOrWhiteSpace(AccountName);
        public void OpenProfile()
        {
            Process.Start("cmd", $"/C start {POEClient.BASE}/account/view-profile/{AccountName}");
        }

        public bool CanInspect => !string.IsNullOrWhiteSpace(AccountName) && !IsLoading;
        public Task Inspect()
        {
            return LoadingTask(InspectTask);
        }

        public async void Handle(InspectEvent message)
        {
            if (IsLoading) return;

            ActivateTab();

            AccountName = message.Account;
            SelectedLeague = message.League;
            await LoadingTask(InspectTask);
        }

        private async Task InspectTask()
        {
            Characters.Clear();

            var account = AccountName.Trim();
            var league = SelectedLeague;

            if (await ExileEngine.AccountExistAsync(account))
            {
                await LoadTrade(account, league);

                var chars = await ExileEngine.Characters(account);
                if (chars == null) return;

                Characters.AddRange(chars.OrderBy(o => o.League).ThenByDescending(b => b.Level));
            }
            else
            {
                _snackQueue.Enqueue($"Account [{account}] does not exist");
            }
        }

        private async Task LoadTrade(string account, string league)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(league)) return;

            TradeResult = await ExileEngine.SearchByAccountAsync(AccountName, SelectedLeague, EPOESort.Desc, EPOEOnlineStatus.Any);
        }
    }
}