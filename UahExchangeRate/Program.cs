using System;
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

            if (DateTime.TryParseExact(messageText, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                PrivatBankApiResponse responseModel = await _exchangeRateProcessor.GetExchangeRatesAsync(date);

                string responseText = _exchangeRateProcessor.GetMessageText(responseModel);
                
                await botClient.SendTextMessageAsync(chatId, responseText, cancellationToken: cancellationToken);
            }
            else
            {
                string responseText = "Invalid date format. Use format: dd.MM.yyyy";
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