using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using UP;
using UPNew.ViewModels;
using UPNew.Views;
using MainWindow = UPNew.Views.MainWindow;

namespace UPNew;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {

            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}