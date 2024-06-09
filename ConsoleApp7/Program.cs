
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConsoleApp7
{
    class Program
    {
        // Замените на ID чата разработчиков
        private static readonly long DevelopersChatId = -4227593901;

        static void Main(string[] args)
        {
            var client = new TelegramBotClient("7377075467:AAFGc1EjScNueDYA1p-93s5PkV18TzcbaUM");
            var cancellationTokenSource = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            client.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationTokenSource.Token
            );

            Console.WriteLine("Бот запущен.");
            Console.ReadKey();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessage(botClient, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQuery(botClient, update.CallbackQuery);
            }
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private static async Task HandleMessage(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.ToLower().Contains("/start"))
            {
                InlineKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Оставить Bug Report", callbackData: "make_bugreport"),
                    },
                });

                await botClient.SendTextMessageAsync(message.Chat.Id, "Если вы столкнулись с ошибкой работы программы, прошу выберите кнопку заполнения bug report'а", replyMarkup: replyKeyboardMarkup);
                return;
            }

            if (message.ReplyToMessage != null && message.ReplyToMessage.Text.StartsWith("Вы выбрали:"))
            {
                string severity = message.ReplyToMessage.Text.Split(':')[1].Trim();
                string bugReport = $"Пользователь: @{message.From.Username}\nСерьезность: {severity}\nСообщение: {message.Text}\nДата: {message.Date}";

                await botClient.SendTextMessageAsync(DevelopersChatId, bugReport);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Bug report был отправлен разработчикам.");
            }
        }

     
        private static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == "make_bugreport")
            {
                InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Trivial", callbackData: "severity_trivial"),
                        InlineKeyboardButton.WithCallbackData(text: "Minor", callbackData: "severity_minor"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Major", callbackData: "severity_major"),
                        InlineKeyboardButton.WithCallbackData(text: "Critical", callbackData: "severity_critical"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Blocker", callbackData: "severity_blocker"),
                    },
                });

                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Выберите классификацию ошибки:", replyMarkup: inlineKeyboard);
            }
            else if (callbackQuery.Data.StartsWith("severity_"))
            {
                string severity = callbackQuery.Data.Split('_')[1];
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Вы выбрали: {severity}. Пожалуйста, опишите ошибку:", replyMarkup: new ForceReplyMarkup { Selective = true });
            }
        }
    }
}