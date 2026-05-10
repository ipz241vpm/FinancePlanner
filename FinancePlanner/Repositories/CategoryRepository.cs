using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using FinancePlanner.Services;
using FinancePlanner.Repositories.Interfaces;

namespace FinancePlanner.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        public void Add(Category category)
        {
            var connection = DatabaseConnection.Instance.GetConnection();
            var command = connection.CreateCommand();
            
            command.CommandText = "INSERT INTO Categories (Name, Type, UserId, ProjectedAmount, StartMonth, StartYear, EndMonth, EndYear) VALUES ($name, $type, $userId, $projectedAmount, $startMonth, $startYear, $endMonth, $endYear)";
            command.Parameters.AddWithValue("$name", category.Name);
            command.Parameters.AddWithValue("$type", category.Type);
            
            int currentUserId = SessionManager.Instance.CurrentUser?.Id ?? 0;
            command.Parameters.AddWithValue("$userId", currentUserId);
            command.Parameters.AddWithValue("$projectedAmount", category.ProjectedAmount);
            command.Parameters.AddWithValue("$startMonth", category.StartMonth);
            command.Parameters.AddWithValue("$startYear", category.StartYear);
            command.Parameters.AddWithValue("$endMonth", category.EndMonth);
            command.Parameters.AddWithValue("$endYear", category.EndYear);
            
            command.ExecuteNonQuery();
        }

        public List<Category> GetByType(string type)
        {
            var list = new List<Category>();
            var connection = DatabaseConnection.Instance.GetConnection();
            var command = connection.CreateCommand();
            
            int currentUserId = SessionManager.Instance.CurrentUser?.Id ?? 0;
            command.CommandText = "SELECT Id, Name, Type, UserId, ProjectedAmount, StartMonth, StartYear, EndMonth, EndYear FROM Categories WHERE Type = $type AND UserId = $userId ORDER BY Name";
            command.Parameters.AddWithValue("$type", type);
            command.Parameters.AddWithValue("$userId", currentUserId);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    UserId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    ProjectedAmount = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                    StartMonth = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    StartYear = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    EndMonth = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                    EndYear = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                });
            }
            
            return list;
        }
        public List<Category> GetAll()
        {
            var list = new List<Category>();
            var connection = DatabaseConnection.Instance.GetConnection();
            var command = connection.CreateCommand();
            
            int currentUserId = SessionManager.Instance.CurrentUser?.Id ?? 0;
            command.CommandText = "SELECT Id, Name, Type, UserId, ProjectedAmount, StartMonth, StartYear, EndMonth, EndYear FROM Categories WHERE UserId = $userId ORDER BY Name";
            command.Parameters.AddWithValue("$userId", currentUserId);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    UserId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    ProjectedAmount = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                    StartMonth = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    StartYear = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    EndMonth = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                    EndYear = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                });
            }
            
            return list;
        }

        public void Update(Category category)
        {
            var connection = DatabaseConnection.Instance.GetConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE Categories 
                SET Name = $name, 
                    Type = $type, 
                    ProjectedAmount = $projectedAmount,
                    StartMonth = $startMonth,
                    StartYear = $startYear,
                    EndMonth = $endMonth,
                    EndYear = $endYear
                WHERE Id = $id AND UserId = $userId;";

            command.Parameters.AddWithValue("$name", category.Name);
            command.Parameters.AddWithValue("$type", category.Type);
            command.Parameters.AddWithValue("$projectedAmount", category.ProjectedAmount);
            command.Parameters.AddWithValue("$startMonth", category.StartMonth);
            command.Parameters.AddWithValue("$startYear", category.StartYear);
            command.Parameters.AddWithValue("$endMonth", category.EndMonth);
            command.Parameters.AddWithValue("$endYear", category.EndYear);
            command.Parameters.AddWithValue("$id", category.Id);

            int currentUserId = SessionManager.Instance.CurrentUser?.Id ?? 0;
            command.Parameters.AddWithValue("$userId", currentUserId);

            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            var connection = DatabaseConnection.Instance.GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Categories WHERE Id = $id AND UserId = $userId";
            command.Parameters.AddWithValue("$id", id);
            
            int currentUserId = SessionManager.Instance.CurrentUser?.Id ?? 0;
            command.Parameters.AddWithValue("$userId", currentUserId);
            
            command.ExecuteNonQuery();
        }
    }
}
