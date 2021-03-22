using System;
using System.Windows.Threading;
using Stylet;

namespace WDLT.PoESpy.Helpers
{
    public class RateLimitTimer : PropertyChangedBase
    {
        public string Endpoint { get; }
        public double RemainingPercent { get; private set; }
        public int Remaining { get;  set; }

        private readonly DispatcherTimer _timer;

        public RateLimitTimer(DateTimeOffset banUntil, string endpoint, Action<RateLimitTimer> onStop)
        {
            Endpoint = endpoint;

            var total = (int) (banUntil - DateTimeOffset.Now).TotalSeconds;
            Remaining = total;

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (Remaining <= 0)
                {
                    _timer.Stop();
                    onStop?.Invoke(this);
                }

                Remaining--;

                RemainingPercent = (double)Remaining / total * 100;
            }, Dispatcher.CurrentDispatcher);

            _timer.Start();
        }
    }
}