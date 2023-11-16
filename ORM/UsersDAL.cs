using SSHUserManagement.Model;
using System.Data;
using System.Data.SqlClient;

namespace SSHUserManagement.ORM
{
    public class UsersDAL
    {
        private static readonly string connectionString = AppSetting.SQLDB;
        public static Users Get(string email)
        {
            Users user = new Users();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    var cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"Select * from users where IsDeleted = 0 And Email = '{email}'";

                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        user.Id = Convert.ToInt64(rdr["Id"]);
                        user.ChatId = Convert.ToInt64(rdr["ChatId"]);
                        user.Email = rdr["Email"].ToString();
                        user.TelegramId = rdr["TelegramId"].ToString();
                        user.ExpDate = rdr["ExpDate"].ToString();
                        user.Name = rdr["Name"].ToString();
                        user.Password = rdr["Password"].ToString();
                    }
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return user;
        }
        public static Users Get(long chatId)
        {
            Users user = new Users();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    var cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"Select * from users where  IsDeleted = 0 And  chatId = '{chatId}'";

                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        user.Id = Convert.ToInt32(rdr["Id"]);
                        user.Email = rdr["Email"].ToString();
                        user.TelegramId = rdr["TelegramId"].ToString();
                        user.ChatId = Convert.ToInt64(rdr["ChatId"]);
                        user.Name = rdr["Name"].ToString();
                        user.ExpDate = rdr["ExpDate"].ToString();
                        user.Password = rdr["Password"].ToString();
                    }
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return user;
        }
        public static List<Users> GetAll()
        {

            List<Users> users = new List<Users>();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    var cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"Select * from users Where  IsDeleted = 0     ";

                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        Users user = new Users();
                        user.Id = Convert.ToInt32(rdr["Id"]);
                        user.Email = rdr["Email"].ToString();
                        user.ChatId = Convert.ToInt64(rdr["ChatId"]);
                        user.Name = rdr["Name"].ToString();
                        user.ExpDate = rdr["ExpDate"].ToString();
                        user.Password = rdr["Password"].ToString();
                        user.TelegramId = rdr["TelegramId"]?.ToString();
                        users.Add(user);
                    }
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }

            return users;
        }
        public static bool Insert(Users user)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string commandString = "INSERT INTO Users (TelegramId, Name, Email,ExpDate, Password, ChatId) VALUES (@TelegramId, @Name, @Email,@ExpDate ,@Password, @ChatId)";
            SqlCommand command = new SqlCommand(commandString, connection);
            command.Parameters.AddWithValue("@TelegramId", user.TelegramId);
            command.Parameters.AddWithValue("@Name", user.Name);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@ExpDate", user.ExpDate);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@ChatId", user.ChatId);
            command.Parameters.AddWithValue("@IsDeleted", user.IsDeleted);
            int rowsAffected = command.ExecuteNonQuery();
            connection.Close();
            command.Dispose();
            connection.Dispose();
            if (rowsAffected > 0)
                return true;
            else { return false; }
        }
        public static bool Delete(long id)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string commandString = "UPDATE dbo.Users SET IsDeleted = 1 WHERE Id = @Id";
            SqlCommand command = new SqlCommand(commandString, connection);
            command.Parameters.AddWithValue("@Id", id);
            int rowsAffected = command.ExecuteNonQuery();
            connection.Close();
            command.Dispose();
            connection.Dispose();
            if (rowsAffected > 0)
                return true;
            else { return false; }
        }
    }
}