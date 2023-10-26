using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UahExchangeRate.WebApiFunc
{
    public class PrivatBankApiResponse
    {
        public string date { get; set; }
        public string bank { get; set; }
        public int baseCurrency { get; set; }
        public string baseCurrencyLit { get; set; }
        public List<ExchangeRate> exchangeRate { get; set; }
    }
}
