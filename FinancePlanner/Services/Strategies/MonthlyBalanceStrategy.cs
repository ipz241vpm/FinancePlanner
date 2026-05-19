using System;

namespace FinancePlanner.Services.Strategies
{
    public class MonthlyBalanceStrategy : IBalanceCalculationStrategy
    {
        private readonly int _month;
        private readonly int _year;

        public MonthlyBalanceStrategy(int year, int month)
        {
            if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
            _year = year;
            _month = month;
        }

        public decimal Calculate(List<Transaction> transactions)
        {
            return BalanceCalculationUtils.BalanceForMonth(transactions, _year, _month);
        }
    }
}
