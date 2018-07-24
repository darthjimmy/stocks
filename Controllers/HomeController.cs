﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using stocks.Models;

namespace stocks.Controllers
{
    [Route("Home")]
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
        public bool Login(string user, string password, string type)
        {
            bool loggedIn = DBHelper.logger(username, password, type);
            if (loggedIn == true) return true;
            return false;
        }
    }
}
