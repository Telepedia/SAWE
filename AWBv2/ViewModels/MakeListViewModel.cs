using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AWBv2.ViewModels;

public class MakeListViewModel : ReactiveObject
{
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
        
        // when the value of the selection box on the make list control changes, update the thingy  property so it shows
        this.WhenAnyValue(x => x.SelectedMakeOption)
            .Subscribe(option =>
            {
                ShowAdditionalTextBox = !(option == "None" || optionsDontShowTextBox.Contains(option));
                MakeOptionText = string.Empty;
            });
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
}