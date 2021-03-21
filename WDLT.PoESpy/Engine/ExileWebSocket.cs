using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stylet;
using WDLT.PoESpy.Models;
using Websocket.Client;

namespace WDLT.PoESpy.Engine
{
    public class ExileWebSocket : PropertyChangedBase
    {
        public Action<string> MessageRecieved;
        public ExileWebSocketSetting Setting { get; }
        public bool IsOpen { get; private set; }

        private readonly WebsocketClient _client;

        public ExileWebSocket(ExileWebSocketSetting setting, Func<ClientWebSocket> factory)
        {
            Setting = setting;

            _client = new WebsocketClient(new Uri($"wss://www.pathofexile.com/api/trade/live/{setting.League}/{setting.Query}"), factory)
            {
                IsReconnectionEnabled = false
            };

            _client.MessageReceived
                .Select(msg => Observable.FromAsync(() => OnWSMessage(msg)))
                .Concat()
                .Subscribe();

            _client.DisconnectionHappened.Subscribe(OnDisconnected);
        }

        public Task Start()
        {
            return _client.StartOrFail();
        }

        public Task Stop()
        {
            return _client.Stop(WebSocketCloseStatus.Empty, null);
        }

        private void OnDisconnected(DisconnectionInfo info)
        {
            IsOpen = false;
        }

        private async Task OnWSMessage(ResponseMessage message)
        {
            var text = message.Text;
            var des = JsonConvert.DeserializeObject<ExileWebSocketMessage>(text);

            if (des.Auth != null)
            {
                if (des.Auth.Value)
                {
                    IsOpen = true;
                }
                else
                {
                    IsOpen = false;
                    await _client.Stop(WebSocketCloseStatus.Empty, null);
                }
            }

            if (des.New != null)
            {
                foreach (var id in des.New)
                {
                    MessageRecieved?.Invoke(id);
                }
            }
        }
    }
}