using System;
using MaterialDesignThemes.Wpf;
using Stylet;
using StyletIoC;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Helpers;
using WDLT.PoESpy.Interfaces;
using WDLT.PoESpy.ViewModels;

namespace WDLT.PoESpy
{
    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Bind<BaseTabViewModel>().ToAllImplementations();
            builder.Bind<ISnackbarMessageQueue>().ToInstance(new SnackbarMessageQueue(TimeSpan.FromSeconds(3)));
            builder.Bind<IWindowFactory>().ToAbstractFactory();

            builder.Bind<AppWindowManager>().ToSelf().InSingletonScope();

            builder.Bind<ExileEngine>().ToSelf().InSingletonScope();
        }
    }
}