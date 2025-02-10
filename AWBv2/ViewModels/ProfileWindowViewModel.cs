using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.Fody;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AWBv2.Models;
using Functions;
using Functions.Profiles;
using ReactiveUI.Fody.Helpers;

namespace AWBv2.ViewModels;

public class ProfileWindowViewModel : ReactiveObject
{
    public ObservableCollection<Profile> Profiles { get; } = new ObservableCollection<Profile>();
    private readonly AWBProfiles _profileService = new AWBProfiles();
    
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
    
    public async Task LoadProfilesAsync()
    {
        
        var profilesList = _profileService.GetProfiles();
        Profiles.Clear();
        foreach (var profile in profilesList)
        {
            Profiles.Add(profile);
        }
    }
}