namespace stocks
{
    public class UserInvestment
    {
        public long UserInvestmentsID {get; set;}
        public string Username { get; set; }
        public string Ticker { get; set; }
        public decimal CostPerShare {get; set;}
        public double Shares {get; set;}
    }
}