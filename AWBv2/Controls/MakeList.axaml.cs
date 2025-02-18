using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AWBv2.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace AWBv2.Controls;

public partial class MakeList : UserControl
{   
 
    private MakeListViewModel ViewModel { get; set; }
    
    public MakeList()
    {
        InitializeComponent();
        ViewModel = new MakeListViewModel();
        DataContext = ViewModel;
    }
}