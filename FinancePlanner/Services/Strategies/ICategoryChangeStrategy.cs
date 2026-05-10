using FinancePlanner.Repositories;

namespace FinancePlanner.Services.Strategies
{
    /// <summary>
    /// Стратегія застосування зміни до категорії залежно від області (Scope).
    /// </summary>
    public interface ICategoryChangeStrategy
    {
        /// <summary>
        /// Видаляє категорію відповідно до області застосування.
        /// </summary>
        /// <param name="category">Категорія для видалення.</param>
        /// <param name="currentYear">Поточний рік.</param>
        /// <param name="currentMonth">Поточний місяць.</param>
        /// <param name="reassignToId">ID категорії для перепризначення транзакцій (опціонально).</param>
        /// <param name="categoryRepo">Репозиторій категорій.</param>
        /// <param name="transactionRepo">Репозиторій транзакцій.</param>
        void ExecuteDelete(Category category, int currentYear, int currentMonth, int? reassignToId,
            CategoryRepository categoryRepo, TransactionRepository transactionRepo);

        /// <summary>
        /// Перейменовує категорію відповідно до області застосування.
        /// </summary>
        /// <param name="category">Категорія для перейменування.</param>
        /// <param name="newName">Нова назва.</param>
        /// <param name="currentYear">Поточний рік.</param>
        /// <param name="currentMonth">Поточний місяць.</param>
        /// <param name="categoryRepo">Репозиторій категорій.</param>
        /// <param name="transactionRepo">Репозиторій транзакцій.</param>
        void ExecuteRename(Category category, string newName, int currentYear, int currentMonth,
            CategoryRepository categoryRepo, TransactionRepository transactionRepo);
    }
}