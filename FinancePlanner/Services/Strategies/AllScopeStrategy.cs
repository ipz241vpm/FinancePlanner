using System.Linq;
using FinancePlanner.Repositories;
using FinancePlanner.Repositories.Interfaces;

namespace FinancePlanner.Services.Strategies
{
    /// <summary>
    /// Стратегія застосування зміни до всіх записів категорії (All).
    /// </summary>
    public class AllScopeStrategy : ICategoryChangeStrategy
    {
        public void ExecuteDelete(Category category, int currentYear, int currentMonth, int? reassignToId,
            ICategoryRepository categoryRepo, ITransactionRepository transactionRepo)
        {
            transactionRepo.ReassignTransactions(category.Id, reassignToId);
            categoryRepo.Delete(category.Id);
        }

        public void ExecuteRename(Category category, string newName, int currentYear, int currentMonth,
            ICategoryRepository categoryRepo, ITransactionRepository transactionRepo)
        {
            category.Name = newName;
            categoryRepo.Update(category);
        }
    }
}