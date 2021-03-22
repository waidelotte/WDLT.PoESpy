using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Stylet;
using WDLT.Clients.POE;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Engine.Events;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;
using WDLT.PoESpy.Helpers;
using WDLT.PoESpy.Services;

namespace WDLT.PoESpy.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IHandle<ActivateTabEvent>, IHandle<POESESSIDChangedEvent>
    {
        public bool IsInitDone { get; private set; }

        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

        public BindableCollection<RateLimitTimer> RateLimits { get; }

        private readonly IEventAggregator _eventAggregator;

        private readonly ExileEngine _exileEngine;

        public ShellViewModel(ISnackbarMessageQueue snackQueue, IEventAggregator eventAggregator, IEnumerable<BaseTabViewModel> tabs, ExileEngine exileEngine)
        {
            DisplayName = $"PoE Spy v.{GetType().Assembly.GetName().Version}";
            RateLimits = new BindableCollection<RateLimitTimer>();

            Items.AddRange(tabs.OrderBy(o => o.Tab));
            ActiveItem = Items.First(f => ((BaseTabViewModel) f).Tab == ETab.Moneyway);

            SnackbarMessageQueue = snackQueue;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            _exileEngine = exileEngine;
            _exileEngine.OnMessageEvent += ExileEngineOnMessage;
            _exileEngine.OnRateLimitEvent += ExileEngineOnNewRateLimit;
        }

        public void OpenTrade()
        {
            Process.Start("cmd", $"/C start {POEClient.BASE + "/trade"}");
        }

        public void Screenshot(FrameworkElement element)
        {
            var renderTargetBitmap = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            renderTargetBitmap.Render(element);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            var bitmapImage = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            Clipboard.SetImage(bitmapImage);

            SnackbarMessageQueue.Enqueue("Screenshot copied to clipboard", null, null, null, false, false, TimeSpan.FromSeconds(2));
        }

        public void Handle(ActivateTabEvent message)
        {
            ActivateItem(Items.First(f => ((BaseTabViewModel)f).Tab == message.Tab));
        }

        public void Handle(POESESSIDChangedEvent message)
        {
            _exileEngine.SetSession(message.NewValue);
        }

        protected override async void OnInitialActivate()
        {
            var leagues = await _exileEngine.TradeLeaguesAsync();
            var tradeStatic = await _exileEngine.TradeStaticAsync();

            if (leagues == null || tradeStatic == null)
            {
                MessageBox.Show("Loading error. Please try again later.");
                RequestClose();
                return;
            }

            _eventAggregator.Publish(new LeaguesLoadedEvent(leagues.Result));

            ImageCacheService.CreateDirectories();
            foreach (var currency in tradeStatic.Result.SelectMany(s => s.Entries).Where(w => !string.IsNullOrWhiteSpace(w.Image)))
            {
                if (ImageCacheService.Exist(currency.Id)) continue;
                await _exileEngine.DownloadImageAsync(currency.Image, ImageCacheService.Get(currency.Id));
            }

            IsInitDone = true;

            _eventAggregator.Publish(new AppLoadedEvent());

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            foreach (MetroWindow win in Application.Current.Windows)
            {
                win.Close();
            }
        }

        private void ExileEngineOnNewRateLimit(object sender, ExileRateLimitArgs args)
        {
            var exist = RateLimits.FirstOrDefault(f => f.Endpoint == args.Endpoint);

            if (exist == null)
            {
                RateLimits.Add(new RateLimitTimer(args.RateLimit.BanUntil, args.Endpoint, rl => RateLimits.Remove(rl)));
            }
            else
            {
                exist.Remaining = (int)(args.RateLimit.BanUntil - DateTimeOffset.Now).TotalSeconds;
            }
        }

        private void ExileEngineOnMessage(object sender, string message)
        {
            SnackbarMessageQueue.Enqueue(message, null, null, null, false, false);
        }
    }
}