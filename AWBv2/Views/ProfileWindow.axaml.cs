using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AWBv2.ViewModels;
using ReactiveUI;

namespace AWBv2.Views;

public partial class ProfileWindow : ReactiveWindow<ProfileWindowViewModel>
{
    public ProfileWindow()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            ViewModel.LoadProfilesAsync().ConfigureAwait(false);
            ViewModel.CloseWindow.RegisterHandler(interaction =>
            {
                this.Close();
                interaction.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
        });
    }
}