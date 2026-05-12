using System;
using System.Collections.Generic;
using FinancePlanner.Models.Factories;
using FinancePlanner.Repositories.Interfaces;
using FinancePlanner.Services.Strategies;

namespace FinancePlanner.Services
{
    public class TransactionService
    {
        private readonly ITransactionRepository _repository;

        public TransactionService(ITransactionRepository repository)
        {
            _repository = repository;
        }

        // Додавання доходу за конкретний день
        public void AddIncomeForDay(decimal amount, DateTime specificDate, string description, int categoryId = 0)
        {
            var factory = new IncomeFactory();
            var income = factory.CreateTransaction(amount, specificDate, description, categoryId);
            _repository.Add(income);
        }

        // Додавання витрати за конкретний день
        public void AddExpenseForDay(decimal amount, DateTime specificDate, string description, int categoryId = 0)
        {
            var factory = new ExpenseFactory();
            var expense = factory.CreateTransaction(amount, specificDate, description, categoryId);
            _repository.Add(expense);
        }

        // Оновлення транзакції
        public void UpdateTransaction(Transaction transaction)
        {
            _repository.Update(transaction);
        }

        // Видалення транзакції
        public void DeleteTransaction(int id)
        {
            _repository.Delete(id);
        }

        // Підрахунок балансу з використанням вибраної стратегії
        public decimal CalculateBalance(IBalanceCalculationStrategy strategy)
        {
            List<Transaction> allTransactions = _repository.GetAll();
            return strategy.Calculate(allTransactions);
        }
    }
}
