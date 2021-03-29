using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Stylet;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Enums;

namespace WDLT.PoESpy.ViewModels
{
    public class ToolsViewModel : BaseTabViewModel
    {
        public string AccountSearchResult { get; private set; }

        private string _characterName;
        public string CharacterName
        {
            get => _characterName;
            set
            {
                AccountSearchResult = null;
                SetAndNotify(ref _characterName, value);
                NotifyOfPropertyChange(() => CanAccountSearch);
            }
        }

        private readonly ExileEngine _exileEngine;

        public ToolsViewModel(IEventAggregator eventAggregator, ISnackbarMessageQueue snackbarMessageQueue,
            ExileEngine exileEngine) : base(ETab.Tools, eventAggregator, snackbarMessageQueue)
        {
            _exileEngine = exileEngine;
        }

        public bool CanAccountSearch => !IsLoading && !string.IsNullOrWhiteSpace(CharacterName);
        public Task AccountSearch()
        {
            return LoadingTask(AccountSearchTask);
        }

        private async Task AccountSearchTask()
        {
            var result = await _exileEngine.AccountNameByCharacter(CharacterName.Trim());

            if (result != null) AccountSearchResult = result.AccountName;
        }
    }
}