using LibraryBookingApp;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LibraryBookingApp
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        private string _connectionString = @"Data Source=DataBase.db;";


        public LoginWindow()
        {
            InitializeComponent();
        }

        // Обработчик кнопки "Войти"
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка на администратора
            if (username == "admin" && password == "1111")
            {
                //Действия для администратора
                MessageBox.Show("Вход выполнен успешно (Администратор)", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow mainWindow = new MainWindow(username);
                mainWindow.Show();

                // Закрываем текущее окно входа
                this.Close();
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM User WHERE Username = @Username AND Password = @Password", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                MessageBox.Show("Вход выполнен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                MainWindow mainWindow = new MainWindow(username);
                                mainWindow.Show();

                                // Закрываем текущее окно входа
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик кнопки "Регистрация"
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно регистрации 
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();

            // Закрываем текущее окно входа
            this.Close();
        }
    }
}