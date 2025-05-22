using Xunit;
using System.Windows;
using System.Data.SQLite;
using LibraryBookingApp;
using System.Threading.Tasks;
using System;

namespace LibraryBookingApp.Tests
{
    public class LoginTests
    {
        private const string TestConnectionString = @"Data Source=TestDatabase.db;Version=3;";  // Путь к вашей тестовой базе данных
        // Вспомогательный метод для очистки и создания тестовой базы данных
        private void SetupTestDatabase()
        {
            // Удаляем старую базу данных (если существует)
            if (System.IO.File.Exists("TestDatabase.db"))
            {
                System.IO.File.Delete("TestDatabase.db");
            }

            // Создаем новую базу данных и таблицы (аналогично вашему коду в приложении)
            using (var connection = new SQLiteConnection(TestConnectionString))
            {
                connection.Open();

                using (var createTableCommand = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS User (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL,
                        Password TEXT NOT NULL,
                        LastName TEXT,
                        FirstName TEXT,
                        MiddleName TEXT,
                        ReaderNumber TEXT,
                        Address TEXT,
                        PhoneNumber TEXT
                    );
                    CREATE TABLE IF NOT EXISTS Book (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Author TEXT,
                        Genre TEXT,
                        Quantity INTEGER DEFAULT 1
                    );
                    CREATE TABLE IF NOT EXISTS Reservation (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        BookId INTEGER NOT NULL,
                        ReservationDate TEXT,
                        Date TEXT NOT NULL,
                        ReservationStatus TEXT,
                        FOREIGN KEY (UserId) REFERENCES User(ID),
                        FOREIGN KEY (BookId) REFERENCES Book(ID)
                    );
                    ", connection))
                {
                    createTableCommand.ExecuteNonQuery();
                }

                // Добавляем тестового пользователя admin (для TC-01)
                using (var insertAdminCommand = new SQLiteCommand("INSERT INTO User (Username, Password) VALUES ('admin', '1111')", connection))
                {
                    insertAdminCommand.ExecuteNonQuery();
                }
            }
        }

        [Fact]
        public async Task SuccessfulAdminLogin()
        {
            // Arrange
            SetupTestDatabase(); // Настраиваем тестовую базу данных перед каждым тестом
            var loginWindow = new LoginWindow();
            loginWindow.UsernameTextBox.Text = "admin";
            loginWindow.PasswordBox.Password = "1111";

            // Act
            loginWindow.LoginButton_Click(null, null); // Симулируем нажатие кнопки

            // Assert
            await Task.Delay(100); // Ждем, чтобы окно успело открыться.  Увеличьте, если нужно.
            Assert.False(loginWindow.IsVisible, "Окно входа не должно быть видимым после успешного входа"); //Проверяем что окно LoginWindow закрыто.
            //Дополнительно: можно проверить, что открылось MainWindow.
        }

        [Fact]
        public async Task SuccessfulUserLogin()
        {
            SetupTestDatabase(); // Настраиваем тестовую базу данных перед каждым тестом

            // Arrange: Создаем тестового пользователя в базе данных
            using (var connection = new SQLiteConnection(TestConnectionString))
            {
                connection.Open();
                using (var insertUserCommand = new SQLiteCommand("INSERT INTO User (Username, Password) VALUES ('testuser', 'password')", connection))
                {
                    insertUserCommand.ExecuteNonQuery();
                }
            }
            var loginWindow = new LoginWindow();
            loginWindow.UsernameTextBox.Text = "testuser";
            loginWindow.PasswordBox.Password = "password";

            // Act
            loginWindow.LoginButton_Click(null, null);

            // Assert
            await Task.Delay(100); // Ждем, чтобы окно успело открыться.  Увеличьте, если нужно.
            Assert.False(loginWindow.IsVisible, "Окно входа не должно быть видимым после успешного входа"); //Проверяем что окно LoginWindow закрыто.
            // Дополнительно: можно проверить, что открылось MainWindow.
        }

        [Fact]
        public void FailedLoginInvalidCredentials()
        {
            SetupTestDatabase(); // Настраиваем тестовую базу данных перед каждым тестом
            // Arrange
            var loginWindow = new LoginWindow();
            loginWindow.UsernameTextBox.Text = "wronguser";
            loginWindow.PasswordBox.Password = "wrongpassword";

            // Act
            loginWindow.LoginButton_Click(null, null);

            // Assert
            Assert.True(loginWindow.IsVisible, "Окно входа должно оставаться видимым после неудачной попытки входа"); //Проверяем что окно LoginWindow осталось видимым.
            //Дополнительно: можно проверить, что отображается сообщение об ошибке.
        }
    }
}
