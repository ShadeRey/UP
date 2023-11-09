using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MySqlConnector;

namespace UPNew.Views;

public partial class LogInUp : Window
{
    string Email => EmailBox.Text;
    string Password => PasswordBox.Text;
        
    //string connectionString = "Server=10.10.1.24;Database=pro1_23;User Id=user_01;Password=user01pro";
     string connectionString = "Server=localhost;Database=UP;User Id=root;Password=sharaga228";
    public LogInUp() {
        InitializeComponent();
    }
    
    private bool AuthenticateUser(string Email, string Password)
    {
        // Подключение к базе данных
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            // Выполнение SQL-запроса для проверки аутентификации
            string query = "SELECT COUNT(*) FROM Teacher WHERE Email = @Email AND Password = @Password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", Email);
            command.Parameters.AddWithValue("@Password", Password);

            int count = Convert.ToInt32(command.ExecuteScalar());

            return count > 0; // Если есть соответствие, вернуть true, иначе false
        }
    }

    private static readonly FontFamily COCFont =
        FontFamily.Parse("avares://UP/Assets/Supercell-Magic.ttf#Supercell-Magic");
    
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (AuthenticateUser(Email, Password))
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        else
        {
            TextBlock authError = new TextBlock()
            {
                FontFamily = COCFont,
                Foreground = Brushes.Red
            };
            authError.Text = "Invalid email or password";
            AuthPanel.Children.Add(authError);
            var secondsToDestroy = 5;
            var timer = new DispatcherTimer(TimeSpan.FromSeconds(secondsToDestroy), DispatcherPriority.Background, (o, args) =>
            {
                AuthPanel.Children.Remove(authError);
                if (o is DispatcherTimer dt)
                {
                    dt.Stop();
                }
            });
            timer.Start();
        }
    }
}