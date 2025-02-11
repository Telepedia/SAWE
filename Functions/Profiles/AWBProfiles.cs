using AWBv2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Functions.Profiles
{
    public class AWBProfiles
    {
        /// <summary>
        /// Hold all of the profiles; placeholder for now
        /// </summary>
        public List<Profile> Profiles { get; set; } = new List<Profile>
        {
            new Profile { ID = 1, Username = "Alice", Password = "PasswordAlice" },
            new Profile { ID = 2, Username = "Bob", Password = "PasswordBob" },
            new Profile { ID = 3, Username = "Charlie", Password = "PasswordCharlie" }
        };

        /// <summary>
        /// Return the Profiles property
        /// </summary>
        /// <returns></returns>
        public List<Profile> GetProfiles()
        {
            return Profiles;
        }

        /// <summary>
        /// Removes a profile from the list and returns a task indicating success.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public Task<bool> DeleteProfile(Profile profile)
        {
            // only do something if the profile is actually in the list
            if (Profiles.Contains(profile))
            {
                Profiles.Remove(profile);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}