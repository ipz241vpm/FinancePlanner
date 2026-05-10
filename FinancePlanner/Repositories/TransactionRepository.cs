using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Transactions;
using FinancePlanner.Services;
using FinancePlanner.Repositories.Interfaces;
/// <summary>
/// Репозиторій для управління транзакціями в БД SQLite.
/// Реалізує патерн Repository для ізоляції логіки доступу до даних.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    public void Add(Transaction transaction)
    {
        var connection = DatabaseConnection.Instance.GetConnection();

        var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Transactions (Amount, Date, CategoryId, Type, Description, UserId)
            VALUES ($amount, $date, $categoryId, $type, $description, $userId);
            ";

        command.Parameters.AddWithValue("$amount", transaction.Amount);
        command.Parameters.AddWithValue("$date", transaction.Date.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("$categoryId", transaction.CategoryId == 0 ? (object)DBNull.Value : transaction.CategoryId);
        command.Parameters.AddWithValue("$type", transaction.Type);
        command.Parameters.AddWithValue("$description", transaction.Description ?? (object)DBNull.Value);
        
        int currentUserId = SessionManager.Instance.CurrentUser?.Id ?? 0;
        command.Parameters.AddWithValue("$userId", currentUserId);

        command.ExecuteNonQuery();
    }

    public List<Transaction> GetAll()
    {
        var list = new List<Transaction>();

        var connection = DatabaseConnection.Instance.GetConnection();

        var command = connection.CreateCommand();
        int currentUserId = SessionManager.Instance.CurrentUser?.Id ?? 0;
        command.CommandText = "SELECT * FROM Transactions WHERE UserId = $userId ORDER BY Date DESC"; 
        command.Parameters.AddWithValue("$userId", currentUserId);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            // Мапінг даних з БД на об'єкт моделі Transaction
            list.Add(new Transaction
            {
                Id = reader.GetInt32(0),
                Amount = reader.GetDecimal(1),
                Date = DateTime.Parse(reader.GetString(2)),
                CategoryId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                Type = reader.GetString(4),
                Description = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                UserId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6)
            });
        }

        return list;
    }

    public void Update(Transaction transaction)
    {
        var connection = DatabaseConnection.Instance.GetConnection();
        var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Transactions 
            SET Amount = $amount, 
                Date = $date, 
                CategoryId = $categoryId, 
                Type = $type, 
                Description = $description 
            WHERE Id = $id AND UserId = $userId;
            ";

        command.Parameters.AddWithValue("$amount", transaction.Amount);
        command.Parameters.AddWithValue("$date", transaction.Date.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("$categoryId", transaction.CategoryId == 0 ? (object)DBNull.Value : transaction.CategoryId);
        command.Parameters.AddWithValue("$type", transaction.Type);
        command.Parameters.AddWithValue("$description", transaction.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$id", transaction.Id);
        
        int currentUserId = SessionManager.Instance.CurrentUser?.Id ?? 0;
        command.Parameters.AddWithValue("$userId", currentUserId);

        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        var connection = DatabaseConnection.Instance.GetConnection();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Transactions WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        command.ExecuteNonQuery();
    }

    public void ReassignTransactionsToNoCategory(int categoryId, int? year = null, int? month = null, bool fromDateOnwards = false)
    {
        ReassignTransactions(categoryId, null, year, month, fromDateOnwards);
    }

    public void ReassignTransactions(int oldCategoryId, int? newCategoryId, int? year = null, int? month = null, bool fromDateOnwards = false)
    {
        var connection = DatabaseConnection.Instance.GetConnection();
        var command = connection.CreateCommand();
        
        string sql = "UPDATE Transactions SET CategoryId = $newCatId WHERE CategoryId = $oldCatId AND UserId = $userId";
        
        command.Parameters.AddWithValue("$oldCatId", oldCategoryId);
        command.Parameters.AddWithValue("$newCatId", newCategoryId.HasValue && newCategoryId.Value != 0 ? (object)newCategoryId.Value : DBNull.Value);
        
        int currentUserId = SessionManager.Instance.CurrentUser?.Id ?? 0;
        command.Parameters.AddWithValue("$userId", currentUserId);

        if (year.HasValue && month.HasValue)
        {
            if (fromDateOnwards)
            {
                string dateStr = $"{year.Value}-{month.Value:D2}-01";
                sql += " AND Date >= $date";
                command.Parameters.AddWithValue("$date", dateStr);
            }
            else
            {
                sql += " AND strftime('%Y', Date) = $yearStr AND strftime('%m', Date) = $monthStr";
                command.Parameters.AddWithValue("$yearStr", year.Value.ToString());
                command.Parameters.AddWithValue("$monthStr", month.Value.ToString("D2"));
            }
        }

        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}