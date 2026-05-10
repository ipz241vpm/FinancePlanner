using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancePlanner.Models.Factories
{
    public interface ITransactionFactory
    {
        Transaction CreateTransaction(decimal amount, DateTime date, string description, int categoryId);
    }
}
