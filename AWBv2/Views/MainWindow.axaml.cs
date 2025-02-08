using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using AWBv2.ViewModels;
using Functions;

namespace AWBv2.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        WebBrowser.Navigate("https://en.wikipedia.org");
    }
}