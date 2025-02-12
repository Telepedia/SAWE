using AWBv2.Models;

namespace Functions.Profiles
{
    public static class AWBProfiles
    {

        /// <summary>
        /// Static member so that is available for the lifetime of the application
        /// and will not be recycled by the garbage collector (hopefully)
        /// </summary>
        private static List<Profile> _profiles = [];


        /// <summary>
        /// Load all of the profiles onto the instance. This is called during app startup
        /// </summary>
        /// <returns></returns>
        public static async Task LoadProfilesAsync()
        {
           
            _profiles = new List<Profile>
            {
                new Profile { ID = 1, Username = "Alice", Password = "PasswordAlice" },
                new Profile { ID = 2, Username = "Bob", Password = "PasswordBob" },
                new Profile { ID = 3, Username = "Charlie", Password = "PasswordCharlie" }
            };

            // temp sim of async work heh
            await Task.CompletedTask;
        }

        /// <summary>
        /// Return the Profiles property
        /// </summary>
        /// <returns></returns>
        public static List<Profile> GetProfiles() => _profiles;

        /// <summary>
        /// Removes a profile from the list and returns a task indicating success.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static Task<bool> DeleteProfile(Profile profile)
        {
            // only do something if the profile is actually in the list
            if (_profiles.Contains(profile))
            {
                _profiles.Remove(profile);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}