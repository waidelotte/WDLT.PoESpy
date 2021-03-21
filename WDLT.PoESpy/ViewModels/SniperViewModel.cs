using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Stylet;
using Swordfish.NET.Collections;
using WDLT.Clients.POE;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;
using WDLT.PoESpy.Models;
using WDLT.PoESpy.Properties;
using WDLT.Utils.Extensions;

namespace WDLT.PoESpy.ViewModels
{
    public class SniperViewModel : BaseTabViewModel, IHandle<AppLoadedEvent>
    {
        public BindableCollection<POEFetchResult> TradeItems { get;}
        public ConcurrentObservableCollection<ExileWebSocket> WebSockets { get; }
        public ExileEngine ExileEngine { get; }
        public string SelectedLeague { get; set; }
        public string Query { get; set; }
        public string Name { get; set; }

        private readonly Func<ClientWebSocket> _wsFactory;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly Queue<string> _messageQueue;

        public SniperViewModel(IEventAggregator eventAggregator, ISnackbarMessageQueue snackbarMessageQueue, ExileEngine exileEngine) : base(ETab.Sniper, eventAggregator)
        {
            _snackbarMessageQueue = snackbarMessageQueue;
            _messageQueue = new Queue<string>();
            ExileEngine = exileEngine;

            _wsFactory = () =>
            {
                var client = new ClientWebSocket();

                client.Options.SetRequestHeader("User-Agent", Settings.Default.UserAgent);
                client.Options.SetRequestHeader("Origin", POEClient.BASE);
                client.Options.SetRequestHeader("Cookie", $"POESESSID={ExileEngine.GetSession()}");

                return client;
            };

            WebSockets = new ConcurrentObservableCollection<ExileWebSocket>();
            TradeItems = new BindableCollection<POEFetchResult>();
        }

        protected override void OnInitialActivate()
        {
            MessageLoader();

            base.OnInitialActivate();
        }

        public void ClearTradeItems()
        {
            TradeItems.Clear();
        }

        public void Whisper(POEFetchResult fetch)
        {
            fetch.ClipboardSetWhisper();
        }

        public void Copy(POEFetchResult fetch)
        {
            fetch.ClipboardSetItem();
        }

        public async void MessageLoader()
        {
            while (true)
            {
                var queue = _messageQueue.DequeueChunk(10).ToList();

                if (queue.Any())
                {
                    var result = await ExileEngine.FetchAsync(queue);

                    if (result != null)
                    {
                        var items = result.Result;
                        items.Reverse();

                        foreach (var item in items)
                        {
                            TradeItems.Insert(0, item);
                            if(TradeItems.Count > 100) TradeItems.RemoveAt(TradeItems.Count - 1);
                        }
                    }
                }

                await Task.Delay(1000);
            }
        }

        public async Task RemoveWebSocket(ExileWebSocket ws)
        {
            if(ws == null) return;

            if (await StopWebSocket(ws))
            {
                WebSockets.Remove(ws);
            }
        }

        public async Task StopAll()
        {
            foreach (var webSocket in WebSockets)
            {
                await StopWebSocket(webSocket);
            }
        }

        public async Task StartAll()
        {
            foreach (var webSocket in WebSockets)
            {
                await StartWebSocket(webSocket);
            }
        }

        private async Task<bool> StopWebSocket(ExileWebSocket webSocket)
        {
            try
            {
                await webSocket.Stop();
                return true;
            }
            catch (Exception)
            {
                _snackbarMessageQueue.Enqueue($"Failed to stop [{webSocket.Setting.Name}][{webSocket.Setting.Query}]", null, null, null, false, false, TimeSpan.FromSeconds(3));
                return false;
            }
        }

        private async Task<bool> StartWebSocket(ExileWebSocket webSocket)
        {
            try
            {
                await webSocket.Start();
                return true;
            }
            catch (Exception)
            {
                _snackbarMessageQueue.Enqueue($"Failed to start [{webSocket.Setting.Name}][{webSocket.Setting.Query}]", null, null, null, false, false, TimeSpan.FromSeconds(3));
                return false;
            }
        }

        public bool CanTryAdd => !string.IsNullOrWhiteSpace(SelectedLeague) && !string.IsNullOrWhiteSpace(Query) && !string.IsNullOrWhiteSpace(Name) && WebSockets.Count < 19;
        public void TryAdd()
        {
            if (Add(new ExileWebSocketSetting
            {
                League = SelectedLeague,
                Name = Name.Trim(),
                Query = Query.Trim()
            }))
            {

                Query = null;
                Name = null;
            }
        }

        private bool Add(ExileWebSocketSetting setting)
        {
            if (string.IsNullOrWhiteSpace(ExileEngine.GetSession()))
            {
                _snackbarMessageQueue.Enqueue("You must specify POESESSID in the Settings", null, null, null, false, false, TimeSpan.FromSeconds(2));
                return false;
            }

            if (WebSockets.Count >= 19)
            {
                _snackbarMessageQueue.Enqueue("Limit reached [19/19]", null, null, null, false, false, TimeSpan.FromSeconds(3));
                return false;
            }

            var ws = new ExileWebSocket(setting, _wsFactory)
            {
                MessageRecieved = s => _messageQueue.Enqueue(s)
            };

            WebSockets.Add(ws);
            return true;
        }

        public void Handle(AppLoadedEvent message)
        {
            if (!File.Exists("ws.json")) return;

            try
            {
                var des = JsonConvert.DeserializeObject<IEnumerable<ExileWebSocketSetting>>(File.ReadAllText("ws.json"));
                foreach (var setting in des)
                {
                    if (!Add(setting)) break;
                }
            }
            catch
            {
                // Ignore
            }
        }

        public override Task<bool> CanCloseAsync()
        {
            if (WebSockets.Any()) File.WriteAllText("ws.json", JsonConvert.SerializeObject(WebSockets.Select(s => s.Setting)));

            return base.CanCloseAsync();
        }
    }
}