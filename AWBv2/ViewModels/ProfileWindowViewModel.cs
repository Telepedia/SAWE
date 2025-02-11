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
    [Reactive] public Profile SelectedProfile { get; set; }
    
    // placeholder for any error message that might occur during this process, since Avalonia
    // doesn't natively support messagebox or anything like that.
    [Reactive] public string ErrorMessage { get; set; } = string.Empty;
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public Interaction<Unit, Unit> CloseWindow { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
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
        
        var canEdit = this.WhenAnyValue(x => x.SelectedProfile)
            .Select(profile => profile != null);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteProfileAsync, canEdit);
        EditCommand = ReactiveCommand.CreateFromTask(EditProfileAsync, canEdit);
        
        // when the value of either the username or password changes, clear the error message for safety
        this.WhenAnyValue(x => x.Username, x => x.Password)
            .Subscribe(_ => ErrorMessage = string.Empty);
    }

    /// <summary>
    /// Perform the login; doesn't do much at the moment except close the window.
    /// </summary>
    private async Task PerformLogin()
    {
      Variables.HttpAuthUsername = Username;
      Variables.HttpAuthPassword = Password;
      await CloseWindow.Handle(Unit.Default);
    }
    
    /// <summary>
    /// Load all of the profiles. For now, just returns the list at the top of Profiles.cs
    /// Eventually, the profiles there will be loaded from the database at application startup and be
    /// available on that class for accessing.
    /// </summary>
    public async Task LoadProfilesAsync()
    {
        
        var profilesList = _profileService.GetProfiles();
        Profiles.Clear();
        foreach (var profile in profilesList)
        {
            Profiles.Add(profile);
        }
    }
    
    /// <summary>
    /// Delete a profile from the profile service/list and also remove it from the UI; optionally showing the user
    /// an error if there was an issue.
    /// </summary>
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile != null)
        {
            bool result = await _profileService.DeleteProfile(SelectedProfile);
            if (result)
            {
                // Also remove from the ObservableCollection so the UI updates if we were successful; if not, then
                // don't, I suppose.
                Profiles.Remove(SelectedProfile);
                ErrorMessage = string.Empty;
                SelectedProfile = null;
            }
            else
            {
                ErrorMessage = "Error deleting profile. Please try again later.";
            }
        }
    }
    
    /// <summary>
    /// Command to edit the profile of the user; this is to change the username and/or the password.
    /// for now just log the ID  to the command line so we can see it is working at this moment.
    /// </summary>
    private async Task EditProfileAsync()
    {
        if (SelectedProfile != null)
        {
            // placeholder, just write the profile to the command line, thank you
            Console.WriteLine(SelectedProfile.ID);
        }
    }
}