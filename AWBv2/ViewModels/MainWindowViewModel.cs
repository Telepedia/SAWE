using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using AWBv2.Controls;
using AWBv2.Models;
using Functions;
using Functions.Article;
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
    [Reactive] public ObservableCollection<Article> Articles { get; set; } = new();
    [Reactive] public string EditBoxContent { get; set; } = string.Empty;
    private Wiki Wiki { get; set; }
    
    public ReactiveCommand<Unit, Unit> OpenProfileWindow { get; }
    public ReactiveCommand<Unit, Unit> RequestClose { get; }
    public Interaction<ProfileWindowViewModel, Unit> ShowProfileWindowInteraction { get; }
    public Interaction<Unit, Unit> CloseWindowInteraction { get; }
    
    private CancellationTokenSource _cts;
    
    private TaskCompletionSource<bool> _saveTcs = new TaskCompletionSource<bool>();

    public AWBWebBrowser WebBrowser
    {
        get => _webBrowser ??= new AWBWebBrowser();
        set => this.RaiseAndSetIfChanged(ref _webBrowser, value);
    }
    
    [Reactive] public MakeListViewModel MakeListViewModel { get; set; }
    
    [Reactive] public ProcessOptionsViewModel ProcessOptionsViewModel { get; set; }
    
    public ReactiveCommand<Unit, Unit> ProcessArticlesCommand { get; }
    
    public MainWindowViewModel()
    {
        ShowProfileWindowInteraction = new Interaction<ProfileWindowViewModel, Unit>();
        CloseWindowInteraction = new Interaction<Unit, Unit>();

        OpenProfileWindow = ReactiveCommand.CreateFromTask(ShowProfileWindow);
        RequestClose = ReactiveCommand.CreateFromTask(CloseWindow);
        MakeListViewModel = new MakeListViewModel();
        ProcessOptionsViewModel = new ProcessOptionsViewModel();
        
        ProcessOptionsViewModel.StartProcessingCommand.Subscribe(async _ =>
        {
            _cts = new CancellationTokenSource();
            try
            {
                await ProcessArticlesAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Debug]: Processing was canceled; no further processing will occur.");
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
            
        });
        
        // when the stop button is clicked, cancel the token, which signals to the while loop in 
        // self::ProcessArticlesAsync() that it should stop looping
        ProcessOptionsViewModel.StopCommand.Subscribe(_ =>
        {
            _cts?.Cancel();
        });
        
        // when the skip button is called, set the result as false so that we know to skip the page and move onto the 
        // next page in the list
        ProcessOptionsViewModel.SkipCommand.Subscribe(async _ =>  
        {
            _saveTcs?.TrySetResult(false);
        });
        
        // when the save button is clicked, set the result as true on the token so that we can move onto the
        // next article in the list
        ProcessOptionsViewModel.SaveCommand.Subscribe(_ =>
        {
            _saveTcs?.TrySetResult(true);
        });
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
            MakeListViewModel.Initialize(Wiki);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to init wiki: {ex.Message}");
            throw;
        }

        LblProject = Wiki.Sitename;
        LblUsername = Wiki.User.Username;
    }

    private async Task ProcessArticlesAsync(CancellationToken ct)
    {
        
        bool autoSave = ProcessOptionsViewModel.AutoSave;
        int editDelay = ProcessOptionsViewModel.EditDelay;
        string editSummary = ProcessOptionsViewModel.EditSummary;
        
        // we need a copy of the pages collection becasue its not safe to remove from it
        // whilst it is being iterated over.
        // maybe we shouldn't be accessing the contents of the view model like this? idek, it works though so fuck it
        List<string> pagesToProcess = MakeListViewModel.Pages.ToList();
        
        foreach (string pageTitle in pagesToProcess)
        {
            // if we requested cancellation, by clicking the stop button, exit.
            if (ct.IsCancellationRequested) break;
            
            try
            {
                var article = await Wiki.ApiClient.GetArticleAsync(pageTitle);
                
                if (article == null)
                {
                    LblIgnoredArticles++;
                    continue;
                }
                
                Articles.Add(article);
                    
                // set the content for editing, we will need this for later so that we can edit the content
                // that SAWE may change.
                EditBoxContent = article.OriginalArticleText;
                
                // wait for the delay if needed 
                if (autoSave)
                {
                    // save the article (placeholder, we will get the article back here from the changes we made, and
                    // then we save the changes
                    await DoChanges(article, editSummary);
                    await Task.Delay(editDelay * 1000, ct);
                }
                else
                {
                    _saveTcs = new TaskCompletionSource<bool>();
                    using (ct.Register(() => _saveTcs.TrySetCanceled()))
                    {
                        try
                        {
                            var save = await _saveTcs.Task;
                            if (save)
                            {
                                await DoChanges(article, editSummary);
                                LblEditCount++;
                            }
                            else
                            {
                                LblIgnoredArticles++;
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                        finally
                        {
                            _saveTcs = null;
                        }
                    }
                }

                MakeListViewModel.Pages.Remove(pageTitle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {pageTitle}: {ex.Message}");
            }
            finally
            {
                EditBoxContent = string.Empty;
            }
        }
    }

    /// <summary>
    /// Encapsulate all the logic for doing the changes here (such as calling out to other methods)
    /// To keep the main loop for processing the articles clear.
    /// </summary>
    /// <param name="article"></param>
    /// <param name="editSummary"></param>
    /// <returns></returns>
    private Task DoChanges(Article article, string editSummary)
    {
        // placeholder for now
        Console.WriteLine($"Title: {article.Name}, Summary: {editSummary}");
        return Task.CompletedTask;
    }
}
