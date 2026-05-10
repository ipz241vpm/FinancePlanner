using System;
using System.Collections.Generic;
using System.Linq;
using FinancePlanner.Models;
using FinancePlanner.Repositories;
using FinancePlanner.Services.Strategies;

namespace FinancePlanner.Services
{
    public class CategoryProjection
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Projected { get; set; }
        public decimal Actual { get; set; }
    }

    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepo;
        private readonly TransactionRepository _transactionRepo;
        private readonly Dictionary<CategoryScope, ICategoryChangeStrategy> _strategies;

        public CategoryService(CategoryRepository categoryRepo, TransactionRepository transactionRepo)
        {
            _categoryRepo = categoryRepo;
            _transactionRepo = transactionRepo;

            _strategies = new Dictionary<CategoryScope, ICategoryChangeStrategy>
            {
                { CategoryScope.All, new AllScopeStrategy() },
                { CategoryScope.ThisMonth, new ThisMonthScopeStrategy() },
                { CategoryScope.FromNowOn, new FromNowOnScopeStrategy() }
            };
        }

        public bool IsCategoryActive(Category c, int year, int month)
        {
            if (c.StartYear > 0)
            {
                if (year < c.StartYear || (year == c.StartYear && month < c.StartMonth))
                    return false;
            }

            if (c.EndYear > 0)
            {
                if (year > c.EndYear || (year == c.EndYear && month > c.EndMonth))
                    return false;
            }

            return true;
        }

        public bool IsCategoryActiveInRange(Category c, DateTime start, DateTime end)
        {
            DateTime catStart = DateTime.MinValue;
            if (c.StartYear > 0)
            {
                catStart = new DateTime(c.StartYear, c.StartMonth, 1);
            }

            DateTime catEnd = DateTime.MaxValue;
            if (c.EndYear > 0)
            {
                catEnd = new DateTime(c.EndYear, c.EndMonth, 1).AddMonths(1).AddSeconds(-1);
            }

            return catStart <= end && catEnd >= start;
        }

        public List<CategoryProjection> GetProjectionsForMonth(int year, int month)
        {
            var categories = _categoryRepo.GetAll()
                .Where(c => IsCategoryActive(c, year, month))
                .ToList();

            var monthTransactions = _transactionRepo.GetAll()
                .Where(t => t.Date.Year == year && t.Date.Month == month)
                .ToList();

            return categories.Select(category => new CategoryProjection
            {
                CategoryId = category.Id,
                Name = category.Name,
                Type = category.Type,
                Projected = category.ProjectedAmount,
                Actual = monthTransactions
                    .Where(t => t.CategoryId == category.Id && t.Type == category.Type)
                    .Sum(t => t.Amount)
            }).ToList();
        }

        public void UpdateProjection(int categoryId, decimal newProjection)
        {
            var category = _categoryRepo.GetAll().FirstOrDefault(c => c.Id == categoryId);
            if (category != null)
            {
                category.ProjectedAmount = newProjection;
                _categoryRepo.Update(category);
            }
        }

        public void DeleteCategory(int categoryId, CategoryScope scope, int currentYear, int currentMonth, int? reassignToId = null)
        {
            var category = _categoryRepo.GetAll().FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return;

            if (_strategies.TryGetValue(scope, out var strategy))
            {
                strategy.ExecuteDelete(category, currentYear, currentMonth, reassignToId, _categoryRepo, _transactionRepo);
            }
        }

        public void RenameCategory(int categoryId, string newName, CategoryScope scope, int currentYear, int currentMonth)
        {
            var category = _categoryRepo.GetAll().FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return;

            if (_strategies.TryGetValue(scope, out var strategy))
            {
                strategy.ExecuteRename(category, newName, currentYear, currentMonth, _categoryRepo, _transactionRepo);
            }
        }
    }
}