using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CustomerRecognition.Common;
using CustomerRecognition.Forms.Services;
using Xamarin.Forms;

namespace CustomerRecognition.Forms.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<GroupedOrderCollection> GroupedOrders { get; protected set; } = new ObservableCollection<GroupedOrderCollection>();
        public ICommand RefreshOrdersCommand { protected set; get; }

        bool defaultCameraRear;
        public bool DefaultCameraRear
        {
            get { return defaultCameraRear; }
            set
            {
                if (defaultCameraRear == value)
                    return;

                defaultCameraRear = value;
                Settings.CameraOption = value ? CameraOptions.Rear : CameraOptions.Front;

                OnPropertyChanged("DefaultCameraRear");
            }
        }

        int timerInterval;
        public int TimerInterval
        {
            get { return Settings.TimerInterval; }
            set
            {
                if (Settings.TimerInterval == value)
                    return;

                Settings.TimerInterval = value;
                OnPropertyChanged("TimerInterval");
            }
        }

        public SettingsModel()
        {
            DefaultCameraRear = (Settings.CameraOption == CameraOptions.Rear);
        }

        public async Task<string> ResetData()
        {
            var response = await AzureService.Reset(true, true, true, true);
            if (!response.HasError)
            {
                return response.Message;
            }

            return "Something went wrong";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
