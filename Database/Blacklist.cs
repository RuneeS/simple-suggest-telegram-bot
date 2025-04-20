using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;

public class Database 
{
    private const string DBFile = "botdata.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection($"Data Source={DBFile}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS BannedUsers 
        (Id INTEGER PRIMARY KEY AUTOINCREMENT,
        TelegramId TEXT NOT NULL);";
        command.ExecuteNonQuery();
    }

    public static void AddBannedUser(string telegramId)
    {
        using var connection =  new SqliteConnection($"Data Source={DBFile}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
            INSERT INTO BannedUsers (TelegramId)
            VALUES ($id);
        ";
        command.Parameters.AddWithValue("$id", telegramId);
        command.ExecuteNonQuery();
    }

    public static void RemoveBannedUser(string telegramId)
    {
        using var connection = new SqliteConnection($"Data Source={DBFile}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
            DELETE FROM BannedUsers
            WHERE TelegramId = $id;";
            
        command.Parameters.AddWithValue("$id", telegramId);
        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            Console.WriteLine($"🟢 User {telegramId} removed from ban list.");
        }
        else
        {
            Console.WriteLine($"⚠️ User {telegramId} was not found in ban list.");
        }
    }

    public static bool IsUserBanned(string telegramId)
    {
        using var connection = new SqliteConnection($"Data Source={DBFile}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        @"
            SELECT COUNT(*) FROM BannedUsers
            WHERE TelegramId = $id;
        ";
        command.Parameters.AddWithValue("$id", telegramId);
        long count = (long)command.ExecuteScalar();
        return count > 0;
    }

}
