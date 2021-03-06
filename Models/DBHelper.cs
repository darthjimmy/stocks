using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

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

                    if (amount >= 3) return true;
                    return false;


                }
            }
        }
        
        public static List<UserHistory> HistoryR(string username)
        {
            List<UserHistory> history = new List<UserHistory>();
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = "SELECT ticker, dateOfChange, stockPrice, userShares " +
                        "FROM userStockHistory INNER JOIN stocks ON userStockHistory.stockID = stocks.stockID " +
                        "WHERE userid = @userID ORDER BY dateOfChange DESC";

                    cmd.Parameters.AddWithValue("@userID", GetUserId(username));

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            history.Add(new UserHistory()
                            {
                                ticker = reader.GetString("ticker"),
                                dateOfChange = reader.GetDateTime("dateOfChange"),
                                CostPerShare = reader.GetDecimal("stockPrice"),
                                userShares = reader.GetDouble("userShares"),
                            });
                        }
                    }

                    return history;

                }
            }
        }
        
        public static bool PurchaseStock(string username, string ticker, decimal shares)
        {
            decimal newShares = 0;
            long stockID = -1;
            int userInvestmentsID = -1;
            decimal difference = 0;
            decimal cost = StockPrice(ticker);
            decimal coster = cost;
            decimal sharer = shares;
            if (shares > 0)
            {
                decimal balance = GetBalance(username);
                
                cost = cost * shares;
                if (balance < 0)
                    return false;

                if (balance < cost)
                {
                    cost = balance;
                }
                difference = balance - cost;
                shares = (cost / coster);
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
                            newShares = reader.GetDecimal("shares");
                        }
                    }
                    sharer = shares;
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
                        History(sharer, stockID, username, coster);
                        return true;
                    }
                }
            }
            return false;
        }
        
        public static bool SellStock(string username, string ticker, decimal shares)
        {
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                shares = shares * (-1);
                int stockID = -1;
                decimal pricer = 0.0m;
                decimal sharer = 0.0m;
                decimal price = 0.0m;
                decimal oldShares = 0.0m;
                int userInvestmentsID = -1;
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
                    
                    cmd.CommandText = "SELECT userInvestmentsID, shares FROM userInvestments WHERE stockID = @stockID AND userID = @userID";
                    cmd.Parameters.AddWithValue("@stockID", stockID);
                    cmd.Parameters.AddWithValue("@userID", GetUserId(username));

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userInvestmentsID = reader.GetInt32("userInvestmentsID");
                            oldShares = reader.GetDecimal("shares");
                        }
                    }
                    
                    if (oldShares < (shares * -1))
                    {
                        shares = oldShares * -1;
                    }
                    price = StockPrice(ticker);
                    pricer = price;
                    price = price * (shares * -1);
                    sharer = shares;
                    shares = oldShares + shares;
                    
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
        
        public static bool History(decimal shares, long stockID, string username, decimal price)
        {
            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    
                    cmd.CommandText = "INSERT INTO userStockHistory (userID, stockID, dateOfChange, stockPrice, userShares) " +
                        "VALUES (@userID, @stockID, @dater, @price, @shares)";

                    cmd.Parameters.AddWithValue("@userID", GetUserId(username));
                    cmd.Parameters.AddWithValue("@stockID", stockID);
                    cmd.Parameters.AddWithValue("@shares", shares);
                    cmd.Parameters.AddWithValue("@dater", DateTime.Now);
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
                        StockDataClient client = new StockDataClient();
                        // we need to create it
                        cmd.CommandText = "INSERT INTO stocks (ticker, costPerShare) VALUES (@ticker, @costPerShare);";
                        cmd.Parameters.AddWithValue("@costPerShare", client.GetDelayedQuote(ticker).Result.DelayedPrice);
                        cmd.ExecuteNonQuery();

                        stockID = cmd.LastInsertedId;
                    }
                }
            }

            return stockID;
        }

        public static string GetStocks(string username)
        {
            string stockList = "";

            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT i.userInvestmentsID, u.username, s.ticker, s.costPerShare, i.shares from userInvestments i inner join stocks s on s.stockID = i.stockID inner join user u on u.userID = i.userID WHERE u.username = @username";
                    cmd.Parameters.AddWithValue("@username", username);

                    var stocks = new UserInvestmentCollection();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stocks.Stocks.Add(new UserInvestment
                            {
                                UserInvestmentsID = reader.GetInt64("userInvestmentsID"),
                                Username = reader.GetString("username"),
                                Ticker = reader.GetString("ticker"),
                                CostPerShare = reader.GetDecimal("CostPerShare"),
                                Shares = reader.GetDouble("shares")
                            });
                        }
                    }

                    stockList = JsonConvert.SerializeObject(stocks);
                }
            }
        
            return stockList;
        }

        public static List<string> GetAllStocks()
        {
            var tickers = new List<string>();

            using (var conn = new MySqlConnection(connstring.ToString()))
            {
                conn.Open();
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT ticker from stocks";

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tickers.Add(reader.GetString("ticker"));
                        }
                    }
                }

            }
            return tickers;
        }

        public static bool UpdateAllStocks(List<DelayedQuote> stocks)
        {
            bool success = false;
            using (var conn = new MySqlConnection(connstring.ToString()))
            {   
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE stocks SET costPerShare = @cost WHERE ticker = @ticker";
                    cmd.Parameters.AddWithValue("@ticker", "");
                    cmd.Parameters.AddWithValue("@cost", 0.0m);

                    conn.Open();
                    foreach (var stock in stocks)
                    {
                        cmd.Parameters["@ticker"].Value = stock.Symbol;
                        cmd.Parameters["@cost"].Value = stock.DelayedPrice;

                        if (cmd.ExecuteNonQuery() > 0)
                            success = true;
                    }
                }
            }

            return success;
        }
    }
}

