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

    [Reactive] public string EditSummary { get; set; } = string.Empty;
    
    public ProcessOptionsViewModel()
    {
        OpenFindReplaceCommand = ReactiveCommand.Create(OpenFindReplace);
        StartProcessingCommand = ReactiveCommand.Create(() => { });
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