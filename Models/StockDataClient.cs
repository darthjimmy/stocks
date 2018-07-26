using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace stocks
{
    public class StockDataClient
    {
        private static string _baseUrl = "https://api.iextrading.com/";
        private static string _apiVer = "1.0";

        private HttpClient client;

        public StockDataClient()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<DelayedQuote> GetDelayedQuote(string ticker)
        {
            string path = $"{_apiVer}/stock/{ticker}/delayed-quote";

            DelayedQuote quote = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                quote = JsonConvert.DeserializeObject<DelayedQuote>(result);
            }

            return quote;
        }

        public async Task<string> GetMostActive()
        {
            string path = $"{_apiVer}/stock/market/list/mostactive";

            var list = "";

            List<TickerData> data = null;

            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                data = JsonConvert.DeserializeObject<List<TickerData>>(result);
            }

            if (data != null)
            {
                foreach (var item in data)
                {
                    list += $"{item.Symbol}<br />";
                }
            }
            return list;
        }
    }
}