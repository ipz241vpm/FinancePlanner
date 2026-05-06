using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FinancePlanner.Repositories;
using FinancePlanner.Repositories.Interfaces;

namespace FinancePlanner.Forms
{
    public class AddTransactionForm : Form
    {
        public decimal Amount { get; private set; }
        public string Description { get; private set; }
        public int CategoryId { get; private set; }
        public DateTime Date { get; private set; }
        public bool IsDeleted { get; private set; }

        private TextBox txtAmount;
        private TextBox txtDescription;
        private ComboBox cmbCategory;
        private DateTimePicker dtpDate;
        private string _internalTransactionType; // "Income" or "Expense"
        private ICategoryRepository _categoryRepo;
        private Transaction _existingTransaction;

        public AddTransactionForm(
            string transactionType, 
            ICategoryRepository categoryRepo,
            Transaction existingTransaction = null
            )
        {
            _existingTransaction = existingTransaction;
            
            if (_existingTransaction != null)
            {
                _internalTransactionType = _existingTransaction.Type;
                Text = "Редагувати транзакцію";
            }
            else
            {
                _internalTransactionType = transactionType.ToLower() == "дохід" ? "Income" : "Expense";
                Text = $"Додати {transactionType}";
            }

            _categoryRepo = categoryRepo;

            Size = new Size(350, 360);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // Сума
            Label lblAmount = new Label { Text = "Сума:", Location = new Point(30, 30), AutoSize = true };
            txtAmount = new TextBox { Location = new Point(100, 28), Width = 180 };

            // Категорія
            Label lblCategory = new Label { Text = "Категорія:", Location = new Point(30, 70), AutoSize = true };
            cmbCategory = new ComboBox { Location = new Point(100, 68), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            
            Button btnAddCategory = new Button { Text = "+", Location = new Point(250, 67), Width = 30 };
            btnAddCategory.Click += BtnAddCategory_Click;

            // Дата
            Label lblDate = new Label { Text = "Дата:", Location = new Point(30, 110), AutoSize = true };
            dtpDate = new DateTimePicker { Location = new Point(100, 108), Width = 180, Format = DateTimePickerFormat.Short };
            dtpDate.ValueChanged += (s, e) => LoadCategories();

            // Опис
            Label lblDescription = new Label { Text = "Опис:", Location = new Point(30, 150), AutoSize = true };
            txtDescription = new TextBox { Location = new Point(100, 148), Width = 180 };

            // Кнопки
            Button btnOk = new Button { Text = "Зберегти", Location = new Point(60, 210), Width = 90 };
            btnOk.Click += BtnOk_Click;

            Button btnCancel = new Button { Text = "Скасувати", Location = new Point(170, 210), Width = 90 };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            if (_existingTransaction != null)
            {
                Button btnDelete = new Button { Text = "Видалити", Location = new Point(115, 260), Width = 100, BackColor = Color.LightCoral };
                btnDelete.Click += (s, e) => 
                {
                    if (MessageBox.Show("Ви впевнені, що хочете видалити цю транзакцію?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        IsDeleted = true;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                };
                Controls.Add(btnDelete);
            }

            Controls.Add(lblAmount);
            Controls.Add(txtAmount);
            Controls.Add(lblCategory);
            Controls.Add(cmbCategory);
            Controls.Add(btnAddCategory);
            Controls.Add(lblDate);
            Controls.Add(dtpDate);
            Controls.Add(lblDescription);
            Controls.Add(txtDescription);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            LoadCategories();

            if (_existingTransaction != null)
            {
                txtAmount.Text = _existingTransaction.Amount.ToString();
                txtDescription.Text = _existingTransaction.Description;
                dtpDate.Value = _existingTransaction.Date;
                cmbCategory.SelectedValue = _existingTransaction.CategoryId;
            }
        }

        private void LoadCategories()
        {
            var allCategories = _categoryRepo.GetByType(_internalTransactionType);
            
            int selectedYear = dtpDate.Value.Year;
            int selectedMonth = dtpDate.Value.Month;

            var filteredCategories = allCategories
                .Where(c => 
                {
                    if (c.StartYear > 0 && (selectedYear < c.StartYear || (selectedYear == c.StartYear && selectedMonth < c.StartMonth)))
                        return false;
                    if (c.EndYear > 0 && (selectedYear > c.EndYear || (selectedYear == c.EndYear && selectedMonth > c.EndMonth)))
                        return false;
                    return true;
                })
                .ToList();

            var displayList = new List<Category> { new Category { Id = 0, Name = "Без категорії" } };
            displayList.AddRange(filteredCategories);

            cmbCategory.DataSource = displayList;
            cmbCategory.DisplayMember = "Name";
            cmbCategory.ValueMember = "Id";
            
            if (_existingTransaction == null)
                cmbCategory.SelectedIndex = 0;
        }

        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            using (var form = new AddCategoryForm(_internalTransactionType, dtpDate.Value, _categoryRepo))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadCategories();
                }
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtAmount.Text, out decimal amount) && amount > 0)
            {
                Amount = amount;
                Description = txtDescription.Text.Trim();
                CategoryId = (int)cmbCategory.SelectedValue;
                Date = dtpDate.Value;
                
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Введіть коректну суму більше нуля!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
