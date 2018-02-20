using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using WebSocketSharp;

namespace XamarinBotClient
{
    public class BotClient
    {
        static readonly Lazy<BotClient> LazyInstance = new Lazy<BotClient>(() => new BotClient(), true);
        public static BotClient Instance => LazyInstance.Value;

        DirectLineClient _chatClient;
        Conversation _conversation;
        ChannelAccount _channelAccount;
        WebSocket _webSocket;

        readonly ConcurrentQueue<Activity> _outputActivities = new ConcurrentQueue<Activity>();

        public event EventHandler<Exception> Error;
        public event EventHandler<string> MessageSent;
        public event EventHandler<string> MessageRecieved;

        public bool IsConversationStarted => !string.IsNullOrEmpty(_conversation?.ConversationId);

        BotClient(){}

        public async Task<bool> Connect(string directChannelSecret, string userId, string userName, CancellationToken ctx)
        {
            if (string.IsNullOrEmpty(directChannelSecret))
                throw new ArgumentNullException(nameof(directChannelSecret));

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            DisposeConnection();

            _channelAccount = new ChannelAccount(userId, userName);

            _chatClient = new DirectLineClient(directChannelSecret);
            _conversation = await _chatClient.Conversations.StartConversationAsync(ctx);
            if (string.IsNullOrEmpty(_conversation?.ConversationId))
                return false;

            _webSocket = new WebSocket(_conversation.StreamUrl);
            _webSocket.OnMessage += WebSocketOnOnMessage;
            _webSocket.Connect();
            Task.Run(() => StartRunningLoop(ctx));
            return true;
        }

        void DisposeConnection()
        {
            if (_chatClient != null)
            {
                _chatClient.Dispose();
                _chatClient = null;
            }

            if (_webSocket != null)
            {
                _webSocket.OnMessage -= WebSocketOnOnMessage;
                _webSocket.Close();
                _webSocket = null;
            }

            _channelAccount = null;
            _conversation = null;
        }

        void WebSocketOnOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            if (string.IsNullOrEmpty(messageEventArgs.Data))
                return;

            var activitySet = JsonConvert.DeserializeObject<ActivitySet>(messageEventArgs.Data);

            foreach (var activity in activitySet.Activities.Where(a => a.From.Id != _channelAccount.Id))
                MessageRecieved?.Invoke(this, activity.Text);
        }

        async Task StartRunningLoop(CancellationToken ctx)
        {
            while (_chatClient != null && !ctx.IsCancellationRequested)
            {
                try
                {
                    if (_conversation == null || string.IsNullOrEmpty(_conversation?.ConversationId))
                        throw new FieldAccessException(nameof(Conversation.ConversationId));
                         
                    while (!string.IsNullOrEmpty(_channelAccount.Id) && _outputActivities.TryDequeue(out var outputActivity))
                    {
                        var response = await _chatClient.Conversations.PostActivityAsync(_conversation.ConversationId, outputActivity, ctx);
                        MessageSent?.Invoke(this, outputActivity.Text);
                    }
                }
                catch (Exception exception)
                {
                    Error?.Invoke(this, exception);
                }

                await Task.Delay(200, ctx).ConfigureAwait(false);
            }
        }

        public void AddOutputMessage(string messageText)
        {
            var activity= new Activity
            {
                From = _channelAccount,
                Text = messageText,
                Type = ActivityTypes.Message
            };
            _outputActivities.Enqueue(activity);
        }
    }
}
