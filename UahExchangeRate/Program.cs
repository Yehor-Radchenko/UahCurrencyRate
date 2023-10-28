using System;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UahExchangeRate.WebApiFunc;

namespace UahExchangeRate
{
    internal class Program
    {
        private static TelegramBotClient _botClient;
        private static ExchangeRateProcessor _exchangeRateProcessor = new ExchangeRateProcessor();

        static async Task Main(string[] args)
        {
            _botClient = new TelegramBotClient("6532844010:AAGJYlhHq0Iu984pmDVApEy_BltI9sLmqWU");

            using CancellationTokenSource cts = new();

            int offset = 0; 

            while (!cts.Token.IsCancellationRequested)
            {
                var updates = await _botClient.GetUpdatesAsync(offset, cancellationToken: cts.Token);

                foreach (var update in updates)
                {
                    await HandleUpdateAsync(_botClient, update, cts.Token);
                    offset = update.Id + 1;
                }
            }
        }

        async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message || message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            string responseText;

            if (string.IsNullOrEmpty(messageText))
            {
                responseText = "Inputed string is empty.";
                await botClient.SendTextMessageAsync(chatId, responseText, cancellationToken: cancellationToken);
                return;
            }

            var parameters = messageText.Split(' ');

            if (parameters.Length != 1 && parameters.Length != 2) 
            {
                responseText = "You have entered more parameters than required. To issue a list of currencies, enter only the date (dd.MM.yyyy format). To issue information on a specific currency enter the currency code and date. Example: '26.07.2019 USD'";
                await botClient.SendTextMessageAsync(chatId, responseText, cancellationToken: cancellationToken);
                return;
            }

            if (!DateTime.TryParseExact(parameters[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                responseText = "Invalid date format. Use format: dd.MM.yyyy";
                await botClient.SendTextMessageAsync(chatId, responseText, cancellationToken: cancellationToken);
                return;
            }

            if (parameters.Length == 2)
            {
                ExchangeRate exchangeRate = await _exchangeRateProcessor.GetExchangeRateToCurrenсyAsync(date, parameters[1]);
                responseText = _exchangeRateProcessor.GetMessageText(exchangeRate);
                await botClient.SendTextMessageAsync(chatId, responseText, cancellationToken: cancellationToken);
            }
            else
            {
                PrivatBankApiResponse responseModel = await _exchangeRateProcessor.GetExchangeRatesAsync(date);
                responseText = _exchangeRateProcessor.GetMessageText(responseModel.exchangeRate);
                await botClient.SendTextMessageAsync(chatId, responseText, cancellationToken: cancellationToken);
            }
        }

        async static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
        }
    } 
}