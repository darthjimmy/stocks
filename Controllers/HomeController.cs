using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using stocks.Models;

namespace stocks.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public bool Login(string username, string password, string type)
        {
            return DBHelper.logger(username, password, type);
        }

        [HttpPost]
        public string AddStock(string ticker, string username)
        {
            StockDataClient client = new StockDataClient();

            var result = client.GetDelayedQuote(ticker).Result;

            if (result != null)
            {
                if (DBHelper.PurchaseStock(username, ticker, 0))
                    return $"{result.Symbol}, {result.DelayedPrice}";
                else
                    return "false";
            }

            return "false";
        }

        [HttpPost]
        public decimal GetPrice(string ticker)
        {
            StockDataClient client = new StockDataClient();
            return (client.GetDelayedQuote(ticker).Result)?.DelayedPrice ?? 0.0m;
        }
        
        [HttpPost]
        public bool PurchaseStock(string username, string ticker, int shares)
        {
            return DBHelper.PurchaseStock(username, ticker, shares);
        }

        [HttpPost]
        public string MostActive()
        {
            StockDataClient client = new StockDataClient();
            return (client.GetMostActive().Result);
        }
        
        [HttpPost]
        public bool AddBalance(decimal amount, string username)
        {
            return DBHelper.AddBalance(amount, username);
        }
        
        [HttpPost]
        public bool DoStocks(string username)
        {
            return DBHelper.DoStocks(username);
        }
        
        [HttpPost]
        public decimal GetBalance(string username)
        {
            return DBHelper.GetBalance(username);
        }
    }
}
