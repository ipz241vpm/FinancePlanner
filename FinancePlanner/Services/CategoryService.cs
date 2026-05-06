using System;
using System.Collections.Generic;
using System.Linq;
using FinancePlanner.Models;
using FinancePlanner.Repositories;
using FinancePlanner.Repositories.Interfaces;

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
        private readonly ICategoryRepository _categoryRepo;
        private readonly ITransactionRepository _transactionRepo;

        public CategoryService(ICategoryRepository categoryRepo, ITransactionRepository transactionRepo)
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

        public void DeleteCategory(int categoryId, CategoryScope scope, int currentYear, int currentMonth, int? reassignToId = null)
        {
            var category = _categoryRepo.GetAll().FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return;

            if (scope == CategoryScope.All)
            {
                _transactionRepo.ReassignTransactions(categoryId, reassignToId);
                _categoryRepo.Delete(categoryId);
            }
            else if (scope == CategoryScope.ThisMonth)
            {
                _transactionRepo.ReassignTransactions(categoryId, reassignToId, currentYear, currentMonth);

                var (prevYear, prevMonth) = PreviousYearMonth(currentYear, currentMonth);
                var (nextYear, nextMonth) = NextYearMonth(currentYear, currentMonth);

                // Створюємо продовження категорії, якщо вона не закінчується раніше
                if (category.EndYear == 0 || category.EndYear > nextYear || (category.EndYear == nextYear && category.EndMonth >= nextMonth))
                {
                    var catContinuation = new Category
                    {
                        Name = category.Name,
                        Type = category.Type,
                        ProjectedAmount = category.ProjectedAmount,
                        StartMonth = nextMonth,
                        StartYear = nextYear,
                        EndMonth = category.EndMonth,
                        EndYear = category.EndYear
                    };
                    _categoryRepo.Add(catContinuation);
                }

                category.EndMonth = prevMonth;
                category.EndYear = prevYear;
                _categoryRepo.Update(category);
            }
            else if (scope == CategoryScope.FromNowOn)
            {
                _transactionRepo.ReassignTransactions(categoryId, reassignToId, currentYear, currentMonth, true);

                var (prevYear, prevMonth) = PreviousYearMonth(currentYear, currentMonth);
                category.EndMonth = prevMonth;
                category.EndYear = prevYear;
                _categoryRepo.Update(category);
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

        public void RenameCategory(int categoryId, string newName, CategoryScope scope, int currentYear, int currentMonth)
        {
            var category = _categoryRepo.GetAll().FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return;

            if (scope == CategoryScope.All)
            {
                category.Name = newName;
                _categoryRepo.Update(category);
            }
            else if (scope == CategoryScope.ThisMonth)
            {
                // Створюємо нову категорію на 1 місяць
                var tempCat = new Category { Name = newName, Type = category.Type, ProjectedAmount = category.ProjectedAmount, StartMonth = currentMonth, StartYear = currentYear, EndMonth = currentMonth, EndYear = currentYear };
                _categoryRepo.Add(tempCat);
                
                var newCat = _categoryRepo.GetAll().OrderByDescending(c => c.Id).FirstOrDefault(c => c.Name == newName);
                int? newId = newCat?.Id;

                // Виключаємо цей місяць з оригінальної категорії та переносимо транзакції
                DeleteCategory(categoryId, CategoryScope.ThisMonth, currentYear, currentMonth, newId);
            }
            else if (scope == CategoryScope.FromNowOn)
            {
                // Створюємо нову категорію
                var newCat = new Category { Name = newName, Type = category.Type, ProjectedAmount = category.ProjectedAmount, StartMonth = currentMonth, StartYear = currentYear, EndMonth = category.EndMonth, EndYear = category.EndYear };
                _categoryRepo.Add(newCat);

                var newCatCreated = _categoryRepo.GetAll().OrderByDescending(c => c.Id).FirstOrDefault(c => c.Name == newName);
                int? newId = newCatCreated?.Id;

                // Завершуємо стару та переносимо транзакції
                DeleteCategory(categoryId, CategoryScope.FromNowOn, currentYear, currentMonth, newId);
            }
        }
    }
}
