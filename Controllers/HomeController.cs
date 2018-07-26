﻿using System;
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
        public bool AddStock(string ticker)
        {
            return false;
        }
        
        [HttpPost]
        public bool AddBalance(double amount, string username)
        {
            return DBHelper.AddBalance(amount, username);
        }
    }
}
