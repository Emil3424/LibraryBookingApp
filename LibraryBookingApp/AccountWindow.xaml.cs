using System.Data.SQLite;
using System;
using System.Windows;
using LibraryBookingApp.DataBases;
using System.Collections.Generic;

namespace LibraryBookingApp
{
    /// <summary>
    /// Логика взаимодействия для AccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        private string _connectionString = @"Data Source=DataBase.db;";
        public string username { get; set; }
        public AccountWindow(string user)
        {
            InitializeComponent();
            username = user;
            LoadUserData();
            LoadBookedBooks();
        }

        private void LoadUserData()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT FirstName, LastName, ReaderNumber FROM User WHERE Username = @Username", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstName = reader["FirstName"]?.ToString() ?? "";
                                string lastName = reader["LastName"]?.ToString() ?? "";
                                string readerNumber = reader["ReaderNumber"]?.ToString() ?? "";

                                UserNameTextBlock.Text = $"{firstName} {lastName}";
                                UserReaderNumberTextBlock.Text = readerNumber;
                            }
                            else
                            {
                                MessageBox.Show("Пользователь не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBookedBooks()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(
                        @"SELECT b.Title, b.Author, b.Genre, r.Date
                FROM Reservation r
                INNER JOIN Book b ON r.BookId = b.ID
                INNER JOIN User u ON r.UserId = u.ID
                WHERE u.Username = @Username AND r.ReservationStatus = 'Ожидание'", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            var bookedBooks = new List<object>();
                            while (reader.Read())
                            {
                                bookedBooks.Add(new
                                {
                                    Title = reader["Title"]?.ToString(),
                                    Author = reader["Author"]?.ToString(),
                                    Genre = reader["Genre"]?.ToString(),
                                    ReservationDate = DateTime.Parse(reader["Date"].ToString()) // Преобразуем строку в дату
                                });
                            }

                            BookedBooksListBox.ItemsSource = bookedBooks;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки забронированных книг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookedBooksListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите бронирование для отмены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedBooking = (dynamic)BookedBooksListBox.SelectedItem;

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Получаем ID книги на основе выбранного бронирования
                    using (var bookIdCommand = new SQLiteCommand(
                        @"SELECT b.ID 
                  FROM Reservation r
                  INNER JOIN Book b ON r.BookId = b.ID
                  INNER JOIN User u ON r.UserId = u.ID
                  WHERE b.Title = @Title AND u.Username = @Username AND r.ReservationStatus = 'Ожидание'", connection))
                    {
                        bookIdCommand.Parameters.AddWithValue("@Title", selectedBooking.Title);
                        bookIdCommand.Parameters.AddWithValue("@Username", username);

                        var bookId = Convert.ToInt32(bookIdCommand.ExecuteScalar());

                        // Удаляем бронирование
                        using (var deleteCommand = new SQLiteCommand(
                            @"DELETE FROM Reservation 
                      WHERE UserId = (SELECT ID FROM User WHERE Username = @Username) 
                      AND BookId = @BookId AND ReservationStatus = 'Ожидание'", connection))
                        {
                            deleteCommand.Parameters.AddWithValue("@Username", username);
                            deleteCommand.Parameters.AddWithValue("@BookId", bookId);
                            deleteCommand.ExecuteNonQuery();
                        }

                        // Увеличиваем количество экземпляров книги
                        using (var updateQuantityCommand = new SQLiteCommand("UPDATE Book SET Quantity = Quantity + 1 WHERE ID = @BookId", connection))
                        {
                            updateQuantityCommand.Parameters.AddWithValue("@BookId", bookId);
                            updateQuantityCommand.ExecuteNonQuery();
                        }

                        MessageBox.Show("Бронирование отменено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadBookedBooks(); // Обновляем список забронированных книг
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отмены бронирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}