using System;
using System.Reactive;
using AWBv2.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AWBv2.ViewModels;

public class ProcessOptionsViewModel : ReactiveObject
{
    /// <summary>
    /// Should we do general fixes on the pages in the list?
    /// </summary>
    [Reactive] public bool GeneralFixes { get; set; } = false;
    
    /// <summary>
    /// Should we convert unicode to their proper symbols? 
    /// </summary>
    [Reactive] public bool UnicodifyWholePage { get; set; } = false;
    
    /// <summary>
    /// Should we enable find and replace? (This toggle is for both standard F&R and the template subst)
    /// </summary>
    [Reactive] public bool FindAndReplace { get; set; } = false;
    
    /// <summary>
    /// Command that is fired to open the find and replace window, heh.
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenFindReplaceCommand { get; }

    public FindReplaceViewModel FindReplaceViewModel { get; } = new FindReplaceViewModel();
    
    /// <summary>
    /// Command emitted when the start button is clicked
    /// </summary>
    public ReactiveCommand<Unit, Unit> StartProcessingCommand { get; }
    
    /// <summary>
    /// Command emitted when the save button is clicked
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    
    /// <summary>
    /// Command emitted when the skip button is clicked
    /// </summary>
    public ReactiveCommand<Unit, Unit> SkipCommand { get; }

    /// <summary>
    /// Command emitted when the stop button is clicked
    /// </summary>
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    
    [Reactive] public string EditSummary { get; set; } = string.Empty;

    /// <summary>
    /// Should we autosave?
    /// </summary>
    [Reactive] public bool AutoSave { get; set; } = false;
    
    /// <summary>
    /// How long should we delay between each save?
    /// </summary>
    [Reactive] public int EditDelay { get; set; } = 0;
    
    public ProcessOptionsViewModel()
    {
        OpenFindReplaceCommand = ReactiveCommand.Create(OpenFindReplace);
        
        // Below are all commands that emit events to be listened to by MainWindowViewModel, they do not trigger
        // a method in this class like OpenFindReplaceCommand
        StartProcessingCommand = ReactiveCommand.Create(() => { });
        SaveCommand = ReactiveCommand.Create(() => { });
        SkipCommand = ReactiveCommand.Create(() => { });
        StopCommand = ReactiveCommand.Create(() => { });
    }
    
    /// <summary>
    /// Open the find and replace (standard) window
    /// </summary>
    private void OpenFindReplace()
    {
        var window = new FindReplaceWindow(FindReplaceViewModel);
        window.Show();
    }
    
}