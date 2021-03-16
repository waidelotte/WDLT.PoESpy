using System;
using System.Threading.Tasks;
using Stylet;
using WDLT.PoESpy.Enums;
using WDLT.PoESpy.Events;

namespace WDLT.PoESpy.ViewModels
{
    public abstract class BaseTabViewModel : Screen, IDisposable, IHandle
    {
        public bool IsLoading { get; protected set; }

        public ETab Tab { get; }

        protected readonly IEventAggregator EventAggregator;

        protected BaseTabViewModel(ETab tab, IEventAggregator eventAggregator)
        {
            Tab = tab;
            DisplayName = tab.ToString();
            EventAggregator = eventAggregator;
            EventAggregator.Subscribe(this);
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

        public virtual void Dispose() { }
    }
}