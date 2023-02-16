using Microsoft.Extensions.Hosting;
using WTelegram;
using TL;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace TelegramTest
{
    public class TelegramInfoService : BackgroundService
    {
        private Client TelegramClient { get; set; }
        private User TelegramUser { get; set; }
        private readonly Dictionary<long, User> Users = new();
        private readonly Dictionary<long, ChatBase> Chats = new();
        private List<Post> AddedPosts { get; set; } 
        private List<Post> DeletedPosts { get; set; } 

        private readonly IRepository<Post> _repository;
        private readonly ILogger<TelegramInfoService> _logger;

        public TelegramInfoService(IRepository<Post> repository, ILogger<TelegramInfoService> logger)
        {
            _repository = repository;
            AddedPosts = new List<Post>();
            DeletedPosts = new List<Post>();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("The program will display updates received for the logged-in user. Press any key to terminate");
            WTelegram.Helpers.Log = (l, s) => System.Diagnostics.Debug.WriteLine(s);

            //To insert!
            Environment.SetEnvironmentVariable("api_id", "");
            Environment.SetEnvironmentVariable("api_hash", "");
            Environment.SetEnvironmentVariable("phone_number", "");

            using (TelegramClient = new Client(Environment.GetEnvironmentVariable))
            {
                TelegramClient.OnUpdate += Client_OnUpdate;
                TelegramUser = await TelegramClient.LoginUserIfNeeded();
                Users[TelegramUser.id] = TelegramUser;

                Console.WriteLine($"We are logged-in as {TelegramUser.username ?? TelegramUser.first_name + " " + TelegramUser.last_name} (id {TelegramUser.id})");

                var dialogs = await TelegramClient.Messages_GetAllDialogs();
                dialogs.CollectUsersChats(Users, Chats);
                Console.ReadKey();
            }
        }

        private async Task Client_OnUpdate(IObject arg)
        {
            try
            {
                if (arg is not UpdatesBase updates) return;
                updates.CollectUsersChats(Users, Chats);

                ////to get only desired updates
                //var status = updates.UpdateList.Where(x => x is TL.UpdateUserStatus).FirstOrDefault();
                //if (status is not null)
                //{
                //    return;
                //}
                ////

                foreach (var update in updates.UpdateList)
                    switch (update)
                    {
                        case UpdateNewMessage unm: AddNewMessage(unm); break;
                        case UpdateDeleteChannelMessages udcm: DeleteMessage(udcm); break;
                    }

                _repository.Add(AddedPosts);
                //_repository.Add(new List<Post> { new Post { Id = 1, ChannelName = "asdasd", ChannelId = 1, CreatedAt = DateTime.Now, Message = "Asdasd" } });
                _repository.Update(DeletedPosts);
                _repository.SaveChanges();

                AddedPosts.Clear();
                DeletedPosts.Clear();
            }
            catch(Exception e)
            {
                _logger.LogInformation($"The next exception has raised: {e.Message}");
            }

        }

        private void AddNewMessage(UpdateNewMessage newMessageEvent)
        {
            var post = new Post();
            post.Id = newMessageEvent.message.ID;
            var message = newMessageEvent.message;
            post.Message = ((Message)message).message;
            post.ChannelId = ((Message)message).peer_id.ID;
            post.ChannelName = Chats.Where(x => x.Key == post.ChannelId).Select(x => x.Value.Title).FirstOrDefault();
            post.CreatedAt = ((Message)message).date;

            AddedPosts.Add(post);
        }
        private void DeleteMessage(UpdateDeleteChannelMessages deletedMessageEvent)
        {
            var posts =  _repository.GetRange(deletedMessageEvent.messages.ToList());
            
            foreach(var post in posts)
            {
                post.DeletedAt = DateTime.Now;
            }
            DeletedPosts.AddRange(posts);
        }
    }
}
