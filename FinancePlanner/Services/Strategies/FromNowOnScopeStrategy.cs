using System.Linq;
using FinancePlanner.Repositories;

namespace FinancePlanner.Services.Strategies
{
    /// <summary>
    /// Стратегія застосування зміни з поточного місяця і надалі (FromNowOn).
    /// </summary>
    public class FromNowOnScopeStrategy : ICategoryChangeStrategy
    {
        public void ExecuteDelete(Category category, int currentYear, int currentMonth, int? reassignToId,
            CategoryRepository categoryRepo, TransactionRepository transactionRepo)
        {
            transactionRepo.ReassignTransactions(category.Id, reassignToId, currentYear, currentMonth, true);

            var (prevYear, prevMonth) = PreviousYearMonth(currentYear, currentMonth);
            category.EndMonth = prevMonth;
            category.EndYear = prevYear;
            categoryRepo.Update(category);
        }

        public void ExecuteRename(Category category, string newName, int currentYear, int currentMonth,
            CategoryRepository categoryRepo, TransactionRepository transactionRepo)
        {
            var newCat = new Category
            {
                Name = newName,
                Type = category.Type,
                ProjectedAmount = category.ProjectedAmount,
                StartMonth = currentMonth,
                StartYear = currentYear,
                EndMonth = category.EndMonth,
                EndYear = category.EndYear
            };
            categoryRepo.Add(newCat);

            var newCatCreated = categoryRepo.GetAll()
                .OrderByDescending(c => c.Id)
                .FirstOrDefault(c => c.Name == newName);
            int? newId = newCatCreated?.Id;

            ExecuteDelete(category, currentYear, currentMonth, newId, categoryRepo, transactionRepo);
        }

        private static (int year, int month) PreviousYearMonth(int year, int month)
        {
            if (month == 1) return (year - 1, 12);
            return (year, month - 1);
        }
    }
}