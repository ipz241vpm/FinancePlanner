using System;
using System.Drawing;
using System.Windows.Forms;
using FinancePlanner.Repositories;
using FinancePlanner.Repositories.Interfaces;

namespace FinancePlanner.Forms
{
    public class AddCategoryForm : Form
    {
        private string _type;
        private TextBox txtName;
        private TextBox txtProjectedAmount;
        private DateTime _selectedDate;
        private readonly ICategoryRepository _categoryRepo;
        
        private RadioButton rbAlways;
        private RadioButton rbFromNow;
        private RadioButton rbOnlyThis;

        public AddCategoryForm(string type, DateTime selectedDate, ICategoryRepository categoryRepository)
        {
            _categoryRepo = categoryRepository;
            _selectedDate = selectedDate;
            _type = type;
            Text = "Додати категорію";
            Size = new Size(320, 260);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label lblName = new Label { Text = "Назва:", Location = new Point(20, 20), AutoSize = true };
            txtName = new TextBox { Location = new Point(100, 18), Width = 160 };

            Label lblProjected = new Label { Text = "Прогноз:", Location = new Point(20, 60), AutoSize = true };
            txtProjectedAmount = new TextBox { Location = new Point(100, 58), Width = 160, Text = "0" };

            Label lblScope = new Label { Text = "Період:", Location = new Point(20, 95), AutoSize = true };
            rbAlways = new RadioButton { Text = "Постійна (для всіх)", Location = new Point(100, 93), Width = 180, Checked = true };
            rbFromNow = new RadioButton { Text = "З цього місяця", Location = new Point(100, 118), Width = 180 };
            rbOnlyThis = new RadioButton { Text = "Лише на цей місяць", Location = new Point(100, 143), Width = 180 };

            Button btnOk = new Button { Text = "Зберегти", Location = new Point(60, 180), Width = 90 };
            btnOk.Click += BtnOk_Click;

            Button btnCancel = new Button { Text = "Скасувати", Location = new Point(160, 180), Width = 90 };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblProjected);
            Controls.Add(txtProjectedAmount);
            Controls.Add(lblScope);
            Controls.Add(rbAlways);
            Controls.Add(rbFromNow);
            Controls.Add(rbOnlyThis);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (!decimal.TryParse(txtProjectedAmount.Text.Trim(), out decimal projectedAmount) || projectedAmount < 0)
            {
                MessageBox.Show("Введіть коректну суму прогнозу (більше або рівну 0)!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!string.IsNullOrEmpty(name))
            {
                int startMonth = 0, startYear = 0, endMonth = 0, endYear = 0;
                
                if (rbFromNow.Checked)
                {
                    startMonth = _selectedDate.Month;
                    startYear = _selectedDate.Year;
                }
                else if (rbOnlyThis.Checked)
                {
                    startMonth = _selectedDate.Month;
                    startYear = _selectedDate.Year;
                    endMonth = _selectedDate.Month;
                    endYear = _selectedDate.Year;
                }

                var newCategory = new Category
                {
                    Name = name,
                    Type = _type,
                    ProjectedAmount = projectedAmount,
                    StartMonth = startMonth,
                    StartYear = startYear,
                    EndMonth = endMonth,
                    EndYear = endYear
                };

                _categoryRepo.Add(newCategory);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Введіть назву категорії!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
