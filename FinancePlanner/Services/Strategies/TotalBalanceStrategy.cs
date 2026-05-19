using System.Collections.Generic;

namespace FinancePlanner.Services.Strategies
{
    public class TotalBalanceStrategy : IBalanceCalculationStrategy
    {
        public decimal Calculate(List<Transaction> transactions)
        {
            var income = BalanceCalculationUtils.SumByType(transactions, "Income");
            var expense = BalanceCalculationUtils.SumByType(transactions, "Expense");
            return income - expense;
        }
    }
}
