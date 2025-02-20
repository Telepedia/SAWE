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
using ReactiveUI.Fody.Helpers;

namespace AWBv2.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private AWBWebBrowser _webBrowser;
    [Reactive] public bool IsMinorEdit { get; set; } = false;
    [Reactive] public string LblUsername { get; set; } = string.Empty;
    [Reactive] public string LblProject { get; set; } = string.Empty;
    [Reactive] public int LblNewArticles { get; set; } = 0;
    [Reactive] public int LblIgnoredArticles { get; set; } = 0;
    [Reactive] public int LblEditCount { get; set; } = 0;
    [Reactive] public int LblEditsPerMin { get; set; } = 0;
    [Reactive] public int LblPagesPerMin { get; set; } = 0;
    [Reactive] public int LblTimer { get; set; } = 0;

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
