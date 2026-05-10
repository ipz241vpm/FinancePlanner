using System.Linq;
using FinancePlanner.Repositories;

namespace FinancePlanner.Services.Strategies
{
    /// <summary>
    /// Стратегія застосування зміни до всіх записів категорії (All).
    /// </summary>
    public class AllScopeStrategy : ICategoryChangeStrategy
    {
        public void ExecuteDelete(Category category, int currentYear, int currentMonth, int? reassignToId,
            CategoryRepository categoryRepo, TransactionRepository transactionRepo)
        {
            transactionRepo.ReassignTransactions(category.Id, reassignToId);
            categoryRepo.Delete(category.Id);
        }

        public void ExecuteRename(Category category, string newName, int currentYear, int currentMonth,
            CategoryRepository categoryRepo, TransactionRepository transactionRepo)
        {
            category.Name = newName;
            categoryRepo.Update(category);
        }
    }
}