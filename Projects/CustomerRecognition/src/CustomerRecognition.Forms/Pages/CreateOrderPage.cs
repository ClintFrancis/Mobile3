using System;
using System.Threading.Tasks;
using CustomerRecognition.Forms.Converters;
using CustomerRecognition.Forms.Models;
using Xamarin.Forms;

namespace CustomerRecognition.Forms
{
    public class CreateOrderPage : ContentPage
    {
        ICreateOrderModel createOrderModel;
        EntryCell entryDescription;
        EntryCell entryFirstName;
        EntryCell entryTotal;

        public CreateOrderPage()
        {
            BindingContext = createOrderModel = ServiceContainer.Resolve<ICreateOrderModel>();

            var image = new Image();
            image.SetBinding(Image.SourceProperty, new Binding("ImageUri", BindingMode.Default));
            image.Aspect = Aspect.AspectFit;
            image.WidthRequest = 200;
            image.HeightRequest = 200;

            entryFirstName = new EntryCell { Label = "First Name", Placeholder = "" };
            entryFirstName.SetBinding(EntryCell.TextProperty, new Binding("FirstName"));

            var entryLastName = new EntryCell { Label = "Last Name", Placeholder = "" };
            entryLastName.SetBinding(EntryCell.TextProperty, new Binding("LastName"));

            var entryGender = new EntryCell { Label = "Gender", Placeholder = "", IsEnabled = false };
            entryGender.SetBinding(EntryCell.TextProperty, new Binding("Gender", BindingMode.Default));

            var entryAge = new EntryCell { Label = "Age", Placeholder = "", IsEnabled = false };
            entryAge.SetBinding(EntryCell.TextProperty, new Binding("Age", BindingMode.Default, stringFormat: "{0:F1}"));

            var entryEmotion = new EntryCell { Label = "Emotion", Placeholder = "", IsEnabled = false };
            entryEmotion.SetBinding(EntryCell.TextProperty, new Binding("Emotion", BindingMode.Default));

            entryDescription = new EntryCell { Label = "Order", Placeholder = "Customer order" };
            entryDescription.SetBinding(EntryCell.TextProperty, new Binding("CurrentOrderDescription", BindingMode.TwoWay));

            entryTotal = new EntryCell { Label = "Total", Placeholder = "0", Keyboard = Keyboard.Numeric };
            entryTotal.SetBinding(EntryCell.TextProperty, new Binding("Total", BindingMode.TwoWay));

            var entryPreviousDate = new EntryCell { Label = "Previous Date", Placeholder = "", IsEnabled = false };
            entryPreviousDate.SetBinding(EntryCell.TextProperty, new Binding("PreviousOrderDate", BindingMode.Default, stringFormat: "{0:MMMM dd, yyyy}"));

            var entryPreviousDescription = new EntryCell { Label = "Previous Order", Placeholder = "", IsEnabled = false };
            entryPreviousDescription.SetBinding(EntryCell.TextProperty, new Binding("PreviousOrderDescription", BindingMode.Default));

            Content = new TableView
            {
                HasUnevenRows = true,
                Root = new TableRoot {
                    new TableSection("Photo"){
                        new ViewCell() {View = image}
                    },
                    new TableSection("Customer"){
                        entryFirstName,
                        entryLastName,
                        entryGender,
                        entryAge,
                        entryEmotion
                    },
                    new TableSection("Order"){
                        entryDescription,
                        entryTotal
                    },
                    new TableSection("Previous Order"){
                        entryPreviousDate,
                        entryPreviousDescription
                    }
                },
                Intent = TableIntent.Settings
            };

            this.ToolbarItems.Add(
                new ToolbarItem("Submit", null, async () => SubmitOrder())
            );
        }

        async Task SubmitOrder()
        {
            string errorMessage = "";
            if (string.IsNullOrEmpty(entryFirstName.Text))
                errorMessage += "Please supply a first name.\n";

            if (string.IsNullOrEmpty(entryDescription.Text))
                errorMessage += "Please supply an order.\n";

            if (double.Parse(entryTotal.Text) == 0)
                errorMessage += "Please supply a valid total.";

            if (!string.IsNullOrEmpty(errorMessage))
            {
                await DisplayAlert("Missing Fields", errorMessage, "OK");
                return;
            }

            var result = await createOrderModel.SubmitNewOrder();
            if (!string.IsNullOrEmpty(result))
            {
                var parts = result.Split(';');
                await DisplayAlert(parts[0], parts[1], "Ok");
                await Navigation.PopAsync();
            }
        }

        protected override void OnDisappearing()
        {
            createOrderModel.Reset();
            base.OnDisappearing();
        }
    }
}

