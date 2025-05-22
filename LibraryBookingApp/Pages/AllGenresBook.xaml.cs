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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace LibraryBookingApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для AllGenresBook.xaml
    /// </summary>
    public partial class AllGenresBook : Page
    {
        private ObservableCollection<DataBases.Book> _books = new ObservableCollection<DataBases.Book>();
        public ObservableCollection<DataBases.Book> Books { get => _books; set => _books = value; }
        private string _connectionString = @"Data Source=DataBase.db;";
        public string username { get; set; }
        public AllGenresBook(string usernamu)
        {
            InitializeComponent();
            DataContext = this;
            LoadBooks();
            username = usernamu;

            if (username == "admin")
            {
                AddButton.Visibility = Visibility.Visible;
                DelButton.Visibility = Visibility.Visible;
            }
            else
            {
                AddButton.Visibility = Visibility.Collapsed;
                DelButton.Visibility = Visibility.Collapsed;
            }

        }


        private void LoadBooks()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Загружаем книги из таблицы Book
                using (var command = new SQLiteCommand("SELECT * FROM Book", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Books.Add(new DataBases.Book
                            {
                                ID = reader.GetInt32(0),
                                Title = reader.IsDBNull(1) ? "" : reader.GetString(1), // Проверка на NULL
                                Author = reader.IsDBNull(2) ? "" : reader.GetString(2), // Проверка на NULL
                                Genre = reader.IsDBNull(3) ? "" : reader.GetString(3),   // Проверка на NULL
                                Quantity = reader.GetInt32(4)
                            });
                        }
                    }
                }
            }
            BooksListView.ItemsSource = Books;
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            AddBookWindow addBookWindow = new AddBookWindow();
            if (addBookWindow.ShowDialog() == true)
            {
                Books.Clear();
                LoadBooks();
            }
        }

        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (BooksListView.SelectedItem is LibraryBookingApp.DataBases.Book selectedBook)
            {
                if (MessageBox.Show($"Удалить книгу '{selectedBook.Title}'?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = new SQLiteConnection(_connectionString))
                        {
                            connection.Open();
                            using (var command = new SQLiteCommand("DELETE FROM Book WHERE ID = @ID", connection))
                            {
                                command.Parameters.AddWithValue("@ID", selectedBook.ID);
                                command.ExecuteNonQuery();
                            }
                        }
                        Books.Remove(selectedBook);
                    }
                    catch (SQLiteException ex)
                    {
                        MessageBox.Show($"Ошибка удаления книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для удаления.");
            }
        }

        private void GenreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedGenre = (GenreComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = selectedGenre == "Все жанры" || string.IsNullOrEmpty(selectedGenre)
                    ? "SELECT * FROM Book" // Без фильтрации
                    : "SELECT * FROM Book WHERE Genre = @Genre"; // С фильтрацией

                using (var command = new SQLiteCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(selectedGenre) && selectedGenre != "Все жанры")
                    {
                        command.Parameters.AddWithValue("@Genre", selectedGenre);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        Books.Clear(); // Очищаем коллекцию перед обновлением

                        while (reader.Read())
                        {
                            Books.Add(new DataBases.Book
                            {
                                ID = reader.GetInt32(0),
                                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Author = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Genre = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Quantity = reader.GetInt32(4)
                            });
                        }
                    }
                }
            }
        }
    }

}
