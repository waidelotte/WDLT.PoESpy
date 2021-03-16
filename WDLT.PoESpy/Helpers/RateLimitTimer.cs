using System;
using System.Windows.Threading;
using Stylet;
using WDLT.Clients.POE.Models;

namespace WDLT.PoESpy.Helpers
{
    public class RateLimitTimer : PropertyChangedBase
    {
        public string Endpoint { get; }
        public POERateLimit RateLimit { get; }
        public int RemainingPercent { get; private set; }
        public int Remaining { get; private set; }

        private readonly DispatcherTimer _timer;
        public TimeSpan Limit { get; set; }

        public RateLimitTimer(POERateLimit limit, string endpoint, Action<RateLimitTimer> onStop)
        {
            Endpoint = endpoint;
            RateLimit = limit;

            Limit = TimeSpan.FromSeconds(limit.Ban);

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (Limit <= TimeSpan.Zero)
                {
                    _timer.Stop();
                    onStop?.Invoke(this);
                }

                Limit = Limit.Add(TimeSpan.FromSeconds(-1));

                RemainingPercent = (int)(Limit.TotalSeconds / limit.Ban * 100);
                Remaining = (int)Limit.TotalSeconds;
            }, Dispatcher.CurrentDispatcher);

            _timer.Start();
        }

    }
}