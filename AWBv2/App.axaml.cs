using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AWBv2.ViewModels;
using AWBv2.Views;

namespace AWBv2;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            
            var splash = new Splash();

            desktop.MainWindow = splash;
            splash.Show();


            try
            {
                await splash.RunInitializationAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Initialization error: {ex.Message}");
            }
            finally
            {
                
                var mainWin = new MainWindow();
                
                mainWin.DataContext = new MainWindowViewModel(mainWin);
                
                desktop.MainWindow = mainWin;
                mainWin.Show();

                splash.Close();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}