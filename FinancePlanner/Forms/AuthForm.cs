using System;
using System.Drawing;
using System.Windows.Forms;
using FinancePlanner.Repositories;
using FinancePlanner.Repositories.Interfaces;
using FinancePlanner.Services;

namespace FinancePlanner.Forms
{
    public class AuthForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private IUserRepository _userRepository;

        public AuthForm(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            
            Text = "Авторизація";
            Size = new Size(350, 250);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label lblUsername = new Label { Text = "Логін:", Location = new Point(30, 30), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(120, 28), Width = 180 };

            Label lblPassword = new Label { Text = "Пароль:", Location = new Point(30, 70), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(120, 68), Width = 180, PasswordChar = '*' };

            Button btnLogin = new Button { Text = "Увійти", Location = new Point(50, 130), Width = 100 };
            btnLogin.Click += BtnLogin_Click;

            Button btnRegister = new Button { Text = "Реєстрація", Location = new Point(170, 130), Width = 100 };
            btnRegister.Click += BtnRegister_Click;

            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(btnRegister);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var user = _userRepository.Login(txtUsername.Text, txtPassword.Text);
            if (user != null)
            {
                SessionManager.Instance.CurrentUser = user;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Невірний логін або пароль!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Заповніть всі поля!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = _userRepository.Register(txtUsername.Text, txtPassword.Text);
            if (user != null)
            {
                SessionManager.Instance.CurrentUser = user;
                MessageBox.Show("Реєстрація успішна!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Користувач з таким логіном вже існує!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
