using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=localhost;Database=application;Uid=root;Pwd=;";

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT Id, Version, UserName, Password, Salt, RegTime, ModTime FROM datas", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGrid.ItemsSource = dt.DefaultView;
            }
        }

        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            string version = txtVersion.Text;
            string username = txtUserName.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Tölts ki minden mezőt!");
                return;
            }

            string salt = GenerateSalt();
            string hashedPassword = HashPassword(password, salt);

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "INSERT INTO datas (Version, UserName, Password, Salt, RegTime, ModTime) " +
                               "VALUES (@version, @username, @password, @salt, @regtime, @modtime)";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@version", version);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@salt", salt);
                    cmd.Parameters.AddWithValue("@regtime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@modtime", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }

            LoadData();
            txtVersion.Clear();
            txtUserName.Clear();
            txtPassword.Clear();
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
                return Convert.ToBase64String(bytes);
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid.SelectedItem is DataRowView row)
            {
                string hashed = row["Password"].ToString();
                string salt = row["Salt"].ToString();
                MessageBox.Show("Password (hashed, nem visszafejthető): " + hashed);
            }
        }
    }
}
