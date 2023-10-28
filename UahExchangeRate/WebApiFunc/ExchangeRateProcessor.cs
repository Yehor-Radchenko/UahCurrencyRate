using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UahExchangeRate.WebApiFunc
{
    public class ExchangeRateProcessor
    {
        private HttpClient _httpClient = new HttpClient();

        public async Task<PrivatBankApiResponse> GetExchangeRatesAsync(DateTime date)
        {
            string apiUrl = $"https://api.privatbank.ua/p24api/exchange_rates?json&date={date.ToString("dd.MM.yyyy")}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                PrivatBankApiResponse apiResponse = JsonSerializer.Deserialize<PrivatBankApiResponse>(responseBody);

                return apiResponse;
            }
            else
            {
                throw new HttpRequestException($"An error occurred while executing the request: {response.StatusCode}");
            }
        }

        public async Task<ExchangeRate> GetExchangeRateToCurrenсyAsync(DateTime date, string currencyCode)
        {
            PrivatBankApiResponse apiResponse = await this.GetExchangeRatesAsync(date);

            foreach (var i in apiResponse.exchangeRate)
            {
                if (i.currency == currencyCode)
                {
                    return i;
                }
            }

            return null;
        }

        public string GetMessageText(List<ExchangeRate> exchangeRates)
        {
            int exchangeRateCount = exchangeRates.Count;
            if (exchangeRateCount <= 0)
            {
                return "No exchange rate data available for the specified date.";
            }

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = exchangeRateCount; i > 1; i--)
            {
                stringBuilder.Append(exchangeRates[i - 1].currency);
                stringBuilder.Append($" | Sale: {exchangeRates[i - 1].saleRateNB}");
                stringBuilder.Append($" | Purchase: {exchangeRates[i - 1].purchaseRateNB}\n\n");
            }

            return stringBuilder.ToString();
        }

        public string GetMessageText(ExchangeRate exchangeRate)
        {
            if (exchangeRate is null)
            {
                return "No exchange rate data available for the specified currency or date.";
            }
            string message = $"{exchangeRate.currency} | Sale: {exchangeRate.saleRateNB} | Purchase: {exchangeRate.purchaseRateNB}";
            return message;
        }
    }
}
