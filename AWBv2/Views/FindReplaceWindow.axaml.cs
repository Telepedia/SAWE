using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AWBv2.ViewModels;

namespace AWBv2.Views;

public partial class FindReplaceWindow : ReactiveWindow<FindReplaceViewModel>
{
    public FindReplaceWindow()
    {
        InitializeComponent();
        DataContext = new FindReplaceViewModel();
    }
}