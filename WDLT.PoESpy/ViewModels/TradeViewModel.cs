using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Stylet;
using WDLT.Clients.POE;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Engine;

namespace WDLT.PoESpy.ViewModels
{
    public class TradeViewModel : Screen
    {
        public POESearchResult Search { get; private set; }
        public BindableCollection<POEFetchResult> Fetch { get; }
        public bool IsLoading { get; set; }

        private int _page;
        private readonly ExileEngine _exileEngine;
        private string _league;

        public TradeViewModel(ExileEngine exileEngine)
        {
            DisplayName = "PoE Spy";

            _exileEngine = exileEngine;

            Fetch = new BindableCollection<POEFetchResult>();
        }

        public void InsertSearch(POESearchResult search, string league)
        {
            _league = league;
            Search = search;
            DisplayName = $"PoE Spy | {league} | Total: {search.Total} [ID:{search.Id}]";
        }

        public void OpenClassicTrade()
        {
            Process.Start("cmd", $"/C start {POEClient.TradeUri(_league, Search.Id)}");
        }

        public void Whisper(POEFetchResult fetch)
        {
            var text = fetch.Listing.Whisper;
            Clipboard.SetText(text);
        }

        public async void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var list = (ListView)sender;
            var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(list, 0);

            if (scrollViewer.ScrollableHeight > 1 && Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 1)
            {
                await LoadNext();
            }
        }

        protected override async void OnInitialActivate()
        {
            await LoadNext();
        }

        private async Task LoadNext()
        {
            IsLoading = true;
            var skip = 10 * _page;
            if (skip < Search.Total)
            {
                var take = Search.Result.Skip(skip).Take(10).ToList();
                if (take.Count > 0)
                {
                    var fetch = await _exileEngine.FetchAsync(take);
                    await Task.Delay(500);
                    if (fetch != null)
                    {
                        _page++;
                        Fetch.AddRange(fetch.Result);
                    }
                }
            }

            IsLoading = false;
        }
    }
}