using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CustomerRecognition.Common;
using CustomerRecognition.Common.Message;
using CustomerRecognition.Forms.Services;
using Xamarin.Forms;

namespace CustomerRecognition.Forms.Models
{
    public interface ICreateOrderModel
    {
        string FirstName { get; }
        string LastName { get; }
        double Age { get; }
        string Emotion { get; }
        string Gender { get; }
        bool IsExistingCustomer { get; }
        string PreviousOrderDate { get; }
        string PreviousOrderDescription { get; }
        string CurrentOrderDescription { get; set; }
        byte[] ImageData { get; }
        Uri ImageUri { get; }

        bool IsBusy { get; }

        Task<bool> IdentifyCustomer(byte[] imageData);
        Task<string> SubmitNewOrder();
        void Reset();
    }

    public class CreateOrderModel : INotifyPropertyChanged, ICreateOrderModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Customer customerData;

        string firstName;
        public string FirstName
        {
            get { return firstName; }
            set
            {
                if (firstName != value)
                {
                    firstName = value;
                    OnPropertyChanged("FirstName");
                }
            }
        }

        string lastName;
        public string LastName
        {
            get { return lastName; }
            set
            {
                if (lastName != value)
                {
                    lastName = value;
                    OnPropertyChanged("LastName");
                }
            }
        }

        double age;
        public double Age
        {
            get { return age; }
            protected set
            {
                if (age != value)
                {
                    age = value;
                    OnPropertyChanged("Age");
                }
            }
        }

        double total;
        public double Total
        {
            get { return total; }
            set
            {
                if (total != value)
                {
                    total = value;
                    OnPropertyChanged("Total");
                }
            }
        }

        string gender;
        public string Gender
        {
            get { return gender; }
            protected set
            {
                if (gender != value)
                {
                    gender = value;
                    OnPropertyChanged("Gender");
                }
            }
        }

        string emotion;
        public string Emotion
        {
            get { return emotion; }
            protected set
            {
                if (emotion != value)
                {
                    emotion = value;
                    OnPropertyChanged("Emotion");
                }
            }
        }

        bool isExistingCustomer;
        public bool IsExistingCustomer
        {
            get { return isExistingCustomer; }
            protected set
            {
                if (isExistingCustomer != value)
                {
                    isExistingCustomer = value;
                    OnPropertyChanged("IsExistingCustomer");
                }
            }
        }

        byte[] imageData;
        public byte[] ImageData
        {
            get { return imageData; }
            protected set
            {
                if (imageData != value)
                {
                    imageData = value;
                    OnPropertyChanged("ImageData");
                }
            }
        }

        Uri imageUri;
        public Uri ImageUri
        {
            get { return imageUri; }
            protected set
            {
                if (imageUri != value)
                {
                    imageUri = value;
                    OnPropertyChanged("ImageUri");
                }
            }
        }

        string previousOrderDate;
        public string PreviousOrderDate
        {
            get { return previousOrderDate; }
            protected set
            {
                if (previousOrderDate != value)
                {
                    previousOrderDate = value;
                    OnPropertyChanged("PreviousOrderDate");
                }
            }
        }

        string previousOrderDescription;
        public string PreviousOrderDescription
        {
            get { return previousOrderDescription; }
            protected set
            {
                if (previousOrderDescription != value)
                {
                    previousOrderDescription = value;
                    OnPropertyChanged("PreviousOrderDescription");
                }
            }
        }

        string currentOrderDescription;
        public string CurrentOrderDescription
        {
            get { return currentOrderDescription; }
            set
            {
                if (currentOrderDescription != value)
                {
                    currentOrderDescription = value;
                    OnPropertyChanged("CurrentOrderDescription");
                }
            }
        }

        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy == value)
                    return;

                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }


        public async Task<bool> IdentifyCustomer(byte[] image)
        {
            if (IsBusy)
                return false;

            IsBusy = true;
            ImageData = image;
            bool result = false;

            IdentifyCustomerResponse response = await AzureService.IdentifyCustomer(imageData);
            if (!response.HasError)
            {
                customerData = response.Customer;

                FirstName = string.IsNullOrEmpty(customerData.FirstName) ? "" : customerData.FirstName;
                LastName = string.IsNullOrEmpty(customerData.LastName) ? "" : customerData.LastName;
                Age = customerData.Age;
                Gender = customerData.Gender;

                if (response.PreviousOrder != null)
                {
                    PreviousOrderDate = (response.PreviousOrder.Date == default(DateTime)) ? null : string.Format("{0:MMMM dd, yyyy}", response.PreviousOrder.Date);
                    PreviousOrderDescription = response.PreviousOrder.Description;
                }

                ImageUri = new Uri(response.ImageUrl);
                Emotion = response.Emotion;
                result = true;
            }

            IsBusy = false;
            return result;
        }

        public async Task<string> SubmitNewOrder()
        {
            if (IsBusy)
                return null;
            IsBusy = true;

            // Update the customer details
            customerData.FirstName = FirstName;
            customerData.LastName = LastName;

            // Create and submit a new order for this user
            var order = new Order();
            order.Description = CurrentOrderDescription;
            order.Date = DateTime.Now;
            order.Emotion = Emotion;
            order.Total = Total;
            order.Status = OrderStatus.Pending;
            order.CustomerImageUrl = imageUri.ToString();

            var response = await AzureService.CreateOrder(customerData, order);
            if (response.ErrorCode != 0)
                return "Uhoh;" + response.Message;

            IsBusy = false;
            return string.Format("Thank you;\nYour order number is {0}", response.Order.OrderNumber);
        }

        public void Reset()
        {
            customerData = null;
            imageUri = null;
            imageData = null;
            firstName = null;
            lastName = null;
            age = 0;
            gender = null;
            emotion = null;
            previousOrderDate = null;
            previousOrderDescription = null;
            currentOrderDescription = null;
            total = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CustomerRecognition.Forms.Models.CreateOrderModel"/> class.
        /// </summary>
        public CreateOrderModel()
        {

        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
