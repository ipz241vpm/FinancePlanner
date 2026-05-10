using Microsoft.Data.Sqlite;

public class DatabaseInitializer
{
    public static void Initialize()
    {
        var connection = DatabaseConnection.Instance.GetConnection();


        var command = connection.CreateCommand();

        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL UNIQUE,
            PasswordHash TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Categories (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Type TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Transactions (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Amount REAL NOT NULL,
            Date TEXT NOT NULL,
            CategoryId INTEGER,
            Type TEXT NOT NULL,
            Description TEXT,
            FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
        );

        CREATE TABLE IF NOT EXISTS Budgets (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CategoryId INTEGER,
            [Limit] REAL,
            Month INTEGER,
            Year INTEGER,
            FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
        );
        ";

        command.ExecuteNonQuery();

        try
        {
            var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = "ALTER TABLE Transactions ADD COLUMN UserId INTEGER DEFAULT 0;";
            alterCommand.ExecuteNonQuery();
        }
        catch { /* Ігноруємо помилку, якщо колонка вже існує */ }

        try
        {
            var alterCategoriesCommand = connection.CreateCommand();
            alterCategoriesCommand.CommandText = "ALTER TABLE Categories ADD COLUMN UserId INTEGER DEFAULT 0;";
            alterCategoriesCommand.ExecuteNonQuery();
        }
        catch { /* Ігноруємо помилку, якщо колонка вже існує */ }

        try
        {
            var alterCategoriesCommand2 = connection.CreateCommand();
            alterCategoriesCommand2.CommandText = "ALTER TABLE Categories ADD COLUMN ProjectedAmount REAL DEFAULT 0;";
            alterCategoriesCommand2.ExecuteNonQuery();
        }
        catch { /* Ігноруємо помилку, якщо колонка вже існує */ }

        void SafeAddColumn(string cmdText)
        {
            try
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = cmdText;
                cmd.ExecuteNonQuery();
            }
            catch { /* Ігноруємо помилку, якщо колонка вже існує */ }
        }   

        SafeAddColumn("ALTER TABLE Categories ADD COLUMN IsRecurring INTEGER DEFAULT 1;");
        SafeAddColumn("ALTER TABLE Categories ADD COLUMN TargetMonth INTEGER DEFAULT 0;");
        SafeAddColumn("ALTER TABLE Categories ADD COLUMN TargetYear INTEGER DEFAULT 0;");
        SafeAddColumn("ALTER TABLE Categories ADD COLUMN StartMonth INTEGER DEFAULT 0;");
        SafeAddColumn("ALTER TABLE Categories ADD COLUMN StartYear INTEGER DEFAULT 0;");
        SafeAddColumn("ALTER TABLE Categories ADD COLUMN EndMonth INTEGER DEFAULT 0;");
        SafeAddColumn("ALTER TABLE Categories ADD COLUMN EndYear INTEGER DEFAULT 0;");
    }
}