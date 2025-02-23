using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Functions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AWBv2.ViewModels;

public class FindReplaceViewModel : ReactiveObject
{
    public ObservableCollection<FindAndReplaceEntry> Entries { get; set; } = new();

    [Reactive] public FindAndReplaceEntry? SelectedEntry { get; set; }
    
    public ReactiveCommand<Unit, Unit> AddEntryCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveEntryCommand { get; }

    public FindReplaceViewModel()
    {
        AddEntryCommand = ReactiveCommand.Create(AddEntry);
       
        RemoveEntryCommand = ReactiveCommand.Create(RemoveEntry, 
            this.WhenAnyValue(x => x.SelectedEntry).Select(entry => entry != null));
    }
    
    /// <summary>
    /// Add a new find and replace to the list
    /// </summary>
    private void AddEntry()
    {
        // for some reason this doesn't work quite yet, idk what's happening, but thats for another day.
        Entries.Add(new FindAndReplaceEntry());
    }

    /// <summary>
    /// Remove/delete a find and replace from the list
    /// </summary>
    /// <param name="entry">The entry for us to remove</param>
    private void RemoveEntry()
    {
        if (SelectedEntry != null)
            Entries.Remove(SelectedEntry);
    }
    
}