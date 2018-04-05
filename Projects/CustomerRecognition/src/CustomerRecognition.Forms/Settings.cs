using System;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace CustomerRecognition.Forms
{
    public static class Settings
    {
        static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        const string cameraOption = "cameraOption";
        private static readonly int CameraOptionDefault = (int)CameraOptions.Rear;

        public static CameraOptions CameraOption
        {
            get { return (CameraOptions)AppSettings.GetValueOrDefault(cameraOption, CameraOptionDefault); }
            set { AppSettings.AddOrUpdateValue(cameraOption, (int)value); }
        }

        const string timerInterval = "timerInterval";
        private static readonly int TimerIntervalDefault = 10;

        public static int TimerInterval
        {
            get { return AppSettings.GetValueOrDefault(timerInterval, TimerIntervalDefault); }
            set { AppSettings.AddOrUpdateValue(timerInterval, (int)value); }
        }
    }
}

