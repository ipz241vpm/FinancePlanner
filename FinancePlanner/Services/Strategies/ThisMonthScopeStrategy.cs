using System.Linq;
using FinancePlanner.Repositories;

namespace FinancePlanner.Services.Strategies
{
    /// <summary>
    /// Стратегія застосування зміни тільки для поточного місяця (ThisMonth).
    /// </summary>
    public class ThisMonthScopeStrategy : ICategoryChangeStrategy
    {
        public void ExecuteDelete(Category category, int currentYear, int currentMonth, int? reassignToId,
            CategoryRepository categoryRepo, TransactionRepository transactionRepo)
        {
            transactionRepo.ReassignTransactions(category.Id, reassignToId, currentYear, currentMonth);

            var (prevYear, prevMonth) = PreviousYearMonth(currentYear, currentMonth);
            var (nextYear, nextMonth) = NextYearMonth(currentYear, currentMonth);

            if (category.EndYear == 0 || category.EndYear > nextYear ||
                (category.EndYear == nextYear && category.EndMonth >= nextMonth))
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
                categoryRepo.Add(catContinuation);
            }

            category.EndMonth = prevMonth;
            category.EndYear = prevYear;
            categoryRepo.Update(category);
        }

        public void ExecuteRename(Category category, string newName, int currentYear, int currentMonth,
            CategoryRepository categoryRepo, TransactionRepository transactionRepo)
        {
            var tempCat = new Category
            {
                Name = newName,
                Type = category.Type,
                ProjectedAmount = category.ProjectedAmount,
                StartMonth = currentMonth,
                StartYear = currentYear,
                EndMonth = currentMonth,
                EndYear = currentYear
            };
            categoryRepo.Add(tempCat);

            var newCat = categoryRepo.GetAll()
                .OrderByDescending(c => c.Id)
                .FirstOrDefault(c => c.Name == newName);
            int? newId = newCat?.Id;

            ExecuteDelete(category, currentYear, currentMonth, newId, categoryRepo, transactionRepo);
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
    }
}