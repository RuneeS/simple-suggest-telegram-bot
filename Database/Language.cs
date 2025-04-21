using Microsoft.Data.Sqlite;

public class UserSettingsRepository
{
    private readonly string _connectionString;
    private const string DBFile = "Users.db";

    public UserSettingsRepository()
    {
        _connectionString = $"Data Source={DBFile}";
        EnsureTableExists();
    }

    private void EnsureTableExists()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createTableCmd = connection.CreateCommand();
        createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                UserId INTEGER PRIMARY KEY,
                Language TEXT NOT NULL
            );
        ";
        createTableCmd.ExecuteNonQuery();
    }

    public void SetLanguage(long userId, string languageCode)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Users (UserId, Language)
            VALUES ($userId, $language)
            ON CONFLICT(UserId) DO UPDATE SET Language = $language;
        ";
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.Parameters.AddWithValue("$language", languageCode);

        cmd.ExecuteNonQuery();
    }

    public string GetLanguage(long userId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Language FROM Users WHERE UserId = $userId LIMIT 1;";
        cmd.Parameters.AddWithValue("$userId", userId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetString(0);
        }

        return "en"; // default language
    }
}
