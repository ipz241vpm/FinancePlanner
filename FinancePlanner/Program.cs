using System;
using System.Windows.Forms;
using FinancePlanner.Repositories;
using FinancePlanner.Services;

namespace FinancePlanner
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Ініціалізуємо БД до запуску вікон
            DatabaseInitializer.Initialize();

            ICurrentUserProvider userProvider = new SessionCurrentUserProvider();

            var userRepo = new UserRepository();
            var categoryRepo = new CategoryRepository(userProvider);
            var transactionRepo = new TransactionRepository(userProvider);

            // Відкриваємо форму авторизації
            using (var authForm = new Forms.AuthForm(userRepo))
            {
                if (authForm.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new Form1(categoryRepo, transactionRepo, userProvider));
                }
            }
        }
    }
}