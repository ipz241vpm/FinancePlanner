using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FinancePlanner.Repositories;
using FinancePlanner.Repositories.Interfaces;
using FinancePlanner.Services;

namespace FinancePlanner.Forms
{
    public class FilterOption
    {
        public string Name { get; set; }
        public string FilterType { get; set; }
        public List<int> CategoryIds { get; set; } = new List<int>();
    }

    public class AnalyticsForm : Form
    {
        private ITransactionRepository _transactionRepo;
        private ICategoryRepository _categoryRepo;
        private CategoryService _categoryService;
        private Dictionary<int, string> _categoriesDict = new();
        
        private Label lblTotalIncome;
        private Label lblTotalExpenses;
        private Label lblBalance;
        private DataGridView _gridAnalytics;
        private DateTimePicker _dtpStart;
        private DateTimePicker _dtpEnd;
        private ComboBox _cmbType;

        public AnalyticsForm(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            CategoryService categoryService
            )
        {
            _transactionRepo = transactionRepository;
            _categoryRepo = categoryRepository;
            _categoryService = categoryService;

            Text = "Фінансова Аналітика";
            Size = new Size(850, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.WhiteSmoke;

            // Header Panel
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White };
            Label title = new Label { Text = "Аналітика за період", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            
            Label lblFrom = new Label { Text = "З:", Location = new Point(320, 32), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _dtpStart = new DateTimePicker { Location = new Point(350, 28), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1) };
            
            Label lblTo = new Label { Text = "По:", Location = new Point(515, 32), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _dtpEnd = new DateTimePicker { Location = new Point(550, 28), Width = 150, Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            
            Label lblType = new Label { Text = "Фільтр:", Location = new Point(715, 32), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _cmbType = new ComboBox { Location = new Point(770, 28), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbType.DisplayMember = "Name";

            _dtpStart.ValueChanged += (s, e) => { LoadFilters(); LoadAnalytics(); };
            _dtpEnd.ValueChanged += (s, e) => { LoadFilters(); LoadAnalytics(); };
            _cmbType.SelectedIndexChanged += (s, e) => LoadAnalytics();

            headerPanel.Controls.Add(title);
            headerPanel.Controls.Add(lblFrom);
            headerPanel.Controls.Add(_dtpStart);
            headerPanel.Controls.Add(lblTo);
            headerPanel.Controls.Add(_dtpEnd);
            headerPanel.Controls.Add(lblType);
            headerPanel.Controls.Add(_cmbType);

            // Summary Cards Panel
            TableLayoutPanel cardsPanel = new TableLayoutPanel { Dock = DockStyle.Top, Height = 140, ColumnCount = 3, Padding = new Padding(10) };
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            cardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            lblTotalIncome = CreateSummaryCard(cardsPanel, "Всього доходів", Color.FromArgb(76, 175, 80), 0);
            lblTotalExpenses = CreateSummaryCard(cardsPanel, "Всього витрат", Color.FromArgb(244, 67, 54), 1);
            lblBalance = CreateSummaryCard(cardsPanel, "Чистий баланс", Color.FromArgb(33, 150, 243), 2);

            // Details Label
            Label lblDetails = new Label { Text = "Список транзакцій", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(20, 0, 0, 5) };

            // Grid
            _gridAnalytics = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AllowUserToResizeRows = false
            };
            
            // Grid Styling
            _gridAnalytics.EnableHeadersVisualStyles = false;
            _gridAnalytics.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            _gridAnalytics.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _gridAnalytics.ColumnHeadersHeight = 40;
            _gridAnalytics.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            _gridAnalytics.RowTemplate.Height = 35;
            _gridAnalytics.GridColor = Color.FromArgb(230, 230, 230);

            Panel gridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 10, 20, 20) };
            gridContainer.Controls.Add(_gridAnalytics);

            Controls.Add(gridContainer);
            Controls.Add(lblDetails);
            Controls.Add(cardsPanel);
            Controls.Add(headerPanel);

            LoadFilters();
            LoadAnalytics();
        }

        private void LoadFilters()
        {
            if (_dtpStart == null || _dtpEnd == null) return;

            var start = _dtpStart.Value.Date;
            var end = _dtpEnd.Value.Date.AddDays(1).AddSeconds(-1);

            // Зберігаємо поточний вибір, щоб спробувати його відновити
            var currentSelection = _cmbType.SelectedItem as FilterOption;

            var options = new List<FilterOption>
            {
                new FilterOption { Name = "Всі записи", FilterType = "All" },
                new FilterOption { Name = "Тільки доходи", FilterType = "Income" },
                new FilterOption { Name = "Тільки витрати", FilterType = "Expense" },
                new FilterOption { Name = "Без категорії", FilterType = "Category", CategoryIds = new List<int> { 0 } }
            };

            // Отримуємо всі транзакції за період, щоб знати, які категорії реально використовувалися
            var transactionsInPeriod = _transactionRepo.GetAll()
                .Where(t => t.Date >= start && t.Date <= end)
                .ToList();
            var usedCategoryIds = transactionsInPeriod.Select(t => t.CategoryId).Distinct().ToHashSet();

            var allCategories = _categoryRepo.GetAll();
            
            // Групуємо категорії за назвою, щоб уникнути дублікатів при змінах у часі
            var groupedCategories = allCategories
                .GroupBy(c => c.Name)
                .ToList();

            foreach (var group in groupedCategories)
            {
                string catName = group.Key;
                var idsInGroup = group.Select(c => c.Id).ToList();

                // Перевіряємо, чи хоча б одна категорія з цієї групи була активна АБО мала транзакції
                bool isRelevant = group.Any(c => _categoryService.IsCategoryActiveInRange(c, start, end)) 
                                  || idsInGroup.Any(id => usedCategoryIds.Contains(id));

                if (isRelevant)
                {
                    options.Add(new FilterOption 
                    { 
                        Name = $"Категорія: {catName}", 
                        FilterType = "Category", 
                        CategoryIds = idsInGroup 
                    });
                }
            }

            // Будуємо словник з уже завантажених категорій (без зайвого запиту до БД)
            _categoriesDict = allCategories.ToDictionary(c => c.Id, c => c.Name);

            // Тимчасово відключаємо обробник, щоб уникнути подвійного виклику LoadAnalytics
            _cmbType.SelectedIndexChanged -= (s, e) => LoadAnalytics();
            
            _cmbType.DataSource = options;

            // Намагаємося відновити попередній вибір за назвою та типом
            if (currentSelection != null)
            {
                var newMatch = options.FirstOrDefault(o => 
                    o.FilterType == currentSelection.FilterType && 
                    o.Name == currentSelection.Name);
                
                if (newMatch != null)
                    _cmbType.SelectedItem = newMatch;
                else
                    _cmbType.SelectedIndex = 0;
            }
            else
            {
                _cmbType.SelectedIndex = 0;
            }

            _cmbType.SelectedIndexChanged += (s, e) => LoadAnalytics();
        }

        private Label CreateSummaryCard(TableLayoutPanel parent, string title, Color color, int column)
        {
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(10) };
            card.Paint += (s, e) => {
                using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };
            
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 10), Location = new Point(15, 15), AutoSize = true, ForeColor = Color.Gray };
            Label lblValue = new Label { Text = "0 ₴", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(15, 50), AutoSize = true, ForeColor = color };
            
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            parent.Controls.Add(card, column, 0);
            return lblValue;
        }

        private void LoadAnalytics()
        {
            if (_dtpStart == null || _dtpEnd == null || _cmbType == null || _cmbType.SelectedItem == null) return;

            var start = _dtpStart.Value.Date;
            var end = _dtpEnd.Value.Date.AddDays(1).AddSeconds(-1);

            var allPeriodTransactions = _transactionRepo.GetAll()
                .Where(t => t.Date >= start && t.Date <= end)
                .ToList();

            // Оновлюємо картки на основі ВСІХ транзакцій за період (незалежно від фільтру)
            decimal totalIncome = allPeriodTransactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            decimal totalExpenses = allPeriodTransactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            if (lblTotalIncome != null) lblTotalIncome.Text = totalIncome.ToString("C");
            if (lblTotalExpenses != null) lblTotalExpenses.Text = totalExpenses.ToString("C");
            if (lblBalance != null) lblBalance.Text = (totalIncome - totalExpenses).ToString("C");

            // Застосовуємо фільтр для таблиці
            var transactions = allPeriodTransactions;
            var filter = (FilterOption)_cmbType.SelectedItem;

            if (filter.FilterType == "Income")
            {
                transactions = transactions.Where(t => t.Type == "Income").ToList();
            }
            else if (filter.FilterType == "Expense")
            {
                transactions = transactions.Where(t => t.Type == "Expense").ToList();
            }
            else if (filter.FilterType == "Category")
            {
                transactions = transactions.Where(t => filter.CategoryIds.Contains(t.CategoryId)).ToList();
            }

            var displayList = transactions.Select(t => new {
                Дата = t.Date.ToString("dd.MM.yyyy HH:mm"),
                Категорія = t.CategoryId == 0 ? "(Без категорії)" : (_categoriesDict.TryGetValue(t.CategoryId, out var catName) ? catName : "Невідома"),
                Тип = t.Type == "Income" ? "Дохід" : "Витрата",
                Сума = t.Amount.ToString("C"),
                Опис = t.Description
            }).OrderByDescending(x => x.Дата).ToList();

            if (_gridAnalytics != null)
            {
                _gridAnalytics.DataSource = displayList;
            }
        }
    }
}
