using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

using static Goobot.InlineKeyboard;

namespace Goobot
{

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;

    static async Task Main()
    {
        _botClient = new TelegramBotClient("YOUR_TOKEN_HERE");
        
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            ThrowPendingUpdates = false
        };
        
        Database.Initialize();

        using var cts = new CancellationTokenSource();

        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"{me.Username} запущен!");

        

        await Task.Delay(-1);
    }

    public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            //Admin ID
            long adminId = 1306218745;

            //Text message handler
             if (update.Type == UpdateType.Message && update.Message != null)
            {
                //User info
                long userID = update.Message.From.Id;
                string userName = $"{update.Message.From.FirstName} {update.Message.From.LastName}".Trim();
                string text = update.Message.Text;
                string clickableName = $"<a href=\"tg://user?id={userID}\">{userName}</a>";

                if (Database.IsUserBanned(userID.ToString()))
                {
                    await botClient.SendStickerAsync(
                    chatId: update.Message.Chat.Id,
                    sticker: new InputFileId("CAACAgIAAxkBAAIBh2gDV44m7xSuwNzzFIRIzUbWiLidAAKYYgACiGngSYM4QQN_zIvMNgQ"));
                    return;
                }

                //Message text
                var message = update.Message;

                if (!string.IsNullOrEmpty(message.Text))
                {
                    string command = message.Text.Trim().ToLower();

                    //Command handler
                    if (command == "/start")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "✅");
                    }

                    //Only text message handler
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId: adminId,
                            text: $"📨 Message from <a href=\"tg://user?id={userID}\">{userName}</a>:\n \n {text}",
                            parseMode: ParseMode.Html,
                            replyMarkup: BuildAdminButton(userID));

                        await botClient.SendTextMessageAsync(message.Chat.Id, "📨 Message has sent!");
                        Console.WriteLine($"📨 Message from {userName} (ID: {userID}): {text}");
                    }
                }

                //Other types of messages
                else if (message.Photo != null && message.Photo.Any())
                {
                    var largestPhoto = message.Photo.Last();
                    string caption = message.Caption ?? "";

                    await botClient.SendPhotoAsync(chatId: adminId,
                        photo: new InputFileId(largestPhoto.FileId),
                        caption: $"📨 Message from <a href=\"tg://user?id={userID}\">{userName}</a>:\n \n{caption}",
                        parseMode: ParseMode.Html,
                        replyMarkup: BuildAdminButton(userID));

                    await botClient.SendTextMessageAsync(message.Chat.Id, "📸 Photo has been sent!");
                }

            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                var callback = update.CallbackQuery;
                string data = callback.Data;


                var splitData = data.Split('-');
                string action = splitData[0]; // reply или ban
                long telegramUserID = long.Parse(splitData[1]);

                if (action == "reply")
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId: callback.Id,
                                                             text: "😭 This feature is not available yet",
                                                             showAlert: true);
                }
                else if (action == "ban")
                {
                    if (Database.IsUserBanned(telegramUserID.ToString()))
                    {
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId: callback.Id,
                                                             text: "🤡 The user has already banned.",
                                                             showAlert: true);
                    }
                    else
                    {
                        //message to admin
                        Database.AddBannedUser(telegramUserID.ToString());
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId: callback.Id,
                                                             text: "🤡 The user has banned.",
                                                             showAlert: true);
                        await botClient.SendTextMessageAsync(chatId: adminId,
                            text: $"🤡 {telegramUserID} is banned.",
                            parseMode: ParseMode.Html,
                            replyMarkup: BuildUnbanButton(telegramUserID));                  

                        //message to user
                        await botClient.SendTextMessageAsync(chatId: telegramUserID, text: "🤡 You have banned");
                    }
                }
                else if (action == "unban")
                {
                    Database.RemoveBannedUser(telegramUserID.ToString());
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId: callback.Id,
                                                         text: "🎉 The user has ubanned.",
                                                         showAlert: true);
                    await botClient.SendTextMessageAsync(chatId: adminId,
                        text: $"🎉 {telegramUserID} is ubanned now.",
                        parseMode: ParseMode.Html);             

                        await botClient.SendTextMessageAsync(chatId: telegramUserID, text: "🎉 You're unbanned now");     

                }
                await botClient.AnswerCallbackQueryAsync(callback.Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    // Обработчик ошибок
    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        // Формируем сообщение об ошибке
        var errorMessage = error switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        // Выводим ошибку в консоль
        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
}