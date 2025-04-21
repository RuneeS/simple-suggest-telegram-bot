using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

using static MyBot.InlineKeyboard;
using static MyBot.Language;

namespace MyBot
{

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;

    static async Task Main()
    {
        _botClient = new TelegramBotClient("TOKEN");
        
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            ThrowPendingUpdates = false
        };
        
        Database.Initialize();

        using var cts = new CancellationTokenSource();

        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"{me.Username} is working now!");

        

        await Task.Delay(-1);
    }

    public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            //Language database
            var userRepo = new UserSettingsRepository();

            //Admin ID
            long adminId = //ID;

            //Text message handler
             if (update.Type == UpdateType.Message && update.Message != null)
            {
                //User info
                long userID = update.Message.From.Id;
                string userName = $"{update.Message.From.FirstName} {update.Message.From.LastName}".Trim();
                string text = update.Message.Text;
                string clickableName = $"<a href=\"tg://user?id={userID}\">{userName}</a>";

                //Language getting
                string langCode = userRepo.GetLanguage(userID);
                Language lang = langCode == "ru" ? Language.Russian : Language.English;



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

                    //Command handler
                    if (command == "/language")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Choose your language / Выберите ваш язык", 
                                                             replyMarkup: BuildLanguageButton(userID));
                    }

                    //Only text message handler
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId: adminId,
                            text: $"{lang["new_message"]} <a href=\"tg://user?id={userID}\">{userName}</a>:\n \n {text}",
                            parseMode: ParseMode.Html,
                            replyMarkup: BuildAdminButton(userID));

                        await botClient.SendTextMessageAsync(message.Chat.Id, lang["message_sent"]);
                        Console.WriteLine($"📨 Message from {userName} (ID: {userID}): {text}");
                    }
                }

                else if (message.Photo != null && message.Photo.Any())
                {
                    var largestPhoto = message.Photo.Last();
                    string caption = message.Caption ?? "";

                    await botClient.SendPhotoAsync(chatId: adminId,
                        photo: new InputFileId(largestPhoto.FileId),
                        caption: $"{lang["new_message"]} <a href=\"tg://user?id={userID}\">{userName}</a>:\n \n{caption}",
                        parseMode: ParseMode.Html,
                        replyMarkup: BuildAdminButton(userID));

                    await botClient.SendTextMessageAsync(message.Chat.Id, lang["photo_sent"]);
                }
                
                else if(message.Video != null)
                {
                    var video = message.Video;
                    string caption = message.Caption ?? "";

                    await botClient.SendVideoAsync(chatId: adminId, 
                        video: new InputFileId(video.FileId), 
                        caption: $"{lang["new_message"]} <a href=\"tg://user?id={userID}\">{userName}</a>:\n \n{caption}",
                        parseMode: ParseMode.Html,
                        replyMarkup: BuildAdminButton(userID));

                    await botClient.SendTextMessageAsync(message.Chat.Id, lang["video_sent"]);
                }

            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                var callback = update.CallbackQuery;
                string data = callback.Data;

                var splitData = data.Split('-');
                string action = splitData[0]; // reply или ban
                long telegramUserID = long.Parse(splitData[1]);

                //Language getting
                string langCode = userRepo.GetLanguage(telegramUserID);
                Language lang = langCode == "ru" ? Language.Russian : Language.English;

                if (action == "reply")
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId: callback.Id,
                                                             text: lang["not_aviable"],
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
                        Database.AddBannedUser(telegramUserID.ToString());
                        await botClient.AnswerCallbackQueryAsync(callbackQueryId: callback.Id,
                                                             text: "🤡 The user has banned.",
                                                             showAlert: true);
                        await botClient.SendTextMessageAsync(chatId: adminId,
                            text: $"🤡 {telegramUserID} {lang["user_banned"]}",
                            parseMode: ParseMode.Html,
                            replyMarkup: BuildUnbanButton(telegramUserID));                  

                        await botClient.SendTextMessageAsync(chatId: telegramUserID, text: lang["you_banned"]);
                    }
                }
                else if (action == "unban")
                {
                    Database.RemoveBannedUser(telegramUserID.ToString());
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId: callback.Id,
                                                         text: "🎉 The user has ubanned.",
                                                         showAlert: true);
                    await botClient.SendTextMessageAsync(chatId: adminId,
                        text: $"🎉 {telegramUserID} {lang["user_unbanned"]}",
                        parseMode: ParseMode.Html);             

                        await botClient.SendTextMessageAsync(chatId: telegramUserID, text: lang["you_unbanned"]);     

                }

                //Languages
                else if (action == "rus")
                {
                    userRepo.SetLanguage(telegramUserID, "ru");

                    await botClient.SendTextMessageAsync(chatId: telegramUserID, text: "✅ Язык успешно сменён!");
                }
                else if (action == "eng")
                {
                    userRepo.SetLanguage(telegramUserID, "eng");

                    await botClient.SendTextMessageAsync(chatId: telegramUserID, text: "✅ Language has succesfully switched!");
                }

                await botClient.AnswerCallbackQueryAsync(callback.Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var errorMessage = error switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
}
