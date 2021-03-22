using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Stylet;
using Swordfish.NET.Collections;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Engine.Models;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;
using WDLT.PoESpy.Models;
using WDLT.PoESpy.Properties;
using WDLT.Utils.Extensions;

namespace WDLT.PoESpy.ViewModels
{
    public class SniperViewModel : BaseTabViewModel, IHandle<POESESSIDChangedEvent>, IHandle<LeaguesLoadedEvent>
    {
        public string SelectedLeague { get; set; }

        public string Query { get; set; }

        public string Name { get; set; }

        public List<POELeague> Leagues { get; private set; }

        public BindableCollection<POEFetchResult> TradeItems { get;}

        public ConcurrentObservableCollection<AppWebSocket> WebSockets { get; }

        private readonly ExileEngine _exileEngine;

        private readonly Queue<string> _messageQueue;

        private readonly string _saveFile = "ws.json";

        private ExileTradeWebSocketManager _exileWebSocketManager;

        public SniperViewModel(IEventAggregator eventAggregator, ISnackbarMessageQueue snackbarMessageQueue, ExileEngine exileEngine) : base(ETab.Sniper, eventAggregator, snackbarMessageQueue)
        {
            _messageQueue = new Queue<string>();
            _exileEngine = exileEngine;

            TradeItems = new BindableCollection<POEFetchResult>();
            WebSockets = new ConcurrentObservableCollection<AppWebSocket>();
        }

        public void ClearTradeItems()
        {
            TradeItems.Clear();
        }

        public void Whisper(POEFetchResult fetch)
        {
            Clipboard.SetText(fetch.WhisperText());
        }

        public void Copy(POEFetchResult fetch)
        {
            Clipboard.SetText(fetch.RawItemText());
        }

        public async Task Stop()
        {
            if (_exileWebSocketManager != null) await _exileWebSocketManager.StopAll();
        }

        public bool CanStart => _exileWebSocketManager != null;
        public async Task Start()
        {
            if (_exileWebSocketManager != null) await _exileWebSocketManager.StartAll();
        }

        public async Task Remove(AppWebSocket ws)
        {
            if(ws == null) return;

            await _exileWebSocketManager.Stop(ws.Name);

            if (_exileWebSocketManager.Remove(ws.Name)) WebSockets.Remove(ws);
        }

        public bool CanAdd => !string.IsNullOrWhiteSpace(SelectedLeague) && !string.IsNullOrWhiteSpace(Query) && !string.IsNullOrWhiteSpace(Name);
        public void Add()
        {
            if (_exileWebSocketManager != null && _exileWebSocketManager.Add(new ExileTradeWebSocketSetting(SelectedLeague, Query, Name)))
            {
                WebSockets.Add(new AppWebSocket(Name));

                Query = null;
                Name = null;
            }
        }

        public async void IDLoaderLoop()
        {
            while (true)
            {
                var queue = _messageQueue.DequeueChunk(10).ToList();

                if (queue.Any())
                {
                    var result = await _exileEngine.FetchAsync(queue);

                    if (result != null)
                    {
                        var items = result.Result;
                        items.Reverse();

                        foreach (var item in items)
                        {
                            TradeItems.Insert(0, item);
                            if (TradeItems.Count > 100) TradeItems.RemoveAt(TradeItems.Count - 1);
                        }
                    }
                }

                await Task.Delay(1000);
            }
        }

        public void Handle(POESESSIDChangedEvent message)
        {
            List<ExileTradeWebSocketSetting> tempSettings = null;
            TradeItems.Clear();
            WebSockets.Clear();

            if (_exileWebSocketManager != null)
            {
                _exileWebSocketManager.OnLogMessageEvent -= OnLogMessage;
                _exileWebSocketManager.OnNewIdRecievedEvent -= OnNewIdRecieved;
                _exileWebSocketManager.OnWebSocketConnectedEvent -= OnWebSocketConnected;
                _exileWebSocketManager.OnWebSocketDisconnectedEvent -= OnWebSocketDisconnected;
                tempSettings = _exileWebSocketManager.Settings();
                _exileWebSocketManager.Dispose();
                _exileWebSocketManager = null;
            }

            if (!string.IsNullOrWhiteSpace(message.NewValue))
            {
                IsEnabled = true;

                _exileWebSocketManager = new ExileTradeWebSocketManager(19, message.NewValue, Settings.Default.UserAgent);
                _exileWebSocketManager.OnLogMessageEvent += OnLogMessage;
                _exileWebSocketManager.OnNewIdRecievedEvent += OnNewIdRecieved;
                _exileWebSocketManager.OnWebSocketConnectedEvent += OnWebSocketConnected;
                _exileWebSocketManager.OnWebSocketDisconnectedEvent += OnWebSocketDisconnected;

                if (tempSettings != null)
                {
                    AddRange(tempSettings);
                }
                else if (File.Exists(_saveFile))
                {
                    try
                    {
                        var des = JsonConvert.DeserializeObject<List<ExileTradeWebSocketSetting>>(File.ReadAllText(_saveFile));
                        AddRange(des);
                    }
                    catch (JsonSerializationException)
                    {
                        // Ignore
                    }
                }
            }
            else
            {
                IsEnabled = false;
            }
        }

        public void Handle(LeaguesLoadedEvent message)
        {
            Leagues = message.Leagues;
        }

        public override Task<bool> CanCloseAsync()
        {
            if (_exileWebSocketManager != null) 
                File.WriteAllText(_saveFile, JsonConvert.SerializeObject(_exileWebSocketManager.Settings()));

            return base.CanCloseAsync();
        }

        protected override void OnInitialActivate()
        {
            IDLoaderLoop();

            base.OnInitialActivate();
        }

        private void AddRange(IEnumerable<ExileTradeWebSocketSetting> settings)
        {
            foreach (var ws in settings)
            {
                if (_exileWebSocketManager.Add(ws))
                {
                    WebSockets.Add(new AppWebSocket(ws.Name));
                }
                else
                {
                    break;
                }
            }
        }

        private void OnLogMessage(object sender, string message)
        {
            SnackbarMessage(message, false, true);
        }

        private void OnNewIdRecieved(object sender, string id)
        {
            _messageQueue.Enqueue(id);
        }

        private void OnWebSocketConnected(object sender, string webSocketName)
        {
            var webSocket = WebSockets.FirstOrDefault(f => f.Name == webSocketName);

            if (webSocket != null) webSocket.IsOpen = true;
        }

        private void OnWebSocketDisconnected(object sender, string webSocketName)
        {
            var webSocket = WebSockets.FirstOrDefault(f => f.Name == webSocketName);

            if (webSocket != null) webSocket.IsOpen = false;
        }
    }
}