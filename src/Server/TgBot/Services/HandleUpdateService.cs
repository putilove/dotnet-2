using System.Text;
using System.Net.Http;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Telegram.Bot.Examples.WebHook.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;

    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task EchoAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Message            => BotOnMessageReceived(update.Message!),
            UpdateType.EditedMessage      => BotOnMessageReceived(update.EditedMessage!),
            _                             => UnknownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    private static int UserNameToInt(string name)
    {
        string alph = "abcdefghijklmnopqrstuvwxyz";
        StringBuilder intString = new();
        for (int i = 0; i < name.Length; i++)
        {
            intString.Append(name[i]^alph[i]);
        }
        return int.Parse(intString.ToString().Substring(0,5));
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation("Receive message type: {messageType}", message.Type);
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "/signIn"   => SendConfirmationCode(_botClient, message),
            "/enable"   => SetEnableMode(_botClient, message),
            "/disable"  => SetDisableMode(_botClient, message),
            "/signOut"  => DeleteUser(_botClient, message),
            _           => Usage(_botClient, message)
        };
        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {sentMessageId}",sentMessage.MessageId);
        async Task<Message> SendConfirmationCode(ITelegramBotClient bot, Message message)
        {

            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            int code = UserNameToInt(message.Chat.Username!);
            string str = $"Your confirmation code: {code}";
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: str);
        }

        async Task<Message> SetEnableMode(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            var client = new HttpClient();
            var response = await client.GetAsync("https://localhost:44349/api/User");
            HttpResponseMessage putResponse;
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("-----The list of users has been received-----");
                List<Server.Model.User> users = JsonConvert.DeserializeObject<List<Server.Model.User>>(await response.Content.ReadAsStringAsync())!;
                if (users.Exists(user => user.Name.Equals(message.Chat.Username)))
                {
                    _logger.LogInformation("-----User found-----");
                    Server.Model.User user = users.Single(user => user.Name == message.Chat.Username);
                    user.Toggle = true;
                    putResponse = await client.PutAsync($"https://localhost:44349/api/User/{user.Id}", new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));
                    if (putResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("-----Success enable-----");
                        string str = $"Success";
                        return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                              text: str);
                    }
                    else
                    {
                        _logger.LogInformation("-----Error enable-----");
                        string str = $"Error {putResponse.StatusCode}";
                        return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                              text: str);
                    }
                }
                else
                {
                    _logger.LogInformation("-----User not found----");
                    string str = "You are not registered";
                    return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                          text: str);
                }
            }
            else
            {
                _logger.LogInformation("-----The list of users hasn't been received-----");
                string str = $"Error {response.StatusCode}";
                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: str);
            }
            
        }

        async Task<Message> SetDisableMode(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            var client = new HttpClient();
            var response = await client.GetAsync("https://localhost:44349/api/User");
            HttpResponseMessage putResponse;
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("-----The list of users has been received-----");
                List<Server.Model.User> users = JsonConvert.DeserializeObject<List<Server.Model.User>>(await response.Content.ReadAsStringAsync())!;
                if (users.Exists(user => user.Name.Equals(message.Chat.Username)))
                {
                    _logger.LogInformation("-----User found-----");
                    Server.Model.User user = users.Single(user => user.Name == message.Chat.Username);
                    user.Toggle = false;
                    putResponse = await client.PutAsync($"https://localhost:44349/api/User/{user.Id}", new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));
                    if (putResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("-----Success enable-----");
                        string str = $"Success";
                        return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                              text: str);
                    }
                    else
                    {
                        _logger.LogInformation("-----Error enable-----");
                        string str = $"Error {putResponse.StatusCode}";
                        return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                              text: str);
                    }
                }
                else
                {
                    _logger.LogInformation("-----User not found----");
                    string str = "You are not registered";
                    return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                          text: str);
                }
            }
            else
            {
                _logger.LogInformation("-----The list of users hasn't been received-----");
                string str = $"Error {response.StatusCode}";
                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: str);
            }

        }

        async Task<Message> DeleteUser(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            var client = new HttpClient();
            var response = await client.GetAsync("https://localhost:44349/api/User");
            HttpResponseMessage deleteResponse;
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("-----The list of users has been received-----");
                List<Server.Model.User> users = JsonConvert.DeserializeObject<List<Server.Model.User>>(await response.Content.ReadAsStringAsync())!;
                if (users.Exists(user => user.Name.Equals(message.Chat.Username)))
                {
                    _logger.LogInformation("-----User found-----");
                    Server.Model.User user = users.Single(user => user.Name == message.Chat.Username);
                    deleteResponse = await client.DeleteAsync($"https://localhost:44349/api/User/{user.Id}");
                    if (deleteResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("-----Success delete-----");
                        string str = $"Success";
                        return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                              text: str);
                    }
                    else
                    {
                        _logger.LogInformation("-----Error delete-----");
                        string str = $"Error {deleteResponse.StatusCode}";
                        return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                              text: str);
                    }
                }
                else
                {
                    _logger.LogInformation("-----User not found----");
                    string str = "You are not registered";
                    return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                          text: str);
                }
            }
            else
            {
                _logger.LogInformation("-----The list of users hasn't been received-----");
                string str = $"Error {response.StatusCode}";
                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: str);
            }
        }


        static async Task<Message> Usage(ITelegramBotClient bot, Message message)
        {
            const string usage = "Usage:\n" +
                                 "/signIn    -  get a confirmation code to log in\n" +
                                 "/enable    -  enable alerts\n" +
                                 "/disable   -  disable alerts\n" +
                                 "/signOut   -  delete user";
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: usage,
                                                  replyMarkup: new ReplyKeyboardRemove());
        }
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {updateType}", update.Type);
        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(Exception exception)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }
}
