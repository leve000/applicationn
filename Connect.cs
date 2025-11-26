using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace application
{
    internal class Connect
    {
        public MySqlConnection _connection;

        private string _host;
        private string _db;
        private string _user;
        private string _password;

        private string _connectionString;
        public Connect()
        {
            _host = "localhost";
            _db = "application";
            _user = "root";
            _password = "";

            _connectionString = $"SERVER={_host};DATABASE={_db};UID={_user};PASSWORD={_password};SslMode=None";

            _connection = new MySqlConnection(_connectionString);
        }

    }
}