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
        public ExileEngine ExileEngine { get; }

        private string _poesessid;
        public string POESESSID
        {
            get => _poesessid;
            set
            {
                ExileEngine.SetSession(value);
                SetAndNotify(ref _poesessid, value);
            }
        }

        private readonly ISnackbarMessageQueue _snackbarMessageQueue;

        public SettingsViewModel(IEventAggregator eventAggregator, ISnackbarMessageQueue snackbarMessageQueue, ExileEngine exileEngine) : base(ETab.Settings, eventAggregator)
        {
            _snackbarMessageQueue = snackbarMessageQueue;
            ExileEngine = exileEngine;
        }

        public void Handle(AppLoadedEvent message)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.POESESSID))
            {
                _snackbarMessageQueue.Enqueue("We strongly recommend specifying POESESSID in the Settings");
            }
            else
            {
                POESESSID = Settings.Default.POESESSID;
            }
        }

        protected override void OnDeactivate()
        {
            Settings.Default.POESESSID = POESESSID;

            Settings.Default.Save();
            base.OnDeactivate();
        }
    }
}