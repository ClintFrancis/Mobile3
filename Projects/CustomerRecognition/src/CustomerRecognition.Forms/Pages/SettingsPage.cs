using System;
using CustomerRecognition.Forms.Models;
using Xamarin.Forms;

namespace CustomerRecognition.Forms
{
    public class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            BindingContext = new SettingsModel();

            var cameraSwitch = new SwitchCell { Text = "Rear Camera" };
            cameraSwitch.SetBinding(SwitchCell.OnProperty, new Binding("DefaultCameraRear"));

            var timerEntry = new EntryCell { Label = "Seconds", Keyboard = Keyboard.Numeric };
            timerEntry.SetBinding(EntryCell.TextProperty, new Binding("TimerInterval", BindingMode.TwoWay));

            var clearDataLabel = new TextCell { Text = "Clear All Data" };
            clearDataLabel.Tapped += async (s, e) =>
            {
                var result = await DisplayAlert("Delete All Data", "Are you sure you wish to clear all stored data online?", "YES", "Cancel");
                if (result)
                {
                    var response = await (BindingContext as SettingsModel).ResetData();
                    DisplayAlert(response, "", "Ok");
                }
            };

            Title = "Settings";
            Content = new TableView
            {
                Root = new TableRoot{
                    new TableSection("Default Camera") {
                        cameraSwitch
                    },
                    new TableSection("Data"){
                        clearDataLabel
                    },
                    new TableSection("Identify Interval"){
                        timerEntry
                    }
                }
            };

            //

        }
    }
}

