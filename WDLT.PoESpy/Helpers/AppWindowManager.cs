using Stylet;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Interfaces;

namespace WDLT.PoESpy.Helpers
{
    public class AppWindowManager
    {
        private readonly IWindowManager _windowManager;
        private readonly IWindowFactory _windowFactory;

        public AppWindowManager(IWindowManager windowManager, IWindowFactory windowFactory)
        {
            _windowManager = windowManager;
            _windowFactory = windowFactory;
        }

        public void OpenTradeWindow(POESearchResult search, string league)
        {
            var wm = _windowFactory.CreateTradeWindow();

            wm.InsertSearch(search, league);

            _windowManager.ShowWindow(wm);
        }
    }
}