using System;
using System.Linq;
using MaterialDesignThemes.Wpf;
using Stylet;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;
using WDLT.PoESpy.Properties;

namespace WDLT.PoESpy.ViewModels
{
    public class SettingsViewModel : BaseTabViewModel, IHandle<AppLoadedEvent>
    {
        public BindableCollection<string> Leagues { get; private set; }

        private string _selectedLeague;
        public string SelectedLeague
        {
            get => _selectedLeague;
            set
            {
                if (_selectedLeague != value)
                {
                    EventAggregator.Publish(new LeagueChangedEvent());
                }

                SetAndNotify(ref _selectedLeague, value);
            }
        }

        private string _poesessid;
        public string POESESSID
        {
            get => _poesessid;
            set
            {
                _exileEngine.SetSession(value);
                SetAndNotify(ref _poesessid, value);
            }
        }

        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly ExileEngine _exileEngine;

        public SettingsViewModel(IEventAggregator eventAggregator, ISnackbarMessageQueue snackbarMessageQueue, ExileEngine exileEngine) : base(ETab.Settings, eventAggregator)
        {
            _snackbarMessageQueue = snackbarMessageQueue;
            _exileEngine = exileEngine;
        }

        public void Handle(AppLoadedEvent message)
        {
            POESESSID = Settings.Default.POESESSID;

            if (string.IsNullOrWhiteSpace(POESESSID))
            {
                _snackbarMessageQueue.Enqueue("We strongly recommend specifying POESESSID in the Settings");
            }

            Leagues = new BindableCollection<string>(_exileEngine.Leagues.Select(s => s.Text));

            if (Settings.Default.League != null && Leagues.Contains(Settings.Default.League))
            {
                SelectedLeague = Settings.Default.League;
            }
            else
            {
                SelectedLeague = Leagues.FirstOrDefault();
            }
        }

        protected override void OnDeactivate()
        {
            Settings.Default.League = SelectedLeague;
            Settings.Default.POESESSID = POESESSID;

            Settings.Default.Save();
            base.OnDeactivate();
        }
    }
}