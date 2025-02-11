using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace AWBv2.Views;

public partial class Splash : Window
{
    public Splash()
    {
        InitializeComponent();
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        Version.Text = $"v. {version}";
        SetProgress(0);
    }
    
    private void SetProgress(int percent)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            ProgressBar.Value = percent;
        });
    }

    public async Task RunInitializationAsync()
    {
        for (int i = 0; i <= 100; i++)
        {
            SetProgress(i);
            await Task.Delay(50); 
        }
    }
}