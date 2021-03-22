using MaterialDesignThemes.Wpf;
using Stylet;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;
using WDLT.PoESpy.Properties;

namespace WDLT.PoESpy.ViewModels
{
    public class SettingsViewModel : BaseTabViewModel, IHandle<AppLoadedEvent>
    {
        public string POESESSID { get; set; }

        public void SavePOESESSID()
        {
            Settings.Default.POESESSID = POESESSID;
            EventAggregator.Publish(new POESESSIDChangedEvent(POESESSID));
        }

        public SettingsViewModel(IEventAggregator eventAggregator, ISnackbarMessageQueue snackbarMessageQueue) : base(ETab.Settings, eventAggregator, snackbarMessageQueue) { }

        public void Handle(AppLoadedEvent message)
        {
            POESESSID = Settings.Default.POESESSID;
            EventAggregator.Publish(new POESESSIDChangedEvent(POESESSID));

            if (string.IsNullOrWhiteSpace(Settings.Default.POESESSID))
            {
                SnackbarMessage("We strongly recommend specifying POESESSID in the Settings", false, false);
            }
        }
    }
}