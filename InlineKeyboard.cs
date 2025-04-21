using Telegram.Bot.Types.ReplyMarkups;

namespace MyBot
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

    public static InlineKeyboardMarkup BuildLanguageButton(long userID)
    {
        var inlineKeyboard3 = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇺🇸 English", $"eng-{userID}"),
                InlineKeyboardButton.WithCallbackData("🏳️‍⚧️ Russian", $"rus-{userID}")
            }});
        return inlineKeyboard3;
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
