using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CustomerRecognition.Common;
using CustomerRecognition.Forms.Services;
using Xamarin.Forms;
using System.Linq;

namespace CustomerRecognition.Forms.Models
{
    public interface IOrdersModel
    {
        ObservableCollection<GroupedOrderCollection> GroupedOrders { get; }
        Task RefreshOrders();
        Task ChangeOrderStatus(Order order, OrderStatus status);
        ICommand RefreshOrdersCommand { get; }
        Order SelectedItem { get; set; }
        bool IsBusy { get; }
        void Reset();
    }

    public class GroupedOrderCollection : ObservableCollection<Order>
    {
        public string LongName { get; set; }
        public string ShortName { get; set; }
    }

    public class OrdersModel : INotifyPropertyChanged, IOrdersModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<GroupedOrderCollection> GroupedOrders { get; protected set; } = new ObservableCollection<GroupedOrderCollection>();
        public ICommand RefreshOrdersCommand { protected set; get; }

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

        Order selectedItem;
        public Order SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem == value)
                    return;

                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public OrdersModel()
        {
            RefreshOrdersCommand = new Command(async () => await RefreshOrders());
        }

        public async Task RefreshOrders()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            var response = await AzureService.GetCurrentOrders();
            if (!response.HasError && response.Orders != null)
            {
                var sortedOrders = response.Orders.OrderByDescending(o => o.Date);
                GroupedOrders.Clear();

                var readyOrders = new GroupedOrderCollection() { LongName = "Ready Orders", ShortName = "R" };
                var pendingOrders = new GroupedOrderCollection() { LongName = "Pending Orders", ShortName = "P" };

                foreach (var item in response.Orders)
                {
                    if (item.Status == OrderStatus.Ready)
                        readyOrders.Add(item);

                    else
                        pendingOrders.Add(item);
                }

                GroupedOrders.Add(readyOrders);
                GroupedOrders.Add(pendingOrders);
            }

            IsBusy = false;
        }

        public async Task ChangeOrderStatus(Order order, OrderStatus status)
        {
            if (IsBusy)
                return;
            IsBusy = true;

            var result = await AzureService.SetOrderStatus(order.id, status);
            IsBusy = false;
            if (result.ErrorCode == 0)
                await RefreshOrders();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
