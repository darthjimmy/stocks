using System;

namespace stocks
{
    public class SymbolData
    {
        public string Symbol { get; set; }
        public string Name { get; set; }

        public DateTime Date { get; set; }

        public bool IsEnabled { get; set; }
    }
}