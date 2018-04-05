using System;
using CustomerRecognition.Common;
using CustomerRecognition.Forms.Converters;
using CustomerRecognition.Forms.Models;
using Xamarin.Forms;

namespace CustomerRecognition.Forms
{
    public class OrderDetailPage : ContentPage
    {
        OrderDetailModel orderModel;

        public OrderDetailPage(Order order)
        {
            BindingContext = orderModel = new OrderDetailModel(order);

            Title = "Order " + order.OrderNumber;

            var image = new Image();
            image.SetBinding(Image.SourceProperty, new Binding("ImageUrl", BindingMode.Default, new UrlToImageSourceConverter()));
            image.Aspect = Aspect.AspectFit;
            image.WidthRequest = 200;
            image.HeightRequest = 200;

            //-------------

            var entryOrderStatus = new EntryCell { Label = "Status", Placeholder = "", IsEnabled = false };
            entryOrderStatus.SetBinding(EntryCell.TextProperty, new Binding("OrderStatus"));

            var entryOrderDate = new EntryCell { Label = "Order Date", Placeholder = "", IsEnabled = false };
            entryOrderDate.SetBinding(EntryCell.TextProperty, new Binding("Date"));

            var entryDescription = new EntryCell { Label = "Order", IsEnabled = false };
            entryDescription.SetBinding(EntryCell.TextProperty, new Binding("Description"));

            var entryTotal = new EntryCell { Label = "Total", Placeholder = "", IsEnabled = false };
            entryTotal.SetBinding(EntryCell.TextProperty, new Binding("Total", BindingMode.TwoWay));

            //-------------
            var entryFirstName = new EntryCell { Label = "First Name", Placeholder = "", IsEnabled = false };
            entryFirstName.SetBinding(EntryCell.TextProperty, new Binding("FirstName"));

            var entryLastName = new EntryCell { Label = "Last Name", Placeholder = "", IsEnabled = false };
            entryLastName.SetBinding(EntryCell.TextProperty, new Binding("LastName"));

            var entryGender = new EntryCell { Label = "Gender", Placeholder = "", IsEnabled = false };
            entryGender.SetBinding(EntryCell.TextProperty, new Binding("Gender", BindingMode.Default));

            var entryAge = new EntryCell { Label = "Age", Placeholder = "", IsEnabled = false };
            entryAge.SetBinding(EntryCell.TextProperty, new Binding("Age", BindingMode.Default, stringFormat: "{0:F1}"));

            var entryEmotion = new EntryCell { Label = "Emotion", Placeholder = "", IsEnabled = false };
            entryEmotion.SetBinding(EntryCell.TextProperty, new Binding("Emotion", BindingMode.Default));

            var labelPreviousOrders = new TextCell();
            labelPreviousOrders.Text = "Previous Orders";
            labelPreviousOrders.Tapped += (sender, e) =>
            {
                Console.WriteLine("Navigate to details");
            };

            Content = new TableView
            {
                HasUnevenRows = true,
                Root = new TableRoot {
                    new TableSection("Order"){
                        entryOrderDate,
                        entryOrderStatus,
                        entryDescription,
                        entryTotal
                    },

                    new TableSection("Photo"){
                        new ViewCell() {View = image}
                    },
                    new TableSection("Customer"){
                        entryFirstName,
                        entryLastName,
                        entryGender,
                        entryAge,
                        entryEmotion,
                        labelPreviousOrders
                    }
                },
                Intent = TableIntent.Settings
            };

            this.ToolbarItems.Add(
                new ToolbarItem("Status", null, async () => OnActionSheetSimpleClicked())
            );
        }

        async void OnActionSheetSimpleClicked()
        {
            var type = typeof(OrderStatus);
            var action = await DisplayActionSheet("Change Order Status", "Cancel", null, Enum.GetNames(type));
            if (action == "Cancel")
                return;

            OrderStatus result = (OrderStatus)Enum.Parse(type, action);
            await orderModel.ChangeOrderStatus(result);
        }
    }
}

