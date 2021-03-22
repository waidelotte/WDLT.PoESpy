using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WDLT.Clients.POE;
using WDLT.PoESpy.Engine.Models;
using Websocket.Client;

namespace WDLT.PoESpy.Engine
{
    public class ExileTradeWebSocketManager : IDisposable
    {
        public ConcurrentDictionary<string, ExileTradeWebSocket> WebSockets { get; }

        public event EventHandler<string> OnNewIdRecievedEvent;
        public event EventHandler<string> OnLogMessageEvent;
        public event EventHandler<string> OnWebSocketConnectedEvent;
        public event EventHandler<string> OnWebSocketDisconnectedEvent;

        private readonly Func<ClientWebSocket> _wsFactory;
        private readonly int _limit;
        private bool _isDisposed;

        public ExileTradeWebSocketManager(int limit, [NotNull] string poesessid, [NotNull] string userAgent)
        {
            if(string.IsNullOrWhiteSpace(poesessid)) throw new ArgumentNullException(nameof(poesessid));
            if (string.IsNullOrWhiteSpace(poesessid)) throw new ArgumentNullException(nameof(userAgent));

            WebSockets = new ConcurrentDictionary<string, ExileTradeWebSocket>();

            _limit = limit;
            _wsFactory = CreateFactory(poesessid, userAgent);
        }

        public async Task StartAll()
        {
            foreach (var webSocket in WebSockets.Values)
            {
                await webSocket.Start();
            }
        }

        public async Task StopAll()
        {
            foreach (var webSocket in WebSockets.Values)
            {
                await webSocket.Stop();
            }
        }

        public void AddRange(IEnumerable<ExileTradeWebSocketSetting> settings)
        {
            foreach (var setting in settings)
            {
                if(!Add(setting)) return;
            }
        }

        public bool Add(ExileTradeWebSocketSetting setting)
        {
            if (WebSockets.Count >= _limit)
            {
                Log($"WebSockets limit reached [{WebSockets.Count}/{_limit}]");
                return false;
            }

            var ws = new ExileTradeWebSocket(setting.Query, setting.League, _wsFactory);
            Subscribe(ws);

            return WebSockets.TryAdd(setting.Name, ws);
        }

        public bool Remove(string name)
        {
            if (WebSockets.TryRemove(name, out var webSocket))
            {
                Unsubscribe(webSocket);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task Stop(string name)
        {
            if (WebSockets.TryGetValue(name, out var webSocket))
            {
                await webSocket.Stop();
            }
        }

        public List<ExileTradeWebSocketSetting> Settings()
        {
            return WebSockets.Select(s => new ExileTradeWebSocketSetting(s.Value.League, s.Value.Query, s.Key)).ToList();
        }

        private void Log(string message)
        {
            OnLogMessageEvent?.Invoke(this, message);
        }

        private Func<ClientWebSocket> CreateFactory(string session, string userAgent)
        {
            return () =>
            {
                var client = new ClientWebSocket();

                client.Options.SetRequestHeader("User-Agent", userAgent);
                client.Options.SetRequestHeader("Origin", POEClient.BASE);
                client.Options.SetRequestHeader("Cookie", $"POESESSID={session}");

                return client;
            };
        }

        private void OnWebSocketConnected(object sender, EventArgs args)
        {
            var webSocket = WebSockets.FirstOrDefault(f => f.Value == sender);
            if(webSocket.Value == null) return;

            OnWebSocketConnectedEvent?.Invoke(this, webSocket.Key);
        }

        private void OnWebSocketDisconnected(object sender, DisconnectionInfo disconnectionInfo)
        {
            var webSocket = WebSockets.FirstOrDefault(f => f.Value == sender);
            if (webSocket.Value == null) return;

            OnWebSocketDisconnectedEvent?.Invoke(this, webSocket.Key);
        }

        private void Subscribe(ExileTradeWebSocket webSocket)
        {
            webSocket.OnNewIdRecievedEvent += OnNewIdRecievedEvent;
            webSocket.OnLogMessageEvent += OnLogMessageEvent;
            webSocket.OnConnectedEvent += OnWebSocketConnected;
            webSocket.OnDisconnectedEvent += OnWebSocketDisconnected;
        }

        private void Unsubscribe(ExileTradeWebSocket webSocket)
        {
            webSocket.OnNewIdRecievedEvent -= OnNewIdRecievedEvent;
            webSocket.OnLogMessageEvent -= OnLogMessageEvent;
            webSocket.OnConnectedEvent -= OnWebSocketConnected;
            webSocket.OnDisconnectedEvent -= OnWebSocketDisconnected;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                foreach (var ws in WebSockets)
                {
                    Unsubscribe(ws.Value);
                    ws.Value.Dispose();
                }
            }

            _isDisposed = true;
        }
    }
}