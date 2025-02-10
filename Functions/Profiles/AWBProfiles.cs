using AWBv2.Models;

namespace Functions.Profiles;

public class AWBProfiles
{
    public List<Profile> GetProfiles()
    {
        // testing for now whilst I work on the UI; just return some bogus stuff thank you kindly
        return [
            new Profile { ID = 1, Username = "Alice",   Password = "PasswordAlice" },
            new Profile { ID = 2, Username = "Bob",     Password = "PasswordBob" },
            new Profile { ID = 3, Username = "Charlie", Password = "PasswordCharlie" }
        ];
    }
}