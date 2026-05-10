using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FinancePlanner.Factories;
using FinancePlanner.Forms;
using FinancePlanner.Repositories;
using FinancePlanner.Repositories.Interfaces;
using FinancePlanner.Services;
using FinancePlanner.Services.Strategies;

namespace FinancePlanner
{
    public class MainMenuDesigner
    {
        private TransactionService _transactionService;
        private ITransactionRepository _transactionRepo;
        private ICategoryRepository _categoryRepo;
        private CategoryService _categoryService;
        private DataGridView _gridTransactions;
        private DataGridView _gridProjections;
        private DateTimePicker _datePicker;
        private System.Collections.Generic.List<Transaction> _currentTransactions;

        public void Setup(Form mainForm, ICategoryRepository categoryRepo, ITransactionRepository transactionRepo)
        {
            _transactionRepo = transactionRepo;
            _categoryRepo = categoryRepo;

            // Ініціалізація сервісів для тестування патернів
            var factory = new TransactionFactory();
            
            _transactionService = new TransactionService(_transactionRepo, factory);
            _categoryService = new CategoryService(_categoryRepo, _transactionRepo);

            // Налаштування головної форми
            mainForm.Text = $"Finance Planner - Головне меню ({SessionManager.Instance.CurrentUser?.Username})";
            mainForm.Size = new Size(950, 600);
            mainForm.MinimumSize = new Size(800, 500);
            mainForm.StartPosition = FormStartPosition.CenterScreen;
            mainForm.BackColor = Color.WhiteSmoke;

            // Ліва панель для меню (кнопки)
            Panel leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = 250;
            leftPanel.BackColor = Color.FromArgb(41, 53, 65); // Dark blue-ish grey

            // Заголовок в лівій панелі
            Label titleLabel = new Label();
            titleLabel.Text = "Finance Planner";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 80;
            leftPanel.Controls.Add(titleLabel);

            // Головна панель для контенту (справа)
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.White;
            mainPanel.Padding = new Padding(20);

            // Верхня частина головної панелі (заголовок та вибір дати)
            Panel topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 60;
            topPanel.BackColor = Color.White;

            Label historyLabel = new Label();
            historyLabel.Text = "Історія транзакцій";
            historyLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            historyLabel.AutoSize = true;
            historyLabel.Location = new Point(0, 15);

            _datePicker = new DateTimePicker();
            _datePicker.Location = new Point(300, 15);
            _datePicker.Width = 200;
            _datePicker.Font = new Font("Segoe UI", 11);
            _datePicker.Format = DateTimePickerFormat.Short;
            _datePicker.ValueChanged += (s, e) => LoadTransactions();

            topPanel.Controls.Add(historyLabel);
            topPanel.Controls.Add(_datePicker);

            // Таблиця для транзакцій
            _gridTransactions = new DataGridView();
            _gridTransactions.Dock = DockStyle.Fill;
            _gridTransactions.AllowUserToAddRows = false;
            _gridTransactions.AllowUserToDeleteRows = false;
            _gridTransactions.ReadOnly = true;
            _gridTransactions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridTransactions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _gridTransactions.BackgroundColor = Color.White;
            _gridTransactions.BorderStyle = BorderStyle.None;
            _gridTransactions.RowHeadersVisible = false;
            _gridTransactions.AllowUserToResizeRows = false;
            _gridTransactions.CellDoubleClick += GridTransactions_CellDoubleClick;
            
            // Стилізація таблиці
            _gridTransactions.EnableHeadersVisualStyles = false;
            _gridTransactions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            _gridTransactions.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _gridTransactions.ColumnHeadersHeight = 40;
            _gridTransactions.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            _gridTransactions.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 240);
            _gridTransactions.DefaultCellStyle.SelectionForeColor = Color.Black;
            _gridTransactions.RowTemplate.Height = 35;
            _gridTransactions.GridColor = Color.FromArgb(230, 230, 230);

            mainPanel.Controls.Add(_gridTransactions);
            mainPanel.Controls.Add(topPanel);
            _gridTransactions.BringToFront(); // Щоб таблиця зайняла залишок місця

            // Додаємо кнопки в ліву панель (додаються знизу вверх через DockStyle.Top)
            Button btnExit = CreateMenuButton("Вихід", (s, e) => Application.Exit());
            btnExit.Dock = DockStyle.Bottom;
            btnExit.BackColor = Color.FromArgb(180, 80, 80);
            btnExit.MouseEnter += (s, e) => btnExit.BackColor = Color.FromArgb(200, 90, 90);
            btnExit.MouseLeave += (s, e) => btnExit.BackColor = Color.FromArgb(180, 80, 80);
            leftPanel.Controls.Add(btnExit);

            leftPanel.Controls.Add(CreateMenuButton("Всього витрат", BtnExpensesOnly_Click));
            leftPanel.Controls.Add(CreateMenuButton("Баланс за цей місяць", BtnMonthlyBalance_Click));
            leftPanel.Controls.Add(CreateMenuButton("Загальний баланс", BtnTotalBalance_Click));
            leftPanel.Controls.Add(CreateMenuButton("Аналітика", BtnAnalytics_Click));
            leftPanel.Controls.Add(CreateMenuButton("Додати витрату", BtnExpense_Click));
            leftPanel.Controls.Add(CreateMenuButton("Додати дохід", BtnIncome_Click));

            // Панель для прогнозу
            Panel rightPanel = new Panel { Dock = DockStyle.Right, Width = 350, BackColor = Color.WhiteSmoke, Padding = new Padding(10) };
            Label lblProjections = new Label { Text = "Прогноз / Реально за місяць", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter };
            
            _gridProjections = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            
            _gridProjections.CellValueChanged += GridProjections_CellValueChanged;
            _gridProjections.CellValidating += GridProjections_CellValidating;
            
            _gridProjections.Columns.Add(new DataGridViewTextBoxColumn { Name = "CategoryName", HeaderText = "Категорія", ReadOnly = true });
            _gridProjections.Columns.Add(new DataGridViewTextBoxColumn { Name = "Projected", HeaderText = "Прогноз" });
            _gridProjections.Columns.Add(new DataGridViewTextBoxColumn { Name = "Actual", HeaderText = "Реально", ReadOnly = true });
            _gridProjections.Columns.Add(new DataGridViewTextBoxColumn { Name = "CategoryId", Visible = false });
            _gridProjections.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", Visible = false });
            _gridProjections.CellMouseClick += GridProjections_CellMouseClick;

            // Кнопки для додавання категорій прямо на головній
            Panel rightBottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.WhiteSmoke };
            LinkLabel lnkAddIncomeCategory = new LinkLabel { Text = "[+] Додати дохід", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), LinkColor = Color.DarkGreen };
            LinkLabel lnkAddExpenseCategory = new LinkLabel { Text = "[+] Додати витрату", Location = new Point(180, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), LinkColor = Color.DarkRed };
            
            lnkAddIncomeCategory.LinkClicked += (s, e) => AddCategoryFromMain("Income");
            lnkAddExpenseCategory.LinkClicked += (s, e) => AddCategoryFromMain("Expense");
            
            rightBottomPanel.Controls.Add(lnkAddIncomeCategory);
            rightBottomPanel.Controls.Add(lnkAddExpenseCategory);

            rightPanel.Controls.Add(_gridProjections);
            rightPanel.Controls.Add(rightBottomPanel);
            rightPanel.Controls.Add(lblProjections);

            // Збираємо все на форму
            mainForm.Controls.Add(rightPanel);
            mainForm.Controls.Add(mainPanel);
            mainForm.Controls.Add(leftPanel);
            mainPanel.BringToFront();

            // Завантажуємо дані при показі форми
            mainForm.Shown += (s, e) => LoadTransactions();
        }

        private void GridTransactions_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < _currentTransactions.Count)
            {
                var transaction = _currentTransactions[e.RowIndex];
                using (var editForm = new AddTransactionForm(transaction.Type, _categoryRepo, transaction))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        if (editForm.IsDeleted)
                        {
                            _transactionService.DeleteTransaction(transaction.Id);
                            MessageBox.Show("Транзакцію видалено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            transaction.Amount = editForm.Amount;
                            transaction.Description = editForm.Description;
                            transaction.CategoryId = editForm.CategoryId;
                            transaction.Date = editForm.Date;

                            _transactionService.UpdateTransaction(transaction);
                            MessageBox.Show("Транзакцію оновлено успішно!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        LoadTransactions();
                    }
                }
            }
        }

        private Button CreateMenuButton(string text, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Dock = DockStyle.Top;
            btn.Height = 55;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.LightGray;
            btn.Font = new Font("Segoe UI", 11);
            btn.Cursor = Cursors.Hand;
            btn.Click += onClick;
            
            // Hover ефект
            btn.MouseEnter += (s, e) => 
            { 
                if (btn.Text != "Вихід") // "Вихід" має свій колір
                {
                    btn.BackColor = Color.FromArgb(60, 75, 90); 
                    btn.ForeColor = Color.White; 
                }
            };
            btn.MouseLeave += (s, e) => 
            { 
                if (btn.Text != "Вихід")
                {
                    btn.BackColor = Color.Transparent; 
                    btn.ForeColor = Color.LightGray; 
                }
            };
            return btn;
        }

        private void LoadTransactions()
        {
            var targetDate = _datePicker.Value.Date;
            var allTransactions = _transactionRepo.GetAll();
            var allCategories = _categoryRepo.GetAll();

            _currentTransactions = allTransactions.Where(t => t.Date.Date == targetDate)
                                      .OrderByDescending(t => t.Date)
                                      .ToList();
            
            var displayList = _currentTransactions.Select(t => {
                var category = allCategories.FirstOrDefault(c => c.Id == t.CategoryId);
                return new {
                    Тип = t.Type,
                    Категорія = category?.Name ?? "Без категорії",
                    Сума = t.Amount.ToString("C"),
                    Опис = t.Description,
                    Час = t.Date.ToString("HH:mm")
                };
            }).ToList();

            _gridTransactions.DataSource = displayList;
            LoadProjections();
        }

        private void LoadProjections()
        {
            if (_categoryService == null || _gridProjections == null) return;

            var year = _datePicker.Value.Year;
            var month = _datePicker.Value.Month;

            var projections = _categoryService.GetProjectionsForMonth(year, month);
            
            var sortedProjections = projections
                .OrderBy(p => p.Type == "Income" ? 0 : 1)
                .ThenBy(p => p.Name)
                .ToList();

            // Відключаємо обробник під час завантаження, щоб не викликати оновлення бази
            _gridProjections.CellValueChanged -= GridProjections_CellValueChanged;
            _gridProjections.Rows.Clear();

            foreach (var proj in sortedProjections)
            {
                int rowIndex = _gridProjections.Rows.Add(proj.Name, proj.Projected, proj.Actual, proj.CategoryId, proj.Type);
                DataGridViewRow row = _gridProjections.Rows[rowIndex];

                if (proj.Type == "Income")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230);
                    row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
            _gridProjections.CellValueChanged += GridProjections_CellValueChanged;
        }

        private void GridProjections_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_gridProjections.Columns[e.ColumnIndex].Name == "Projected")
            {
                if (!decimal.TryParse(e.FormattedValue.ToString(), out decimal res) || res < 0)
                {
                    e.Cancel = true;
                    MessageBox.Show("Введіть коректну суму прогнозу (більше або рівну 0).", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void GridProjections_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _gridProjections.Columns[e.ColumnIndex].Name == "Projected")
            {
                var idVal = _gridProjections.Rows[e.RowIndex].Cells["CategoryId"].Value;
                var projVal = _gridProjections.Rows[e.RowIndex].Cells["Projected"].Value;
                if (idVal != null && projVal != null)
                {
                    int categoryId = (int)idVal;
                    decimal newValue = Convert.ToDecimal(projVal);
                    _categoryService.UpdateProjection(categoryId, newValue);
                }
            }
        }

        private void GridProjections_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                _gridProjections.ClearSelection();
                _gridProjections.Rows[e.RowIndex].Selected = true;

                int categoryId = (int)_gridProjections.Rows[e.RowIndex].Cells["CategoryId"].Value;
                string catName = _gridProjections.Rows[e.RowIndex].Cells["CategoryName"].Value.ToString();
                int currentYear = _datePicker.Value.Year;
                int currentMonth = _datePicker.Value.Month;

                var menu = new ContextMenuStrip();
                var renameItem = new ToolStripMenuItem("Перейменувати");
                renameItem.Click += (s, ev) => 
                {
                    using (var form = new CategoryScopeForm(catName, true))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            _categoryService.RenameCategory(categoryId, form.CategoryName, form.Scope, currentYear, currentMonth);
                            LoadProjections();
                        }
                    }
                };
                
                var deleteItem = new ToolStripMenuItem("Видалити");
                deleteItem.Click += (s, ev) => 
                {
                    using (var form = new CategoryScopeForm(catName, false))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            _categoryService.DeleteCategory(categoryId, form.Scope, currentYear, currentMonth);
                            LoadProjections();
                        }
                    }
                };

                menu.Items.Add(renameItem);
                menu.Items.Add(deleteItem);
                menu.Show(Cursor.Position);
            }
        }

        private void AddCategoryFromMain(string internalTransactionType)
        {
            string typeDisplay = internalTransactionType == "Income" ? "дохід" : "витрату";
            using (var form = new AddCategoryForm(internalTransactionType, _datePicker.Value, _categoryRepo))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadProjections();
                }
            }
        }

        private void BtnAnalytics_Click(object sender, EventArgs e)
        {
            using (var analyticsForm = new AnalyticsForm(_transactionRepo, _categoryRepo, _categoryService))
            {
                analyticsForm.ShowDialog();
            }
        }

        private void BtnIncome_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddTransactionForm("дохід", _categoryRepo))
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    _transactionService.AddIncomeForDay(addForm.Amount, addForm.Date, addForm.Description, addForm.CategoryId);
                    MessageBox.Show("Дохід додано успішно!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                    LoadTransactions();
                }
            }
        }

        private void BtnExpense_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddTransactionForm("витрату", _categoryRepo))
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    _transactionService.AddExpenseForDay(addForm.Amount, addForm.Date, addForm.Description, addForm.CategoryId);
                    MessageBox.Show("Витрату додано успішно!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                    LoadTransactions();
                }
            }
        }

        private void BtnTotalBalance_Click(object sender, EventArgs e)
        {
            var balance = _transactionService.CalculateBalance(new TotalBalanceStrategy());
            MessageBox.Show($"Загальний баланс (всі транзакції): {balance:C}", "Баланс", MessageBoxButtons.OK, MessageBoxIcon.Information); 
        }

        private void BtnMonthlyBalance_Click(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var balance = _transactionService.CalculateBalance(new MonthlyBalanceStrategy(now.Year, now.Month));
            MessageBox.Show($"Баланс за поточний місяць ({now:MMMM}): {balance:C}", "Баланс", MessageBoxButtons.OK, MessageBoxIcon.Information); 
        }

        private void BtnExpensesOnly_Click(object sender, EventArgs e)
        {
            var balance = _transactionService.CalculateBalance(new ExpensesOnlyStrategy());
            MessageBox.Show($"Сума всіх витрат: {balance:C}", "Витрати", MessageBoxButtons.OK, MessageBoxIcon.Information); 
        }
    }
}
