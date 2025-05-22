using LibraryBookingApp.DataBases;
using LibraryBookingApp.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для BookingWindow.xaml
    /// </summary>
    public partial class BookingWindow : Window
    {
        private string _connectionString = @"Data Source=DataBase.db;";
        public string CurrentUser { get; set; } //Имя пользователя
        private DateTime SelectedDate;

        public BookingWindow(string username)
        {
            InitializeComponent();
            CurrentUser = username;
            LoadBooks();
        }

        private void LoadBooks()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT ID, Title FROM Book", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BookComboBox.Items.Add(new ComboBoxItem() { Content = reader.GetString(1), Tag = reader.GetInt32(0) });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BookComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BookComboBox.SelectedItem != null)
            {
                int bookId = (int)((ComboBoxItem)BookComboBox.SelectedItem).Tag;
                UpdateAvailableCount(bookId);
            }
        }

        private void UpdateAvailableCount(int bookId)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT Quantity FROM Book WHERE ID = @BookId", connection))
                    {
                        command.Parameters.AddWithValue("@BookId", bookId);
                        int quantity = Convert.ToInt32(command.ExecuteScalar());
                        AvailableCountTextBlock.Text = quantity.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления количества экземпляров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ConfirmBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookComboBox.SelectedItem == null || BookingDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите книгу и дату!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int bookId = (int)((ComboBoxItem)BookComboBox.SelectedItem).Tag;
            DateTime bookingDate = BookingDatePicker.SelectedDate.Value;
            string bookingDateString = bookingDate.ToString("yyyy-MM-dd"); //Формат даты для SQLite
            

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Уменьшение количества экземпляров
                    using (var checkQuantityCommand = new SQLiteCommand("SELECT Quantity FROM Book WHERE ID = @BookId", connection))
                    {
                        checkQuantityCommand.Parameters.AddWithValue("@BookId", bookId);
                        int quantity = Convert.ToInt32(checkQuantityCommand.ExecuteScalar());

                        if (quantity <= 0)
                        {
                            MessageBox.Show("Книга недоступна для бронирования. Все экземпляры выданы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Добавление бронирования
                    using (var reservationCommand = new SQLiteCommand(
                        @"INSERT INTO Reservation (UserId, BookId, ReservationDate, Date, ReservationStatus) 
                  VALUES ((SELECT ID FROM User WHERE Username = @Username), @BookId, @ReservationDate, @Date, 'Ожидание')", connection))
                    {
                        reservationCommand.Parameters.AddWithValue("@Username", CurrentUser);
                        reservationCommand.Parameters.AddWithValue("@BookId", bookId);
                        reservationCommand.Parameters.AddWithValue("@ReservationDate", DateTime.Now.ToString("yyyy-MM-dd")); // Дата бронирования
                        reservationCommand.Parameters.AddWithValue("@Date", bookingDateString);
                        reservationCommand.ExecuteNonQuery();
                    }

                    // Обновление количества экземпляров
                    using (var updateQuantityCommand = new SQLiteCommand("UPDATE Book SET Quantity = Quantity - 1 WHERE ID = @BookId", connection))
                    {
                        updateQuantityCommand.Parameters.AddWithValue("@BookId", bookId);
                        updateQuantityCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Книга успешно забронирована!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка бронирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadBooks(); // Если поисковый запрос пустой, загружаем все книги
                return;
            }

            try
            {
                BookComboBox.Items.Clear(); // Очищаем список перед загрузкой результатов поиска

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Запрос для поиска книг с учетом регистра
                    using (var command = new SQLiteCommand(
                        "SELECT ID, Title FROM Book WHERE Title LIKE @SearchQuery", connection))
                    {
                        command.Parameters.AddWithValue("@SearchQuery", $"%{searchQuery}%");

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BookComboBox.Items.Add(new ComboBoxItem() { Content = reader.GetString(1), Tag = reader.GetInt32(0) });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска книг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}  

