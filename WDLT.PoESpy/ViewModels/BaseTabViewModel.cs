using System;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Stylet;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;

namespace WDLT.PoESpy.ViewModels
{
    public abstract class BaseTabViewModel : Screen, IHandle
    {
        public bool IsLoading { get; protected set; }

        public bool IsEnabled { get; set; }

        public ETab Tab { get; }

        protected readonly IEventAggregator EventAggregator;

        private readonly ISnackbarMessageQueue _snackbarMessageQueue;

        protected BaseTabViewModel(ETab tab, IEventAggregator eventAggregator, ISnackbarMessageQueue snackbarMessageQueue)
        {
            Tab = tab;
            DisplayName = tab.ToString();
            EventAggregator = eventAggregator;
            EventAggregator.Subscribe(this);
            IsEnabled = true;

            _snackbarMessageQueue = snackbarMessageQueue;
        }

        protected void ActivateTab()
        {
            EventAggregator.Publish(new ActivateTabEvent(Tab));
        }

        protected async Task LoadingTask(Func<Task> task)
        {
            if(IsLoading) return;

            IsLoading = true;

            await task.Invoke();

            IsLoading = false;
        }

        protected void SnackbarMessage(string message, bool promote, bool duplicateCheck, int sec = 3)
        {
            _snackbarMessageQueue.Enqueue(message, null, null, null, promote, !duplicateCheck, TimeSpan.FromSeconds(sec));
        }
    }
}