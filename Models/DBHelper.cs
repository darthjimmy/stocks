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
        
        public static int GetUserId(string username)
        {
            int userID = -1;

            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
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
                }
            }

            return userID;
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
                    using (MySqlDataReader reader1 = cmd.ExecuteReader())
                    {
                        while (reader1.Read())
                        {
                            userID = reader1.GetInt32("userID");
                        }
                    }

                    cmd.CommandText = "SELECT COUNT(*) AS 'counter' FROM userInvestments WHERE userID = @userID";

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
            int newShares = 0;
            long stockID = -1;
            int userInvestmentsID = -1;
            decimal difference = 0;

            if (shares > 0)
            {
                decimal balance = GetBalance(username);
                decimal cost = StockPrice(ticker);
                int coster = cost;
                cost = cost * shares;
                if (balance < cost)
                {
                    return false;
                }
                difference = balance - cost;
            }

            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    if (shares > 0)
                    {
                        cmd.CommandText = "UPDATE user SET balance = @difference WHERE username = @username";

                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@difference", difference);

                        cmd.ExecuteNonQuery();
                    }
                
                    stockID = InsertStock(ticker);
                    if (stockID == -1)
                    {
                        // Something went horribly wrong here and didn't throw an error
                        return false;
                    }

                    cmd.CommandText = "SELECT userInvestmentsID, shares FROM userInvestments WHERE stockID = @stockID AND userID = @userID";
                    cmd.Parameters.AddWithValue("@stockID", stockID);
                    cmd.Parameters.AddWithValue("@userID", GetUserId(username));

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userInvestmentsID = reader.GetInt32("userInvestmentsID");
                            newShares = reader.GetInt32("shares");
                        }
                    }
                    int sharer = shares;
                    shares = shares + newShares;

                    if (userInvestmentsID == -1)
                    {
                        cmd.CommandText = "INSERT INTO userInvestments (userID, stockID, shares) VALUES (@userID, @stockID, @shares)";
                    }
                    else
                    {
                        cmd.CommandText = "UPDATE userInvestments SET shares = @shares WHERE userID = @userID AND stockID = @stockID";
                    }

                    cmd.Parameters.AddWithValue("@shares", shares);

                    if (cmd.ExecuteNonQuery() > 0) 
                    {
                        History(sharer, userID, username, coster);
                        return true;
                    }
                }
            }
            return false;
        }
        
        public static bool SellStock(string username, string ticker, int shares)
        {
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                shares = shares * (-1);
                int stockID = -1;
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT stockID FROM stocks WHERE ticker = @ticker";
                    cmd.Parameters.AddWithValue("@ticker", ticker);
                    
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stockID = reader.GetInt32("stockID");
                        }
                    }
                    
                    int oldShares = 0;
                    cmd.CommandText = "SELECT userInvestmentsID, shares FROM userInvestments WHERE stockID = @stockID AND userID = @userID";
                    cmd.Parameters.AddWithValue("@stockID", stockID);
                    cmd.Parameters.AddWithValue("@userID", GetUserId(username));

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userInvestmentsID = reader.GetInt32("userInvestmentsID");
                            oldShares = reader.GetInt32("shares");
                        }
                    }
                    
                    if (oldShares < shares)
                    {
                        return false;
                    }
                    decimal price = StockPrice(ticker);
                    decimal pricer = price;
                    price = price * shares;
                    int sharer = shares;
                    shares = oldshares + shares;
                    
                    cmd.CommandText = "UPDATE userInvestments SET shares = @shares WHERE userID = @userID AND stockID = @stockID";

                    cmd.Parameters.AddWithValue("@shares", shares);
                    
                    cmd.ExecuteNonQuery();
                    
                    cmd.CommandText = "SELECT balance FROM user WHERE userID = @userID";
                    
                    decimal balance = 0;
                    
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            balance = reader.GetDecimal("balance");
                        }
                    }
                    balance = balance + price;
                    
                    cmd.CommandText = "UPDATE user SET balance = @balance WHERE userID = @userID";
                    cmd.Parameters.AddWithValue("@balance", balance);
                    cmd.ExecuteNonQuery();
                }
                History(sharer, stockID, username, pricer);
            }
            return true;
        }
        
        public static bool History(int shares, int stockID, string username, decimal price)
        {
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    
                    cmd.CommandText = "INSERT INTO userInvestments (userID, stockID, dateOfChange, stockPrice, userShares) " +
                        "VALUES (@userID, @stockID, @dater, @price, @shares)";

                    cmd.Parameters.AddWithValue("@userID", GetUserId(username));
                    cmd.Parameters.AddWithValue("@stockID", stockID);
                    cmd.Parameters.AddWithValue("@shares", shares);
                    cmd.Parameters.AddWithValue("@dater", DateTime.now);
                    cmd.Parameters.AddWithValue("@price", price);
                    
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
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
                    decimal balance = GetBalance(username);
                    amount = amount + balance;
                    cmd.CommandText = "UPDATE user SET balance = @amount WHERE username = @username";
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@amount", amount);

                    if (cmd.ExecuteNonQuery() > 0) return true;
                        return false;
                    
                }
            }
        }

        /// <summary>
        /// Checks if a stock exists, otherwise inserts it
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        public static long InsertStock(string ticker)
        {
            long stockID = -1;

            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    // check if our stock is already there
                    cmd.CommandText = "SELECT stockID FROM stocks WHERE ticker = @ticker";
                    cmd.Parameters.AddWithValue("@ticker", ticker);

                    using (MySqlDataReader reader1 = cmd.ExecuteReader())
                    {
                        while (reader1.Read())
                        {
                            stockID = reader1.GetInt32("stockID");
                        }
                    }

                    if (stockID == -1)
                    {
                        // we need to create it
                        cmd.CommandText = "INSERT INTO stocks (ticker) VALUES (@ticker);";
                        cmd.ExecuteNonQuery();

                        stockID = cmd.LastInsertedId;
                    }
                }
            }

            return stockID;
        }
    }
}

