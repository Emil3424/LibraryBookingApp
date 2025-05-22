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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibraryBookingApp
{
    /// <summary>
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        
        private string _connectionString = @"Data Source=DataBase.db;";
        private string _generatedReaderNumber;
        public RegisterWindow()
        {
            InitializeComponent();
            GenerateReaderNumber(); // Генерация номера при загрузке окна
            ReaderNumberTextBox.Text = _generatedReaderNumber; // Заполняем поле
            ReaderNumberTextBox.IsReadOnly = true; // Делаем поле только для чтения
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegUsernameTextBox.Text;
            string password = RegPasswordBox.Password;
            string lastName = LastNameTextBox.Text;
            string firstName = FirstNameTextBox.Text;
            string middleName = MiddleNameTextBox.Text;
            string address = AddressTextBox.Text;
            string phoneNumber = PhoneNumberTextBox.Text;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) ||
                string.IsNullOrEmpty(address) || string.IsNullOrEmpty(phoneNumber))
            {
                MessageBox.Show("Все поля должны быть заполнены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Проверка на существование пользователя
                    using (var checkCommand = new SQLiteCommand("SELECT COUNT(*) FROM User WHERE Username = @Username", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", username);
                        int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (userCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким именем уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    // Вставка нового пользователя
                    using (var command = new SQLiteCommand(@"INSERT INTO User (Username, Password, LastName, FirstName, MiddleName, ReaderNumber, Address, PhoneNumber) 
                                                        VALUES (@Username, @Password, @LastName, @FirstName, @MiddleName, @ReaderNumber, @Address, @PhoneNumber)", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@MiddleName", middleName);
                        command.Parameters.AddWithValue("@Address", address);
                        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        command.Parameters.AddWithValue("@ReaderNumber", _generatedReaderNumber); // Используем заранее сгенерированный номер
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();

                    this.Close();
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateReaderNumber()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    _generatedReaderNumber = GenerateUniqueReaderNumber(connection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации номера читательского билета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _generatedReaderNumber = "Ошибка";
            }
        }
        private string GenerateUniqueReaderNumber(SQLiteConnection connection)
        {
            Random random = new Random();
            string readerNumber;
            bool isUnique;

            do
            {
                // Генерация случайного числа от 0 до 9999999
                int randomNumber = random.Next(0, 10000000);
                readerNumber = randomNumber.ToString("0000000"); // Преобразуем в строку с 7 цифрами

                // Проверка уникальности в базе данных
                using (var checkCommand = new SQLiteCommand("SELECT COUNT(*) FROM User WHERE ReaderNumber = @ReaderNumber", connection))
                {
                    checkCommand.Parameters.AddWithValue("@ReaderNumber", readerNumber);
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    isUnique = count == 0;
                }
            }
            while (!isUnique);

            return readerNumber;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

    }
}
