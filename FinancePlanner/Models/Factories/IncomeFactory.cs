using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancePlanner.Models.Factories
{
    public class IncomeFactory : ITransactionFactory
    {
        public Transaction CreateTransaction(decimal amount, DateTime date, string description, int categoryId)
        {
            return new Transaction
            {
                Amount = amount,
                Date = date,
                Description = description,
                CategoryId = categoryId,
                Type = "Income"
            };
        }
    }
}
