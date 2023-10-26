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
        public async Task<PrivatBankApiResponse> GetExchangeRatesAsync(DateTime date)
        {
            using (var httpClient = new HttpClient())
            {
                string apiUrl = $"https://api.privatbank.ua/p24api/exchange_rates?json&date={date.ToShortDateString()}";

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

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
