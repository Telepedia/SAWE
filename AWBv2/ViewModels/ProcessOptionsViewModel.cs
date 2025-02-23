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
    
    public ProcessOptionsViewModel() {}
}