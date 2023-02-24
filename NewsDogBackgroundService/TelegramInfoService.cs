using Microsoft.Extensions.Hosting;
using WTelegram;
using TL;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NewsDogBackgroundService;

namespace TelegramTest
{
    public class TelegramInfoService : BackgroundService
    {
        private Client _telegramClient { get; set; }
        private User _telegramUser { get; set; }
        private readonly Dictionary<long, User> _users;
        private readonly Dictionary<long, ChatBase> _chats;
        private List<Post> _addedPosts { get; set; } 
        private List<Post> _deletedPosts { get; set; }

        private readonly Dictionary<long, SourceGroup> _channelsGroups;
        private readonly IRepository<Post> _repository;
        private readonly ILogger<TelegramInfoService> _logger;
        private readonly IConfiguration _config;

        public TelegramInfoService(IRepository<Post> repository, ILogger<TelegramInfoService> logger, IConfiguration config)
        {
            _repository = repository;
            _addedPosts = new();
            _deletedPosts = new();
            _users = new();
            _chats = new();
            _logger = logger;
            _config = config;
            _channelsGroups =  _config.GetChannelsGroupsDictionary();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("The program will display updates received for the logged-in user. Press any key to terminate");
            WTelegram.Helpers.Log = (l, s) => System.Diagnostics.Debug.WriteLine(s);

            using (_telegramClient = new Client(_config.GetValue<string>))
            {
                _telegramClient.OnUpdate += Client_OnUpdate;
                _telegramUser = await _telegramClient.LoginUserIfNeeded();
                _users[_telegramUser.id] = _telegramUser;

                Console.WriteLine($"We are logged-in as {_telegramUser.username ?? _telegramUser.first_name + " " + _telegramUser.last_name} (id {_telegramUser.id})");

                var dialogs = await _telegramClient.Messages_GetAllDialogs();
                dialogs.CollectUsersChats(_users, _chats);
                Console.ReadKey();
            }
        }

        private async Task Client_OnUpdate(IObject arg)
        {
            try
            {
                if (arg is not UpdatesBase updates)
                {
                    return;
                }

                updates.CollectUsersChats(_users, _chats);

                foreach (var update in updates.UpdateList)
                    switch (update)
                    {
                        case UpdateNewMessage unm: AddNewMessage(unm); break;
                        case UpdateDeleteChannelMessages udcm: DeleteMessage(udcm); break;
                    }

                _repository.Add(_addedPosts);
                _repository.Update(_deletedPosts);
                _repository.SaveChanges();

                _addedPosts.Clear();
                _deletedPosts.Clear();
            }
            catch(Exception e)
            {
                _logger.LogInformation($"The next exception has raised: {e.Message}");
            }

        }

        private void AddNewMessage(UpdateNewMessage newMessageEvent)
        {
            var messageInfo = (Message)newMessageEvent.message;
            var post = new Post();

            post.Id = newMessageEvent.message.ID;
            post.Message = messageInfo.message;
            post.ChannelId = messageInfo.peer_id.ID;
            post.ChannelName = _chats.Where(x => x.Key == post.ChannelId).Select(x => x.Value.Title).FirstOrDefault();
            post.CreatedAt = DateTime.Now;
            post.Group = GetGroupForChannel(post.ChannelId);
            post.HasMedia = messageInfo.flags.HasFlag(Message.Flags.has_media);

            _addedPosts.Add(post);
        }
        private void DeleteMessage(UpdateDeleteChannelMessages deletedMessageEvent)
        {
            var posts =  _repository.GetRange(deletedMessageEvent.messages.ToList());
            foreach (var post in posts)
            {
                post.DeletedAt = DateTime.Now;
            }
            _deletedPosts.AddRange(posts);
        }

        private SourceGroup GetGroupForChannel(long channelId) =>
            _channelsGroups.Where(x => x.Key == channelId).Select(y => y.Value).FirstOrDefault();

    }
}

