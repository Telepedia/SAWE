using Functions.Profiles;
using Microsoft.Data.Sqlite;

namespace Functions;

public sealed class Setup
{
    /// <summary>
    /// Checks if the database exists, if not, create it and return back to the caller
    /// </summary>
    public async Task<(bool Success, Exception? Error)> CheckDatabaseAsync()
    {
        try
        {
            string dbPath = Path.Combine(Globals.GetDataDirPath(), "awbv2.db");
        
            // Only create the DB if it doesn't exist
            if (!File.Exists(dbPath))
            {
                // @TODO: we need to also generate some kind of secret key which can be used to 
                // encrypt the passwords. This needs to be done when the database is created
                // so that we only create the secret key once when we create the database; if
                // the database already exists, assume there is already a secret key stored
                // in the secure storage which has been used, or can be used, to encrypt passwords
                await using var connection = new SqliteConnection($"Data Source={dbPath}");
                await connection.OpenAsync();
            
                await using var command = connection.CreateCommand();
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Profiles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL,
                    Password TEXT NOT NULL,
                    OAuth INTEGER NOT NULL
                );";
            
                await command.ExecuteNonQueryAsync();
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }

    public async Task<bool> LoadProfilesAsync()
    {
        await AWBProfiles.LoadProfilesAsync();
        return true;
    }
}