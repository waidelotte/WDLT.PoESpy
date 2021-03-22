using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WDLT.PoESpy.Engine.Models;
using Websocket.Client;

namespace WDLT.PoESpy.Engine
{
    public class ExileTradeWebSocket : IDisposable
    {
        public string Query { get; }
        public string League { get; }

        public event EventHandler<string> OnNewIdRecievedEvent;
        public event EventHandler<string> OnLogMessageEvent;
        public event EventHandler<DisconnectionInfo> OnDisconnectedEvent;
        public event EventHandler OnConnectedEvent;

        private readonly WebsocketClient _client;
        private bool _isDisposed;

        public ExileTradeWebSocket([NotNull] string query, [NotNull] string league, Func<ClientWebSocket> factory)
        {
            Query = query;
            League = league;

            _client = new WebsocketClient(new Uri($"wss://www.pathofexile.com/api/trade/live/{League}/{Query}"), factory)
            {
                IsReconnectionEnabled = false
            };

            _client.MessageReceived
                .Select(msg => Observable.FromAsync(() => OnMessage(msg)))
                .Concat()
                .Subscribe();

            _client.DisconnectionHappened.Subscribe(Disconnected);
        }

        public async Task<bool> Start()
        {
            try
            {
                await _client.StartOrFail();
                return true;
            }
            catch
            {
                Log($"Failed to start [{Query}][{League}]");
                return false;
            }
        }

        public async Task<bool> Stop()
        {
            try
            {
                await _client.StopOrFail(WebSocketCloseStatus.Empty, null);
                return true;
            }
            catch
            {
                Log($"Failed to stop [{Query}][{League}]");
                return false;
            }
        }

        private void Disconnected(DisconnectionInfo info)
        {
            OnDisconnectedEvent?.Invoke(this, info);
        }

        private async Task OnMessage(ResponseMessage message)
        {
            var des = JsonConvert.DeserializeObject<ExileTradeWebSocketMessage>(message.Text);

            if (des?.Auth != null)
            {
                if (des.Auth.Value)
                {
                    OnConnectedEvent?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    await Stop();
                }
            }

            if (des?.New != null)
            {
                foreach (var id in des.New)
                {
                    OnNewIdRecievedEvent?.Invoke(this, id);
                }
            }
        }

        private void Log(string message)
        {
            OnLogMessageEvent?.Invoke(this, message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)  _client.Dispose();
            
            _isDisposed = true;
        }
    }
}