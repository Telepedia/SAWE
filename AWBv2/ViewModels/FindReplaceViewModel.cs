using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using AWBv2.Models;
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
        
        Entries.Add(new FindAndReplaceEntry { Find = "old text 1", Replace = "new text 1", CaseSensitive = true });
        Entries.Add(new FindAndReplaceEntry { Find = "old text 2", Replace = "new text 2", Regex = true });
        Entries.Add(new FindAndReplaceEntry { Find = "old text 3", Replace = "new text 3", Enabled = false });
    }
    
    /// <summary>
    /// Add a new find and replace to the list
    /// </summary>
    private void AddEntry()
    {
        try
        {
            Entries.Add(new FindAndReplaceEntry());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding entry: {ex}");
        }
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