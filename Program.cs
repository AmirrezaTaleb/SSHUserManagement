using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using SSHUserManagement.BotCommand;
using SSHUserManagement.Model;
using SSHUserManagement.ORM;
using Update = Telegram.Bot.Types.Update;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

 
JObject configJson = JObject.Parse(File.ReadAllText($"appsettings.json"));
   JsonConvert.DeserializeObject<AppSetting>(configJson.ToString());


var botClient = new TelegramBotClient(AppSetting.ApiKeyBot);
using CancellationTokenSource cts = new();
ReceiverOptions receiverOptions = new()
{

    AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery } // receive all update types except ChatMember related updates
};
Dictionary<long, Dictionary<string, object>> userDataDictionary = new();
botClient.StartReceiving(
updateHandler: HandleUpdateAsync,
pollingErrorHandler: HandlePollingErrorAsync,
receiverOptions: receiverOptions,
 cancellationToken: cts.Token
 );
var me = await botClient.GetMeAsync();
Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();
cts.Cancel();


async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken  )
{

    if (update.Message is not { } && update.CallbackQuery is not { })
        return;
    if (update.Message?.Text is not { } && update.CallbackQuery.Data is not { })
        return;
    var message = update.Message;
    var callbackQuery = update.CallbackQuery;
    var messageText = message?.Text ?? update.CallbackQuery.Data;
    var chatId = message?.Chat?.Id ?? callbackQuery.From.Id;
    var userName = message?.Chat?.FirstName ?? callbackQuery.From.FirstName;
    var userId = message?.From?.Username ?? callbackQuery.From.Username;

    if (!userDataDictionary.ContainsKey(chatId))
    {
        userDataDictionary.Add(chatId, new Dictionary<string, object>());
    }


    BotCommandUtility botCommand = new BotCommandUtility(botClient, update, callbackQuery);
    var userData = userDataDictionary[chatId];



    switch (messageText)
    {
        case "/create":
            botCommand.CreateUser();
            userData["createuser"] = "askusername";
            break;
        case "/start":
            await botClient.SendTextMessageAsync(
           chatId: chatId,
           text: "Welcome to the bot. Please choose a option:"
           );
            break;
        case "/delete":
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Please enter a username  for delete:"
            );
            userData["deleteuser"] = "askusernamefordelete";
            break;
        case "/userinfo":

            var users = UsersDAL.GetAll();
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            foreach (var item in users)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData(text: $"{item.Email} ", callbackData: item.Email));
            }
            List<List<InlineKeyboardButton>> sublists = new List<List<InlineKeyboardButton>>();
            int counted = 0;
            for (int i = 0; i < buttons.Count; i += 2)
            {
                if (buttons.Count - counted > 1)
                {
                    sublists.Add(buttons.GetRange(i, 2));
                    counted += 2;
                    continue;
                }
                if (buttons.Count - counted == 1)
                {
                    sublists.Add(buttons.GetRange(i, 1));
                    counted += 1;
                    continue;
                }
            }
            int n = 2; // the chunk size
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(sublists);



            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Please enter a username  for more Info:",
             replyMarkup: inlineKeyboard
            );
            userData["userinfo"] = "askusernameforInfo";
            break;
        default:

            if (userData.ContainsKey("createuser"))
            {
                var privCmd = (string)userData["createuser"];
                switch (privCmd)
                {
                    case "askusername":
                        botCommand.AskUsernameForCreateUser(userData);
                        break;
                    case "askexpdata":
                        botCommand.AskExpDataForCreateUser(userData);
                        break;
                    case "askpassword":

                        var user = new Users { IsDeleted = false, ChatId = chatId, Email = (string)userData["sshusername"], ExpDate = (string)userData["ExpDate"], Name = userName, TelegramId = userId, Password = message.Text };
                        botCommand.AddUserToDB(user);
                        string Createdres = await botCommand.CreateSSHUser(user);
                        string SetExpDateRes = await botCommand.SetExpDate(user, userData);
                        var sshPassword = message.Text;
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                                   text: $"Hello, {message.Chat.FirstName}! I have created a SSH user with the username User_{(string)userData["sshusername"]} and the password {sshPassword}. \n {Createdres} \n {SetExpDateRes} "
                            );



                        userData.Remove("createuser");
                        userData.Remove("sshusername");
                        break;
                }
            }
            else if (userData.ContainsKey("userinfo"))
            {
                var privCmd = (string)userData["userinfo"];
                switch (privCmd)
                {
                    case "askusernameforInfo":
                        await botCommand.AskUsernameForInfo(userData);
                        break;
                }
            }
            else if (userData.ContainsKey("deleteuser"))
            {
                var privCmd = (string)userData["deleteuser"];
                switch (privCmd)
                {
                    case "askusernamefordelete":
                        await botCommand.AskUsernameForDelete(userData);
                        break;
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Invalid command. Please try again."
                );
            }
            break;
    }


}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

