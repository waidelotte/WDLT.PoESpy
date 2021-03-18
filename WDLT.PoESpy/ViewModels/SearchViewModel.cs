using System;
using System.Net;
using System.Net.WebSockets;
using Stylet;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Enums;
using Websocket.Client;

namespace WDLT.PoESpy.ViewModels
{
    public class SearchViewModel : BaseTabViewModel
    {
        private ExileEngine _exileEngine;
        private readonly Func<ClientWebSocket> _wsFactory;

        public SearchViewModel(IEventAggregator eventAggregator, ExileEngine exileEngine) : base(ETab.Search, eventAggregator)
        {
            //_exileEngine = exileEngine;

            //_wsFactory = new Func<ClientWebSocket>(() => new ClientWebSocket
            //{
            //    Options =
            //    {
            //        Cookies = new CookieContainer
            //    }
            //});

            //_wsFactory.

            //var client = new WebsocketClient(url, factory);
            //client.Start();
        }

    }
}