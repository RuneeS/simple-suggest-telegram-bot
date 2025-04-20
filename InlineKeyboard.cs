using Telegram.Bot.Types.ReplyMarkups;

namespace Goobot
{
public static class InlineKeyboard
{
    public static InlineKeyboardMarkup BuildAdminButton(long userID)
    {
                        //Inline keyboard   
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📩", $"reply-{userID}"),
                InlineKeyboardButton.WithCallbackData($"🤡", $"ban-{userID}")
            }});
        return inlineKeyboard;

    }

    public static InlineKeyboardMarkup BuildUnbanButton(long userID)
    {
        var inlineKeyboard2 = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🍆 Unban", $"unban-{userID}")
            }});
        return inlineKeyboard2;
    }
}
}