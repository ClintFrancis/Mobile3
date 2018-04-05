using System;
using CustomerRecognition.Common;
using CustomerRecognition.Forms.Models;
using Xamarin.Forms;
using System.Linq;
using CustomerRecognition.Forms.Converters;

namespace CustomerRecognition.Forms
{
    public class OrdersPage : ContentPage
    {
        IOrdersModel ordersModel;
        static OrdersPage Reference;
        public OrdersPage()
        {
            Reference = this;
            BindingContext = ordersModel = ServiceContainer.Resolve<IOrdersModel>();

            Title = "Orders";

            ListView lstView = new ListView();
            lstView.HasUnevenRows = true;
            lstView.RowHeight = 60;
            lstView.ItemTemplate = new DataTemplate(typeof(CustomOrderCell));
            lstView.IsPullToRefreshEnabled = true;
            lstView.RefreshCommand = ordersModel.RefreshOrdersCommand;
            lstView.ItemsSource = ordersModel.GroupedOrders;
            lstView.IsGroupingEnabled = true;
            lstView.GroupDisplayBinding = new Binding("LongName");
            lstView.SetBinding(ListView.IsRefreshingProperty, new Binding("IsBusy", BindingMode.OneWay));
            lstView.SetBinding(ListView.SelectedItemProperty, new Binding("SelectedItem"));

            // There is currently a bug with UWP Grouped lists
            if (Device.RuntimePlatform != Device.UWP)
            {
                lstView.ItemSelected += async (sender, e) =>
                {
                    var order = (Order)lstView.SelectedItem;
                    await Navigation.PushAsync(new OrderDetailPage(order));
                };
            }

            Content = lstView;

            this.ToolbarItems.Add(
                new ToolbarItem("Refresh", null, () => ordersModel.RefreshOrders())
            );
        }

        protected override void OnAppearing()
        {
            ordersModel.RefreshOrders();
            base.OnAppearing();
        }

        public static async void ChangeCellStatus(MenuItem sender)
        {
            var type = typeof(OrderStatus);
            var order = (Order)sender.CommandParameter;
            var options = Enum.GetNames(type).Where(t => t != order.Status.ToString()).ToArray();
            var action = await Reference.DisplayActionSheet("Change Order Status", "Cancel", null, options);
            if (action == "Cancel")
                return;

            OrderStatus result = (OrderStatus)Enum.Parse(type, action);
            await Reference.ordersModel.ChangeOrderStatus(order, result);
        }

        public class CustomOrderCell : ViewCell
        {
            public CustomOrderCell()
            {
                // Instantiate each of our views
                var labelOrderNumber = new Label();
                labelOrderNumber.FontAttributes = FontAttributes.Bold;
                labelOrderNumber.WidthRequest = 30;
                labelOrderNumber.HorizontalOptions = LayoutOptions.Start;

                var labelOrderDescription = new Label();
                labelOrderDescription.HorizontalOptions = LayoutOptions.Start;

                var labelTimeElapsed = new Label();
                labelTimeElapsed.FontAttributes = FontAttributes.Italic;
                labelTimeElapsed.HorizontalTextAlignment = TextAlignment.End;
                labelTimeElapsed.HorizontalOptions = LayoutOptions.EndAndExpand;

                // Set bindings
                labelOrderNumber.SetBinding(Label.TextProperty, new Binding("OrderNumber", stringFormat: "{0}."));
                labelOrderDescription.SetBinding(Label.TextProperty, new Binding("Description"));
                labelTimeElapsed.SetBinding(Label.TextProperty, new Binding("Date", converter: new DateToElapsedTimeConverter()));

                // Set layout
                var horizontalLayout = new StackLayout() { BackgroundColor = Color.White };
                horizontalLayout.Padding = new Thickness(10);
                horizontalLayout.Orientation = StackOrientation.Horizontal;
                horizontalLayout.VerticalOptions = LayoutOptions.CenterAndExpand;

                // add views to the view hierarchy
                horizontalLayout.Children.Add(labelOrderNumber);
                horizontalLayout.Children.Add(labelOrderDescription);
                horizontalLayout.Children.Add(labelTimeElapsed);

                // add to parent view
                View = horizontalLayout;

                SetActions();
            }

            void SetActions()
            {
                var moreAction = new MenuItem { Text = "Status" };
                moreAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
                moreAction.Clicked += (sender, e) =>
                {
                    var mi = ((MenuItem)sender);
                    ChangeCellStatus(mi);
                };

                // add to the ViewCell's ContextActions property
                ContextActions.Add(moreAction);
            }
        }
    }
}

