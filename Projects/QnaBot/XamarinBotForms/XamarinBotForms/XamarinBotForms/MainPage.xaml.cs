using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinBotClient;

namespace XamarinBotForms
{
    public partial class MainPage : ContentPage
    {
        ObservableCollection<ChatMessage> Messages { get; set; }

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagesListView.Header = new ContentView { MinimumHeightRequest = HeightRequest = MessagesListView.Height, BackgroundColor = Color.Yellow };
        }

        async Task StartBotSession()
        {
            var tokenSource = new CancellationTokenSource();

            try
            {
                var directChannelKey = "pApABSjzvLg.cwA.IGI.b8amfqZNWNDOupra5z0lbOz_9XMHkS7lO6w9D1m3Tns";

                var userId = Guid.NewGuid().ToString();
                var userName = "Xamarin Bot";

                var connectResult = await BotClient.Instance.Connect(directChannelKey, userId, userName, tokenSource.Token);
                if (connectResult)
                {
                    BotClient.Instance.Error += BotClientOnError;
                    BotClient.Instance.MessageRecieved += BotClientOnMessageRecieved;

                    Messages = new ObservableCollection<ChatMessage>
                    {
                        new ChatMessage("Welcome to Xamarin Bot Chat! How can I help you?", ChatMessageType.System)
                    };

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagesListView.ItemsSource = Messages;
                        SetConversationMode();
                    });
                }
                else
                    Device.BeginInvokeOnMainThread(SetConnectMode);
            }
            catch (Exception exception)
            {
                tokenSource.Cancel(false);
                Messages.Add(new ChatMessage($"Error: {exception.Message}", ChatMessageType.System));
            }
        }

        void BotClientOnMessageRecieved(object sender, string inputMessage)
        {
            var message = new ChatMessage(inputMessage, ChatMessageType.Input);
            Messages?.Add(message);
        }

        async void BotClientOnError(object sender, Exception exception)
        {
            SetConnectMode();

            Messages.Add(new ChatMessage($"Error: {exception.Message}", ChatMessageType.System));

            if (await DisplayAlert("Error", exception.Message, "Retry", "Cancel"))
                Task.Run(StartBotSession);
        }

        void SendButtonOnClicked(object sender, EventArgs e)
        {
            if (BotClient.Instance.IsConversationStarted)
            {
                if (string.IsNullOrEmpty(OutputMessageEntry.Text))
                    return;

                var message = new ChatMessage(OutputMessageEntry.Text, ChatMessageType.Output);
                Messages.Add(message);
                BotClient.Instance.AddOutputMessage(message.Text);
                OutputMessageEntry.Text = string.Empty;
                MessagesListView.ScrollTo(message, ScrollToPosition.End, true);
            }
            else
            {
                SetConnectingMode();
                Task.Run(StartBotSession);
            }
        }

        void SetConnectMode()
        {
            SendButton.Text = "Connect";
            SendButton.IsEnabled = true;
        }

        void SetConversationMode()
        {
            SendButton.Text = "Send";
            SendButton.IsEnabled = true;

            OutputMessageEntry.Text = string.Empty;
            OutputMessageEntry.IsEnabled = true;
        }

        void SetConnectingMode()
        {
            SendButton.Text = "Connecting";
            SendButton.IsEnabled = false;
        }
    }

    public class ChatMessage
    {
        public string Text { get; }
        public ChatMessageType Type { get; }

        public ChatMessage(string text, ChatMessageType type)
        {
            Text = text;
            Type = type;
        }
    }

    public enum ChatMessageType
    {
        None,
        Input,
        Output,
        System
    }

    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InputMessageTemplate { get; set; }
        public DataTemplate OutputMessageTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is ChatMessage chatMessage)
                return chatMessage.Type == ChatMessageType.Output ? OutputMessageTemplate : InputMessageTemplate;

            return null;
        }
    }
}
