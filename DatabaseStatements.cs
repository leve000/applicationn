using application;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationWpf
{
    internal class DatabaseStatements
    {
        Connect conn = new Connect();

        public object AddNewUser(object user)
        {
            try
            {
                conn._connection.Open();
                var newUser = user.GetType().GetProperties();

                string salt = GenerateSalt();
                string hashedPassword = ComputeHmacSha256(newUser[2].GetValue(user).ToString(), salt);

                string sql = "INSERT INTO `users`(`Version`, `UserName`, `Password`, `RegTime`, `DateTime`) VALUES (@Version,@Username,@Password,@RegTime,@DateTime)";

                MySqlCommand cmd = new MySqlCommand(sql, conn._connection);

                cmd.Parameters.AddWithValue("@username", newUser[0].GetValue(user));
                cmd.Parameters.AddWithValue("@fullname", newUser[1].GetValue(user));
                cmd.Parameters.AddWithValue("@password", hashedPassword);
                cmd.Parameters.AddWithValue("@salt", salt);
                cmd.Parameters.AddWithValue("@email", newUser[4].GetValue(user));

                cmd.ExecuteNonQuery();

                conn._connection.Close();

                return new { message = "Sikeres hozzáadás." };
            }
            catch (System.Exception ex)
            {
                return new { message = ex.Message };
            }

        }

        public bool LoginUser(object user)
        {
            try
            {
                conn._connection.Open();

                string sql = "SELECT * FROM users WHERE UserName = @username;";

                MySqlCommand cmd = new MySqlCommand(sql, conn._connection);

                var logUser = user.GetType().GetProperties();

                cmd.Parameters.AddWithValue("@username", logUser[0].GetValue(user));

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string storedHash = reader.GetString(3);
                    string storedSalt = reader.GetString(4);
                    string computeHash = ComputeHmacSha256(logUser[1].GetValue(user).ToString(), storedSalt);
                    conn._connection.Close();

                    return storedHash == computeHash;
                }
                conn._connection.Close();
                return false;
            }
            catch
            {
                return false;
            }

        }

        public DataView UserList()
        {
            try
            {
                conn._connection.Open();
                string sql = "SELECT `UserName`,`FullName`,`Email`,`RegDate` FROM `users` ";

                MySqlCommand cmd = new MySqlCommand(sql, conn._connection);

                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn._connection);

                DataTable dt = new DataTable();

                adapter.Fill(dt);

                conn._connection.Close();

                return dt.DefaultView;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public object DeleteUser(object id)
        {
            try
            {
                conn._connection.Open();

                string sql = "DELETE FROM users WHERE Id = @id";

                MySqlCommand cmd = new MySqlCommand(sql, conn._connection);

                var idObj = id.GetType().GetProperties();

                cmd.Parameters.AddWithValue("@id", idObj[0].GetValue(id));

                cmd.ExecuteNonQuery();

                conn._connection.Close();
                return new { message = "Sikeres törlés." };
            }
            catch (Exception ex)
            {

                return new { message = ex.Message };
            }


        }

        public string GenerateSalt()
        {
            byte[] salt = new byte[16];

            using (var rnd = RandomNumberGenerator.Create())
            {
                rnd.GetBytes(salt);
            }

            return Convert.ToBase64String(salt);
        }

        public string ComputeHmacSha256(string password, string salt)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }
    }
}