using System;
using System.Collections.Generic;
using System.Linq;

namespace FinancePlanner.Services.Strategies
{
    /// <summary>
    /// Small internal helper to consolidate balance calculation logic.
    /// Reduces duplicated LINQ and centralizes null-safety and case-insensitive type comparison.
    /// </summary>
    internal static class BalanceCalculationUtils
    {
        public static decimal SumByType(IEnumerable<Transaction> transactions, string type)
        {
            if (transactions == null) return 0m;
            return transactions
                .Where(t => string.Equals(t.Type, type, StringComparison.OrdinalIgnoreCase))
                .Sum(t => t.Amount);
        }

        public static IEnumerable<Transaction> FilterByMonth(IEnumerable<Transaction> transactions, int year, int month)
        {
            if (transactions == null) return Enumerable.Empty<Transaction>();
            return transactions.Where(t => t.Date.Year == year && t.Date.Month == month);
        }

        public static decimal BalanceForMonth(IEnumerable<Transaction> transactions, int year, int month)
        {
            var monthly = FilterByMonth(transactions, year, month);
            var income = SumByType(monthly, "Income");
            var expense = SumByType(monthly, "Expense");
            return income - expense;
        }
    }
}