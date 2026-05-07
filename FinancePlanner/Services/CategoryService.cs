using FinancePlanner.Models;
using FinancePlanner.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

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

        public CategoryService(CategoryRepository categoryRepo, TransactionRepository transactionRepo)
        {
            _categoryRepo = categoryRepo;
            _transactionRepo = transactionRepo;
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
            // Перевірка активності категорії у діапазоні дат [start, end]
            
            // Дата початку категорії
            DateTime catStart = DateTime.MinValue;
            if (c.StartYear > 0)
            {
                catStart = new DateTime(c.StartYear, c.StartMonth, 1);
            }
                
            // Дата завершення категорії
            DateTime catEnd = DateTime.MaxValue;
            if (c.EndYear > 0)
            {
                // Остання секунда останнього дня місяця
                catEnd = new DateTime(c.EndYear, c.EndMonth, 1).AddMonths(1).AddSeconds(-1);
            }
                
            // Перетин діапазонів: [catStart, catEnd] та [start, end]
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

        public void DeleteCategory(int categoryId, CategoryScope scope, int year, int month, int? reassignToId = null)
        {
            var category = _categoryRepo.GetAll().FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return;

            switch (scope)
            {
                case CategoryScope.All:
                    _transactionRepo.ReassignTransactions(categoryId, reassignToId);
                    _categoryRepo.Delete(categoryId);
                    break;

                case CategoryScope.ThisMonth:
                    _transactionRepo.ReassignTransactions(categoryId, reassignToId, year, month);

                    var (nextY, nextM) = NextYearMonth(year, month);
                    if (IsDateBeforeOrEqual(nextY, nextM, category.EndYear, category.EndMonth) || category.EndYear == 0)
                    {
                        CreateContinuation(category, nextY, nextM);
                    }
                    CloseCategoryAt(category, year, month);
                    break;

                case CategoryScope.FromNowOn:
                    _transactionRepo.ReassignTransactions(categoryId, reassignToId, year, month, true);
                    CloseCategoryAt(category, year, month);
                    break;
            }
        }

        private static (int year, int month) PreviousYearMonth(int year, int month)
        {
            if (month == 1) return (year - 1, 12);
            return (year, month - 1);
        }

        private static (int year, int month) NextYearMonth(int year, int month)
        {
            if (month == 12) return (year + 1, 1);
            return (year, month + 1);
        }

        public void RenameCategory(int categoryId, string newName, CategoryScope scope, int year, int month)
        {
            var category = _categoryRepo.GetAll().FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return;

            switch (scope)
            {
                case CategoryScope.All:
                    category.Name = newName;
                    _categoryRepo.Update(category);
                    break;

                case CategoryScope.ThisMonth:
                    int? newId = CreateCategoryCopy(category, newName, year, month, year, month);
                    if (newId.HasValue)
                        DeleteCategory(categoryId, CategoryScope.ThisMonth, year, month, newId.Value);
                    break;

                case CategoryScope.FromNowOn:
                    int? futureId = CreateCategoryCopy(category, newName, year, month);
                    if (futureId.HasValue)
                        DeleteCategory(categoryId, CategoryScope.FromNowOn, year, month, futureId.Value);
                    break;
            }
        }
        private void CloseCategoryAt(Category category, int year, int month)
        {
            var (prevYear, prevMonth) = PreviousYearMonth(year, month);
            category.EndYear = prevYear;
            category.EndMonth = prevMonth;
            _categoryRepo.Update(category);
        }

        private void CreateContinuation(Category original, int startYear, int startMonth)
        {
            var continuation = new Category
            {
                Name = original.Name,
                Type = original.Type,
                ProjectedAmount = original.ProjectedAmount,
                StartYear = startYear,
                StartMonth = startMonth,
                EndYear = original.EndYear,
                EndMonth = original.EndMonth
            };
            _categoryRepo.Add(continuation);
        }
        private bool IsDateBeforeOrEqual(int y1, int m1, int y2, int m2)
        {
            if (y2 == 0) return true;
            return y1 < y2 || (y1 == y2 && m1 <= m2);
        }
        private int? CreateCategoryCopy(Category original, string newName, int startYear, int startMonth, int? endYear = null, int? endMonth = null)
        {
            var newCat = new Category
            {
                Name = newName,
                Type = original.Type,
                ProjectedAmount = original.ProjectedAmount,
                StartYear = startYear,
                StartMonth = startMonth,
                EndYear = endYear ?? original.EndYear,
                EndMonth = endMonth ?? original.EndMonth
            };

            _categoryRepo.Add(newCat);

            var created = _categoryRepo.GetAll()
                .OrderByDescending(c => c.Id)
                .FirstOrDefault(c => c.Name == newName);

            return created?.Id;
        }
    }
}
