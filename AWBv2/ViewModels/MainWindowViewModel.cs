using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AWBv2.Controls;
using AWBv2.Views;
using ReactiveUI;

namespace AWBv2.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly Window _window;
    private AWBWebBrowser _webBrowser;
    private string _lblUsername = string.Empty;
    private string _lblProject = "Wikipedia";
    private int _lblNewArticles = 0;
    private int _lblIgnoredArticles = 0;
    private int _lblEditCount = 0;
    private int _lblEditsPerMin = 0;
    private int _lblPagesPerMin = 0;
    private int _lblTimer = 0;

    public ReactiveCommand<Unit, Unit> OpenProfileWindow { get; }

    public AWBWebBrowser WebBrowser
    {
        get => _webBrowser;
        set => this.RaiseAndSetIfChanged(ref _webBrowser, value);
    }

    public string LblUsername
    {
        get => _lblUsername;
        set => this.RaiseAndSetIfChanged(ref _lblUsername, value);
    }

    public string LblProject
    {
        get => _lblProject;
        set => this.RaiseAndSetIfChanged(ref _lblProject, value);
    }

    public int LblNewArticles
    {
        get => _lblNewArticles;
        set => this.RaiseAndSetIfChanged(ref _lblNewArticles, value);
    }

    public int LblIgnoredArticles
    {
        get => _lblIgnoredArticles;
        set => this.RaiseAndSetIfChanged(ref _lblIgnoredArticles, value);
    }

    public int LblEditCount
    {
        get => _lblEditCount;
        set => this.RaiseAndSetIfChanged(ref _lblEditCount, value);
    }

    public int LblEditsPerMin
    {
        get => _lblEditsPerMin;
        set => this.RaiseAndSetIfChanged(ref _lblEditsPerMin, value);
    }

    public int LblPagesPerMin
    {
        get => _lblPagesPerMin;
        set => this.RaiseAndSetIfChanged(ref _lblPagesPerMin, value);
    }

    public int LblTimer
    {
        get => _lblTimer;
        set => this.RaiseAndSetIfChanged(ref _lblTimer, value);
    }

    public MainWindowViewModel(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        
        OpenProfileWindow = ReactiveCommand.CreateFromTask(ShowProfileWindow);
    }

    private async Task ShowProfileWindow()
    {
        var profileWindow = new ProfileWindow
        {
            DataContext = new ProfileWindowViewModel()
        };

        // Get the main window reference properly
        var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        
        await profileWindow.ShowDialog(mainWindow);
    }

    public void CloseWindow()
    {
        _window.Close();
    }
}