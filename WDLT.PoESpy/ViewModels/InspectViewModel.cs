using System.Collections.Generic;
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
    public class InspectViewModel : BaseTabViewModel, IHandle<InspectEvent>, IHandle<LeaguesLoadedEvent>
    {
        public POESearchResult TradeResult { get; private set; }

        public BindableCollection<POECharacter> Characters { get; }

        public List<POELeague> Leagues { get; private set; }

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
        private readonly ExileEngine _exileEngine;

        public InspectViewModel(IEventAggregator eventAggregator, AppWindowManager windowManager,
            ISnackbarMessageQueue snackbarMessageQueue, ExileEngine exileEngine) : base(ETab.Inspect, eventAggregator,
            snackbarMessageQueue)
        {
            _windowManager = windowManager;
            _exileEngine = exileEngine;

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

        public void Handle(LeaguesLoadedEvent message)
        {
            Leagues = message.Leagues;
        }

        private async Task InspectTask()
        {
            Characters.Clear();

            var account = AccountName.Trim();
            var league = SelectedLeague;

            if (await _exileEngine.AccountExistAsync(account))
            {
                await LoadTrade(account, league);

                var chars = await _exileEngine.Characters(account);
                if (chars == null) return;

                Characters.AddRange(chars.OrderBy(o => o.League).ThenByDescending(b => b.Level));
            }
            else
            {
                SnackbarMessage($"Account [{account}] does not exist", true, false);
            }
        }

        private async Task LoadTrade(string account, string league)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(league)) return;

            TradeResult =
                await _exileEngine.SearchByAccountAsync(AccountName, SelectedLeague, EPOESort.Desc,
                    EPOEOnlineStatus.Any);
        }
    }
}