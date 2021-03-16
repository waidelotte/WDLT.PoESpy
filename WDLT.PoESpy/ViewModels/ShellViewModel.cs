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
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;

namespace WDLT.PoESpy.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IHandle<ActivateTabEvent>
    {
        public static ShellViewModel Instance;

        public bool IsInitDone { get; private set; }
        public ExileEngine ExileEngine { get; }
        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

        private readonly IEventAggregator _eventAggregator;

        public ShellViewModel(ISnackbarMessageQueue snackQueue, IEventAggregator eventAggregator, IEnumerable<BaseTabViewModel> tabs, ExileEngine exileEngine)
        {
            Instance = this;

            DisplayName = $"PoE Spy v.{GetType().Assembly.GetName().Version}";

            Items.AddRange(tabs.OrderBy(o => o.Tab));
            ActiveItem = Items.First(f => ((BaseTabViewModel) f).Tab == ETab.Moneyway);

            SnackbarMessageQueue = snackQueue;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            ExileEngine = exileEngine;
        }

        protected override async void OnInitialActivate()
        {
            var result = await ExileEngine.InitAsync();

            if (!result)
            {
                MessageBox.Show("Loading error. Please try again later.");
                RequestClose();
                return;
            }

            IsInitDone = true;

            _eventAggregator.Publish(new AppLoadedEvent());

            base.OnInitialActivate();
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

        protected override void OnClose()
        {
            foreach (MetroWindow win in Application.Current.Windows)
            {
                win.Close();
            }
        }
    }
}