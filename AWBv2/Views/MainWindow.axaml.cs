using System;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using AWBv2.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;

namespace AWBv2.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                ViewModel?.CloseWindowInteraction.RegisterHandler(interaction =>
                {
                    Close();
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
                
                ViewModel?.ShowProfileWindowInteraction.RegisterHandler(async interaction =>
                {
                    var profileWindow = new ProfileWindow
                    {
                        DataContext = interaction.Input
                    };
                    
                    await profileWindow.ShowDialog(this);
                    interaction.SetOutput(Unit.Default);
                }).DisposeWith(disposables);
            });
        }
    }
}