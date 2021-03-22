using System;
using System.Windows;
using MaterialDesignThemes.Wpf;
using Stylet;
using StyletIoC;
using WDLT.PoESpy.Engine;
using WDLT.PoESpy.Helpers;
using WDLT.PoESpy.Interfaces;
using WDLT.PoESpy.Properties;
using WDLT.PoESpy.ViewModels;

namespace WDLT.PoESpy
{
    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Bind<ExileEngine>().ToInstance(new ExileEngine(Settings.Default.UserAgent));

            builder.Bind<BaseTabViewModel>().ToAllImplementations();
            builder.Bind<ISnackbarMessageQueue>().ToInstance(new SnackbarMessageQueue(TimeSpan.FromSeconds(3)));
            builder.Bind<IWindowFactory>().ToAbstractFactory();

            builder.Bind<AppWindowManager>().ToSelf().InSingletonScope();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();

            base.OnExit(e);
        }
    }
}