using System;
using System.Collections.ObjectModel;
using ReactiveUI;
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
    
    [Reactive] public string Username { get; set; }
    [Reactive] public string Password { get; set; }
    [Reactive] public Profile SelectedProfile { get; set; }
    
    [Reactive] public string Wiki { get; set; }
    
    [Reactive] public bool SavePassword { get; set; } = false;
    
    [Reactive] public bool IsLoggingIn { get; set; } = false;
    
    // placeholder for any error message that might occur during this process, since Avalonia
    // doesn't natively support messagebox or anything like that.
    [Reactive] public string ErrorMessage { get; set; } = string.Empty;

    // do we have an error? if so, we show the error message above
    [Reactive] public bool HasError { get; set; } = false;
    
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public Interaction<Unit, Unit> CloseWindow { get; }

    public Interaction<Wiki, Unit> LoginSuccess { get; } = new();
    
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    
    public ProfileWindowViewModel()
    {
        CloseWindow = new Interaction<Unit, Unit>();
        var canLogin = this.WhenAnyValue(
            x => x.Username,
            x => x.Password,
            x => x.Wiki,
            x => x.SelectedProfile,
            (user, pass, wiki, profile) => 
                profile != null ||
                (!string.IsNullOrWhiteSpace(user) && 
                 !string.IsNullOrWhiteSpace(pass) &&
                 !string.IsNullOrWhiteSpace(wiki))
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
        // Clear previous errors
        HasError = false;
        ErrorMessage = "";
        IsLoggingIn = true;
        
        try
        {
            Profile loginProfile = null;

            // Determine if we're using selected profile or manually entered data
            if (SelectedProfile != null)
            {
                loginProfile = SelectedProfile;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Username) || 
                    string.IsNullOrWhiteSpace(Password) || 
                    string.IsNullOrWhiteSpace(Wiki))
                {
                    ErrorMessage = "You must provide fields for username, password, and wiki";
                    HasError = true;
                    return;
                }

                loginProfile = new Profile
                {
                    Username = Username,
                    Password = Password,
                    Wiki = Wiki
                };
            }

            // Handle password saving only for NEW profiles
            if (SavePassword && SelectedProfile == null)
            {
                var saveResult = await AWBProfiles.Save(loginProfile);
                if (!saveResult)
                {
                    ErrorMessage = "Failed to save profile. Please try again.";
                    HasError = true;
                    return;
                }
            }
            
            var wiki = await Functions.Wiki.CreateAsync(loginProfile.Wiki);
            await wiki.ApiClient.LoginUserAsync(loginProfile.Username, loginProfile.Password);
            await wiki.ApiClient.FetchUserInformationAsync();
            
            Console.WriteLine($"Successfully logged in as: {wiki.User.Username}");
            
            // Pass the Wiki object to the main window so that we do not need to create a new instance
            // hopefuilly to save a bit of processing time
            await LoginSuccess.Handle(wiki);
            await CloseWindow.Handle(Unit.Default);
        }
        catch (UnauthorizedAccessException ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to login: {ex.Message}");
            ErrorMessage = $"Login failed: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoggingIn = false;
        }
    }
    
    /// <summary>
    /// Load all of the profiles. For now, just returns the list at the top of Profiles.cs
    /// Eventually, the profiles there will be loaded from the database at application startup and be
    /// available on that class for accessing.
    /// </summary>
    public async Task LoadProfilesAsync()
    {
        
        var profilesList = AWBProfiles.GetProfiles();
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
            bool result = await AWBProfiles.DeleteProfile(SelectedProfile);
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

        // reload the profiles after deleting one
        await LoadProfilesAsync();
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