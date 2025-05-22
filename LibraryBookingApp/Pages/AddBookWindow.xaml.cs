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

namespace LibraryBookingApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddBookWindow.xaml
    /// </summary>
    public partial class AddBookWindow : Window
    {
        private string _connectionString = @"Data Source=DataBase.db;";

        public AddBookWindow()
        {
            InitializeComponent();
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string author = AuthorTextBox.Text;
            string genre = GenreTextBox.Text;
            if (int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                try
                {
                    using (var connection = new SQLiteConnection(_connectionString))
                    {
                        connection.Open();
                        using (var command = new SQLiteCommand("INSERT INTO Book (Title, Author, Genre, Quantity) VALUES (@Title, @Author, @Genre, @Quantity)", connection))
                        {
                            command.Parameters.AddWithValue("@Title", title);
                            command.Parameters.AddWithValue("@Author", author);
                            command.Parameters.AddWithValue("@Genre", genre);
                            command.Parameters.AddWithValue("@Quantity", quantity);
                            command.ExecuteNonQuery();
                            DialogResult = true;
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show($"Ошибка добавления книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Некорректное количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}