using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Telegram.Bot.Types;
using Telegram.Bot;
using SSHUserManagement.Model;
using SSHUserManagement.ORM;
using Newtonsoft.Json;
using Renci.SshNet;
 using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;

namespace SSHUserManagement.BotCommand
{
    public class BotCommandUtility
    {
 
        private ITelegramBotClient _botClient;
        private Telegram.Bot.Types.Update _update;
        private Telegram.Bot.Types.CallbackQuery _callbackQuery;
         public BotCommandUtility(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CallbackQuery callbackQuery)
        {
            _botClient = botClient;
            _update = update;
            _callbackQuery = callbackQuery;
         }
        public async Task AskUsernameForCreateUser(Dictionary<string, object> userData)
        {
            var sshUserName = $"{_update.Message.Text}@SSHUserManagement.COM";
            if (UsersDAL.Get(sshUserName).Id != 0)
                await _botClient.SendTextMessageAsync(
                 chatId: _update.Message.Chat.Id,
                  text: $" UserName Is Exist!"
      );
            else
            {
                userData["sshusername"] = sshUserName;

                await _botClient.SendTextMessageAsync(
               chatId: _update.Message.Chat.Id,
                text: "Please enter a expData with YYYY-MM-DD   for your SSH user:"
               );
                userData["createuser"] = "askexpdata";
            }
        }
        public async Task AskExpDataForCreateUser(Dictionary<string, object> userData)
        {
            var ExpDate = _update.Message.Text;
             if (Regex.IsMatch(ExpDate, @"^[0-9]{4}-[0-9]{2}-[0-9]{2}"))
            {
                userData["ExpDate"] = ExpDate;
                await _botClient.SendTextMessageAsync(

            chatId: _update.Message.Chat.Id,
            text: "Please enter a password for your SSH user:"
                    );
                userData["createuser"] = "askpassword";
            }
            else
            {

                await _botClient.SendTextMessageAsync(

chatId: _update.Message.Chat.Id,
text: "Please enter a Correct Format !!"
        );
            }
        }
        public async Task AskUsernameForInfo(Dictionary<string, object> userData)
        {
            userData["sshusername"] = _callbackQuery.Data;
            var userinfroObj = UsersDAL.Get(userData["sshusername"].ToString());
            var UserInfo = JsonConvert.SerializeObject(userinfroObj);
            userData.Remove("userinfo");
            await _botClient.SendTextMessageAsync(
          chatId: _callbackQuery.From.Id,
          text: UserInfo
          );
            userData.Remove("userinfo");
        }
        public async Task AskUsernameForDelete(Dictionary<string, object> userData)
        {
            userData["sshusername"] = _update.Message.Text;
            userData.Remove("deleteuser");
            var user = UsersDAL.Get(userData["sshusername"].ToString());
            if (user.Id == 0)
            {
                await _botClient.SendTextMessageAsync(
                       chatId: _update.Message.Chat.Id,
                       text: "User not find ! ");
                return;
            }

            string deleteres = await DeleteSSHUser(user);
            DeleteUserFromDB(user.Id);
            await _botClient.SendTextMessageAsync(
            chatId: _update.Message.Chat.Id,
            text: $"User Delete Command with response {deleteres}! ");
            userData.Remove("deleteuser");
        }
        public async Task CreateUser()
        {
            await _botClient.SendTextMessageAsync(chatId: _update.Message.Chat.Id, text: "Welcome to the bot. Please enter a username for your SSH user:");
        }
        public bool AddUserToDB(Users users)
        {
            return UsersDAL.Insert(users);
        }
        public bool DeleteUserFromDB(long id)
        {

            return UsersDAL.Delete(id);
        }

        public async Task<string> ubuntoCmdRunner(string command)
        {
            string res;
            using (var client = new SshClient(AppSetting.SSHServerAddress, AppSetting.SSHUser, AppSetting.SSHPassword))
            {
                // Connect to the server
                client.Connect();

                // Execute the command
                var output = client.RunCommand(command);

                // Check the result
                if (output.ExitStatus == 0)
                {
                    res = $" successfully!";
                }
                else
                {
                    res = output.Error;
                }

                // Disconnect from the server
                client.Disconnect();
            }
            return res;
        }
        public async Task<string> CreateSSHUser(Users users)
        {
            StringBuilder stringBuilder = new StringBuilder();
            // Command to create a new user
            string newUser = $"User_{users.Email}";
            string createUserCommand = $"expect -c 'spawn sudo useradd -m -s /bin/bash {newUser}; expect \"password for\"; send \"{AppSetting.SSHPassword}\\r\"; interact'";
            string SetUserPassCommand = $"echo -e \"{users.Password}\\n{users.Password}\" | sudo -S passwd {newUser}";
            var createduserres = await ubuntoCmdRunner(createUserCommand);
            var setpassworduserres = await ubuntoCmdRunner(SetUserPassCommand);
            stringBuilder.AppendLine("CreatedUser:" + createduserres);
            stringBuilder.AppendLine("SetUserPassCommand:" + setpassworduserres);
            return stringBuilder.ToString();
        }
        public async Task<string> DeleteSSHUser(Users users)
        {
            StringBuilder stringBuilder = new StringBuilder();
            // Command to create a new user
            string User = $"User_{users.Email}";
            string deleteUserCommand = $"deluser {User}";
            var createduserres = await ubuntoCmdRunner(deleteUserCommand);
            return createduserres;
        }
        public async Task<string> SetExpDate(Users users, Dictionary<string, object> userData)
        {

            string newUser = $"User_{users.Email}";
            string ExpDate = userData["ExpDate"].ToString();
            string command = $" echo {AppSetting.SSHPassword} | sudo -S usermod -e {ExpDate} {newUser} ";

            // Create a SSH client
            var SetExpDateres = await ubuntoCmdRunner(command);
            return "SetExpDateres:" + SetExpDateres;
        }

    }
}
