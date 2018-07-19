# stocks
Stock tracker



static private MySqlConnectionStringBuilder connstring = new MySqlConnectionStringBuilder("" +
            "Server=cs4450.cj7o28wmyp47.us-east-2.rds.amazonaws.com;" +
            "UID=erics;" +
            "password=******;" +
"database=Stock");



# Notes

- User
    - userId
    - Username
    - Password (hashed client side)
    - StartingCash

- UserInvestments
    - userId
    - userStocks
    - shares (double)

- Stocks
    - stockId
    - ticker
    - cost per share

- UserStockHistory
    - userStockHistoryId
    - userId
    - stockId
    - Date of change
    - Time of change
    - stock price
    - user shares