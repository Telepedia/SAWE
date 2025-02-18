using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AWBv2.ViewModels;

public class MakeListViewModel : ReactiveObject
{
    public ObservableCollection<string> Pages { get; set; } = new();

    /// <summary>
    /// This holds the value of the page title in the input box for adding a page
    /// </summary>
    [Reactive] public string PageTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// This handles the selected page in the input box (not implemented yet heh)
    /// is nullable given we won't always be selecting a page
    /// </summary>
    [Reactive] public string? SelectedPage { get; set; }

    public MakeListViewModel()
    {
    }
}