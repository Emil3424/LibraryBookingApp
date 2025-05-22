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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibraryBookingApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _connectionString = @"Data Source=DataBase.db;";
        public string username {get; set;}
        public MainWindow(string usernamee)
        {
            InitializeComponent();
            username = usernamee;
        }

        private void BookingButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the AllGenresBook page
            BookingWindow bookingWindow = new BookingWindow(username);
            bookingWindow.ShowDialog();
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {
           AccountWindow AccountWindow = new AccountWindow(username);
            AccountWindow.Show();

        }

        private void GenreButton_Click(object sender, RoutedEventArgs e)
        {
            // Переход обратно на экран входа
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }




        private void GenreButton_Click1(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AllGenresBook(username));
        }
    }
}