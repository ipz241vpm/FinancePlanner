namespace FinancePlanner.Repositories.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        void ReassignTransactionsToNoCategory(int categoryId, int? year = null, int? month = null, bool fromDateOnwards = false);
        void ReassignTransactions(int oldCategoryId, int? newCategoryId, int? year = null, int? month = null, bool fromDateOnwards = false);
    }
}
