using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Functions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AWBv2.ViewModels;

public class MakeListViewModel : ReactiveObject
{

    /// <summary>
    /// Our wiki property; this is really fucked because I don't really understand how to get an instance
    /// </summary>
    [Reactive] private Wiki Wiki { get; set; }
    
    /// <summary>
    /// Track all of the pages in the list within a collection
    /// </summary>
    public ObservableCollection<string> Pages { get; set; } = new();
    
    /// <summary>
    /// All of the possible selections of options to make pages from
    /// </summary>
    public ObservableCollection<string> MakeOptions { get; set; } = new()
    {
        "None",
        "Category", 
        "What Links Here", 
        "All Categories", 
        "All Files", 
        "All Pages", 
        "All Redirects", 
        "Orphaned Pages",
        "Oldest Pages",
        "Random Pages"
    };

    /// <summary>
    /// Track the currently selected option to make pages in the UI
    /// </summary>
    [Reactive] public string? SelectedMakeOption { get; set; } = "None";
    
    /// <summary>
    /// Track the value of the text box that is tied to the "make list" button
    /// </summary>
    [Reactive] public string? MakeOptionText { get; set; }
    
    /// <summary>
    /// This holds the value of the page title in the input box for adding a page
    /// </summary>
    [Reactive] public string PageTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// This handles the selected page in the input box
    /// is nullable given we won't always be selecting a page
    /// </summary>
    [Reactive] public string? SelectedPage { get; set; }
    
    /// <summary>
    /// Fired when the add button is clicked to manually add a page
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddPageCommand { get; }
    
    /// <summary>
    /// Remove a page from the list of pages to work on
    /// </summary>
    public ReactiveCommand<Unit, Unit> RemovePageCommand { get; }
    
    /// <summary>
    /// Should we show the additional text box to allow user input?
    /// </summary>
    [Reactive] public bool ShowAdditionalTextBox { get; private set; }
    
    public ReactiveCommand<Unit, Unit> MakePageListCommand { get; private set; }
    
    public MakeListViewModel()
    {
        
        // only allow the button to be clicked if there is a value in the PageTitle field?!
        IObservable<bool> canAddPage = this.WhenAnyValue(
            x => x.PageTitle,  
            title => !string.IsNullOrWhiteSpace(title) && !Pages.Contains(title)
        );
        
        // only allow the removal if there is a selection and the selection is in the page list
        IObservable<bool> canRemovePage = this.WhenAnyValue(
            x => x.SelectedPage,  
            selectedPage => !string.IsNullOrWhiteSpace(selectedPage) && Pages.Contains(selectedPage)
        );
        
        AddPageCommand = ReactiveCommand.Create(AddPageToList, canAddPage);
        RemovePageCommand = ReactiveCommand.Create(RemovePageFromList, canRemovePage);
        
        List<string> optionsDontShowTextBox = new()
        {
            "All Categories",
            "All Files",
            "All Pages",
            "All Redirects",
            "Orphaned Pages",
            "Oldest Pages",
            "Random Pages"
        };
        
        // only allow the 'make' button to be clicked under certain circumstances
        // @TODO: this needs to be tweaked a bit because at present it is immediately clickable
        // once the wiki object has been set onto the view model. It shouldn't really be like that
        // it should only be clickable:
        // - if the option is not "none", but it is within the criteria that we need to provide input and input has been
        //   provided
        // - if the option is not one, and it is in the critera where we don't need any additional input (all pages, etc)
        var canMakeList = this
            .WhenAnyValue(x => x.Wiki)
            .Select(wiki => wiki != null);
        
        MakePageListCommand = ReactiveCommand.CreateFromTask(MakeList, canMakeList);
        
        // when the value of the selection box on the make list control changes, update the thingy  property so it shows
        this.WhenAnyValue(x => x.SelectedMakeOption)
            .Subscribe(option =>
            {
                ShowAdditionalTextBox = !(option == "None" || optionsDontShowTextBox.Contains(option));
                MakeOptionText = string.Empty;
            });
    }
    
    /// <summary>
    /// Dirty way for us to access the Wiki from this class
    /// </summary>
    /// <param name="wiki"></param>
    public void Initialize(Wiki wiki)
    {
        Wiki = wiki;
    }
    
    /// <summary>
    /// Manually add a page to the listbox to be worked on
    /// </summary>
    private void AddPageToList()
    {
        if (!string.IsNullOrWhiteSpace(PageTitle) && !Pages.Contains(PageTitle))
        {
            Pages.Add(PageTitle);
            PageTitle = string.Empty;
        }
    }

    /// <summary>
    /// Remove a page from the list of pages to work on
    /// </summary>
    private void RemovePageFromList()
    {
        if (!string.IsNullOrWhiteSpace(SelectedPage) && Pages.Contains(SelectedPage))
        {
            Pages.Remove(SelectedPage);
            SelectedPage = null;
        }
    }

    /// <summary>
    /// Make the list of articles to work on
    /// @TODO: decide whether we want to create an article class now for all of them or only when we are working on
    /// a specific article?
    /// </summary>
    private async Task MakeList()
    {
        Console.WriteLine(this.Wiki);
        if (Wiki == null)
        {
            Console.WriteLine("Error: Wiki is null. Ensure Initialize() was called first.");
            return;
        }

        if (Wiki.ApiClient == null)
        {
            Console.WriteLine("Error: Wiki.ApiClient is null. Login might have failed.");
            return;
        }

        if (SelectedMakeOption == "Category" && !string.IsNullOrWhiteSpace(MakeOptionText))
        {
            try
            {
                List<string> pages = await Wiki.ApiClient.GetPagesInCategoryAsync(MakeOptionText);

                if (pages.Count > 0)
                {
                    Pages.Clear();
                    foreach (string page in pages)
                    {
                        Pages.Add(page);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching pages: {ex.Message}");
            }
        }
    }

}