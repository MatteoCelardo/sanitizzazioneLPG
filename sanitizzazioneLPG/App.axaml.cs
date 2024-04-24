using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using sanitizzazioneLPG.ModelliViste;
using sanitizzazioneLPG.Viste;

namespace sanitizzazioneLPG;

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
            desktop.MainWindow = new HomeVista
            {
                DataContext = new HomeModelloVista(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}