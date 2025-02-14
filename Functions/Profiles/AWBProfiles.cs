using AWBv2.Models;
using Microsoft.Data.Sqlite;

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
        /// The location of the database; uses Globals.GetDataDirPath() which will account for
        /// different OS'
        /// </summary>
        /// <returns></returns>
        private static string GetDatabasePath() => Path.Combine(Globals.GetDataDirPath(), "awbv2.db");

        /// <summary>
        /// Get the encryption key from the EncryptionHelper
        /// </summary>
        /// <returns></returns>
        private static byte[]? GetEncryptionKey() => EncryptionHelper.GetEncryptionKey();
        
        /// <summary>
        /// Load all of the profiles onto the instance. This is called during app startup
        /// </summary>
        /// <returns></returns>
        public static async Task LoadProfilesAsync()
        {
            string dbPath = GetDatabasePath();

            if (!File.Exists(dbPath))
            {
                throw new Exception("Database not found. An error occurred during the first run of the application.");
            }

            _profiles = new List<Profile>();

            await using var connection = new SqliteConnection($"Data Source={dbPath}");
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Username, Password, IV, Wiki FROM Profiles";

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                byte[] encryptedPassword = (byte[])reader["Password"];
                byte[] iv = (byte[])reader["IV"];
                byte[] key = GetEncryptionKey() ?? throw new Exception("Encryption key not found.");

                string decryptedPassword = EncryptionHelper.Decrypt(encryptedPassword, iv, key);

                _profiles.Add(new Profile
                {
                    ID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = decryptedPassword,
                    EncryptedPassword = encryptedPassword,
                    Wiki = reader.GetString(4),
                    IV = iv
                });
            }
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
        public static async Task<bool> DeleteProfile(Profile profile)
        {
            if (!_profiles.Contains(profile)) return false;

            _profiles.Remove(profile);

            string dbPath = GetDatabasePath();
            await using var connection = new SqliteConnection($"Data Source={dbPath}");
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Profiles WHERE Id = @id";
            command.Parameters.AddWithValue("@id", profile.ID);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        
        /// <summary>
        /// Save a profile to the database
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static async Task<bool> Save(Profile profile)
        {
            string dbPath = GetDatabasePath();
            await using var connection = new SqliteConnection($"Data Source={dbPath}");
            await connection.OpenAsync();

            bool exists = _profiles.Any(p => p.ID == profile.ID);
            await using var command = connection.CreateCommand();

            byte[] key = GetEncryptionKey() ?? throw new Exception("Encryption key not found.");
            var (cipherText, iv) = EncryptionHelper.Encrypt(profile.Password, key);

            profile.EncryptedPassword = cipherText;
            profile.IV = iv;

            if (exists)
            {
                command.CommandText = @"
                    UPDATE Profiles 
                    SET Username = @username, Password = @password, IV = @iv, Wiki = @wiki
                    WHERE Id = @id";

                command.Parameters.AddWithValue("@id", profile.ID);
            }
            else
            {
                command.CommandText = @"
                    INSERT INTO Profiles (Username, Password, IV, Wiki) 
                    VALUES (@username, @password, @iv, @wiki);
                    SELECT last_insert_rowid();";
            }

            command.Parameters.AddWithValue("@username", profile.Username);
            command.Parameters.AddWithValue("@password", profile.EncryptedPassword);
            command.Parameters.AddWithValue("@iv", profile.IV);
            command.Parameters.AddWithValue("@wiki", profile.Wiki);

            if (exists)
            {
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                profile.ID = Convert.ToInt32(await command.ExecuteScalarAsync());
                _profiles.Add(profile);
            }

            return true;
        }

    }
}