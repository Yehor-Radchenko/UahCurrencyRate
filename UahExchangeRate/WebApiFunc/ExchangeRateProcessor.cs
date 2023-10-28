using System;
using System.Collections.Generic;
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
            using (_httpClient)
            {
                string apiUrl = $"https://api.privatbank.ua/p24api/exchange_rates?json&date={date.ToShortDateString()}";

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
        }

        public string GetMessageText(PrivatBankApiResponse privatBankApiResponse)
        {
            if (privatBankApiResponse is null)
            {
                return "No exchange rate data available for the specified date.";
            }

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = privatBankApiResponse.exchangeRate.Count; i > 1; i--)
            {
                stringBuilder.Append(privatBankApiResponse.exchangeRate[i - 1].currency);
                stringBuilder.Append($" | Sale: {privatBankApiResponse.exchangeRate[i - 1].saleRateNB}");
                stringBuilder.Append($" | Purchase: {privatBankApiResponse.exchangeRate[i - 1].purchaseRateNB}\n\n");
            }

            return stringBuilder.ToString();
        }
    }
}
