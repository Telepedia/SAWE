using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AWBv2.ViewModels;
using AWBv2.Views;
using Functions;

namespace AWBv2
{
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
                // Remove Avalonia data validation duplicate to avoid duplicate validations
                BindingPlugins.DataValidators.RemoveAt(0);
                
                var splash = new Splash();
                desktop.MainWindow = splash;
                splash.Show();
    
                Setup setup = new();
                
                splash.SetProgress(0);
                
                try
                {
                    var databaseExists = await setup.CheckDatabaseAsync();

                    if (databaseExists.Success)
                    {
                        splash.SetProgress(25);
                    }
                    else
                    {
                        // need to show message box eventually, but just log for now and increment the progress
                        Console.WriteLine("Error when checking the database for existence");
                        splash.SetProgress(25);
                    }
                    
                    splash.SetProgress(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Initialization error: {ex.Message}");
                }
                finally
                {
                    var mainWin = new MainWindow();
                    mainWin.DataContext = new MainWindowViewModel();
                    desktop.MainWindow = mainWin;
                    mainWin.Show();
    
                    splash.Close();
                }
            }
    
            base.OnFrameworkInitializationCompleted();
        }
    }
}