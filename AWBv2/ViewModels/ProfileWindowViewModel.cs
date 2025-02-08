using System;
using ReactiveUI;
using ReactiveUI.Fody;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Functions;
using ReactiveUI.Fody.Helpers;

namespace AWBv2.ViewModels;

public class ProfileWindowViewModel : ReactiveObject
{
    [Reactive] public string Username { get; set; }
    [Reactive] public string Password { get; set; }
    
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public Interaction<Unit, Unit> CloseWindow { get; }
    
    public ProfileWindowViewModel()
    {
        CloseWindow = new Interaction<Unit, Unit>();
        var canLogin = this.WhenAnyValue(
            x => x.Username,
            x => x.Password,
            (user, pass) => 
                !string.IsNullOrWhiteSpace(user) && 
                !string.IsNullOrWhiteSpace(pass)
        );

        LoginCommand = ReactiveCommand.CreateFromTask(PerformLogin, canLogin);
    }

    private async Task PerformLogin()
    {
      Variables.HttpAuthUsername = Username;
      Variables.HttpAuthPassword = Password;
      await CloseWindow.Handle(Unit.Default);
    }
}