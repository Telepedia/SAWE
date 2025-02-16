using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using AWBv2.Controls;
using AWBv2.Models;
using Functions;

namespace AWBv2.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private AWBWebBrowser _webBrowser;
    private string _lblUsername = string.Empty;
    private string _lblProject = "None";
    private int _lblNewArticles = 0;
    private int _lblIgnoredArticles = 0;
    private int _lblEditCount = 0;
    private int _lblEditsPerMin = 0;
    private int _lblPagesPerMin = 0;
    private int _lblTimer = 0;
    private bool _isMinorEdit = false;

    private Wiki Wiki { get; set; }
    
    public ReactiveCommand<Unit, Unit> OpenProfileWindow { get; }
    public ReactiveCommand<Unit, Unit> RequestClose { get; }
    public Interaction<ProfileWindowViewModel, Unit> ShowProfileWindowInteraction { get; }
    public Interaction<Unit, Unit> CloseWindowInteraction { get; }

    public AWBWebBrowser WebBrowser
    {
        get => _webBrowser ??= new AWBWebBrowser();
        set => this.RaiseAndSetIfChanged(ref _webBrowser, value);
    }

    public bool IsMinorEdit
    {
        get => _isMinorEdit;
        set => this.RaiseAndSetIfChanged(ref _isMinorEdit, value);
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

    public MainWindowViewModel()
    {
        ShowProfileWindowInteraction = new Interaction<ProfileWindowViewModel, Unit>();
        CloseWindowInteraction = new Interaction<Unit, Unit>();

        OpenProfileWindow = ReactiveCommand.CreateFromTask(ShowProfileWindow);
        RequestClose = ReactiveCommand.CreateFromTask(CloseWindow);
    }

    private async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }

    private async Task ShowProfileWindow()
    {
        var profileVM = new ProfileWindowViewModel();
        await ShowProfileWindowInteraction.Handle(profileVM);
    }
    
    public async Task HandleProfileLogin(Profile profile)
    {
        
        try
        {
            Wiki = await Wiki.CreateAsync(profile.Wiki);
            await Wiki.ApiClient.LoginUserAsync(profile.Username, profile.Password);
            await Wiki.ApiClient.FetchUserInformationAsync();
            
            // deeeeeeebug
            Console.WriteLine(JsonSerializer.Serialize(Wiki.User, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to init wiki: {ex.Message}");
            throw;
        }

        LblProject = Wiki.Sitename;
        LblUsername = Wiki.User.Username;
    }
}
