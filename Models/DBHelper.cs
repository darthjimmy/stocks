using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;

namespace stocks
{
    public class DBHelper
    {
        static private MySqlConnectionStringBuilder connstring = new MySqlConnectionStringBuilder("" +
            "Server=cs4450.cj7o28wmyp47.us-east-2.rds.amazonaws.com;" +
            "UID=erics;" +
            "password=password;" +
            "database=Stock");
        public static bool logger(string username, string password, string type)
        {
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    if (type == "new")
                    {
                        cmd.CommandText = "SELECT username FROM user WHERE username = @username";

                        cmd.Parameters.AddWithValue("@username", username);
                        //reader function

                        var username2 = "";

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                username2 = reader.GetString("username");
                            }
                        }

                        if (username2 == "")
                        {
                            cmd.CommandText = "INSERT INTO user (username, password) VALUES (@username, @password)";
                            cmd.Parameters.AddWithValue("@password", password);

                            if (cmd.ExecuteNonQuery() > 0) return true;
                            return false;
                        }
                        return false;
                    }
                    else if (type == "login")
                    {
                        //reader function
                        var username2 = "";

                        cmd.CommandText = "SELECT username FROM user WHERE username = @username";

                        cmd.Parameters.AddWithValue("@username", username);
                        //reader function

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                username2 = reader.GetString("username");
                            }
                        }
                        if (username2 == username)
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }
            
            return false;
        }
    }
}
