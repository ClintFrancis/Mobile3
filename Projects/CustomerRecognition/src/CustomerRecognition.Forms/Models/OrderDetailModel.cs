using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CustomerRecognition.Common;
using CustomerRecognition.Forms.Services;

namespace CustomerRecognition.Forms.Models
{
    public class OrderDetailModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        OrderStatus orderStatus;
        public OrderStatus OrderStatus
        {
            get { return orderStatus; }
            set
            {
                if (orderStatus == value)
                    return;

                orderStatus = value;
                OnPropertyChanged("OrderStatus");
            }
        }

        string description;
        public string Description
        {
            get { return description; }
            set
            {
                if (description == value)
                    return;

                description = value;
                OnPropertyChanged("Description");
            }
        }

        DateTime date;
        public DateTime Date
        {
            get { return date; }
            set
            {
                if (date == value)
                    return;

                date = value;
                OnPropertyChanged("Date");
            }
        }

        double total;
        public double Total
        {
            get { return total; }
            set
            {
                if (total == value)
                    return;

                total = value;
                OnPropertyChanged("Total");
            }
        }

        string emotion;
        public string Emotion
        {
            get { return emotion; }
            protected set
            {
                if (emotion == value)
                    return;

                emotion = value;
                OnPropertyChanged("Emotion");
            }
        }

        string firstName;
        public string FirstName
        {
            get { return firstName; }
            protected set
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
            protected set
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

        string imageUrl;
        public string ImageUrl
        {
            get { return imageUrl; }
            protected set
            {
                if (imageUrl != value)
                {
                    imageUrl = value;
                    OnPropertyChanged("ImageUrl");
                }
            }
        }

        Order order;
        Customer customer;

        public OrderDetailModel(Order order)
        {
            this.order = order;
            OrderStatus = order.Status;
            Date = order.Date;
            Description = order.Description;
            Total = order.Total;
            Emotion = order.Emotion;
            ImageUrl = order.CustomerImageUrl;

            RequestCustomerDetails();
        }

        public async Task RequestCustomerDetails()
        {
            var result = await AzureService.GetCustomerDetails(order.CustomerId);
            if (result.ErrorCode == 0)
            {
                FirstName = result.Customer.FirstName;
                LastName = result.Customer.LastName;
                Age = result.Customer.Age;
                Gender = result.Customer.Gender;
            }
        }

        public async Task ChangeOrderStatus(OrderStatus status)
        {
            if (IsBusy)
                return;
            IsBusy = true;

            var result = await AzureService.SetOrderStatus(order.id, status);
            if (result.ErrorCode == 0)
            {
                OrderStatus = status;
            }

            IsBusy = false;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
