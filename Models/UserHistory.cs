using System.Collections.Generic;
using System;

namespace stocks
{
    public class UserHistory
    {
        public string ticker {get; set;}
        public DateTime dateOfChange { get; set; }
        public decimal CostPerShare {get; set;}
        public double userShares {get; set;}
    }

}
