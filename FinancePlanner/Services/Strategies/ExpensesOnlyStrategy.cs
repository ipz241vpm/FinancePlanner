using System.Collections.Generic;

namespace FinancePlanner.Services.Strategies
{
    public class ExpensesOnlyStrategy : IBalanceCalculationStrategy
    {
        public decimal Calculate(List<Transaction> transactions)
        {
            return BalanceCalculationUtils.SumByType(transactions, "Expense");
        }
    }
}
