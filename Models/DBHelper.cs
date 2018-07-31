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
                    if (type == "creation")
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
                            cmd.CommandText = "INSERT INTO user (username, password, balance) VALUES (@username, @password, 0)";
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
        
        public static decimal GetBalance(string username)
        {
            decimal amount = 0;
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    
                    cmd.CommandText = "SELECT balance FROM user WHERE username = @username";

                    cmd.Parameters.AddWithValue("@username", username);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            amount = reader.GetDecimal("balance");
                        }
                    }

                    return amount;
                    
                }
            }
        }
        
        public static bool DoStocks(string username)
        {
            int amount = 0;
            int userID = -1;
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT userID FROM user WHERE username = @username";
                    cmd.Parameters.AddWithValue("@username", username);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userID = reader.GetInt32("userID");
                        }
                    }

                    cmd.CommandText = "SELECT COUNT(*) AS 'counter' FROM stocks WHERE userID = @userID";

                    cmd.Parameters.AddWithValue("@userID", userID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            amount = reader.GetInt32("counter");
                        }
                    }

                    if (amount > 3) return true;
                    return false;


                }
            }
        }
        
        public static bool PurchaseStock(string username, string ticker, int shares)
        {
            decimal balance = GetBalance(username);
            decimal cost = StockPrice(ticker);
            int newShares = 0;
            int stockID = -1;
            int userInvestmentsID = -1;
            cost = cost * shares;
            if (balance < cost)
            {
                return false;
            }
            decimal difference = balance - cost;
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = "UPDATE user SET balance = @difference WHERE username = @username";

                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@difference", difference);

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "SELECT stockID FROM stocks WHERE ticker = @ticker";
                    cmd.Parameters.AddWithValue("@ticker", ticker);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stockID = reader.GetInt32("stockID");
                        }
                    }

                    cmd.CommandText = "SELECT userInvestmentsID, shares FROM userInvestments WHERE stockID = @stockID";
                    cmd.Parameters.AddWithValue("@stockID", stockID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userInvestmentsID = reader.GetInt32("userInvestmentsID");
                            newShares = reader.GetInt32("shares");
                        }
                    }

                    shares = shares + newShares;
                    if (userInvestmentsID == -1)
                    {
                        cmd.CommandText = "INSERT INTO userInvestmentsID (userID, stockID, shares) VALUES (@userID, @stockID, @shares)";
                    }
                    else
                    {
                        cmd.CommandText = "UPDATE userInvestmentsID SET shares = @shares WHERE userID = @userID AND stockID = @stockID";
                    }
                    cmd.Parameters.AddWithValue("@shares", shares);

                    if (cmd.ExecuteNonQuery() > 0) return true;
                }
            }
            return false;
        }

        public static decimal StockPrice(string ticker)
        {
            decimal amount = 0;
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = "SELECT costPerShare FROM stocks WHERE ticker = @ticker";

                    cmd.Parameters.AddWithValue("@ticker", ticker);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            amount = reader.GetDecimal("costPerShare");
                        }
                    }

                    return amount;

                }
            }
        }
        
        public static bool AddBalance(decimal amount, string username)
        {
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    
                    cmd.CommandText = "UPDATE user SET balance = @amount WHERE username = @username";

                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@amount", amount);

                    if (cmd.ExecuteNonQuery() > 0) return true;
                        return false;
                    
                }
            }
        }
    }
}

