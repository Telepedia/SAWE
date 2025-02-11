using Microsoft.Data.Sqlite;

namespace Functions;

public class Setup
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
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                await connection.OpenAsync();
            
                using var command = connection.CreateCommand();
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
}