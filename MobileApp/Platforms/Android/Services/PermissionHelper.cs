using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDropApp.Platforms.Android.Services
{
    public static class PermissionHelper
    {
        public static async void RequestBluetoothAndGpsPermissions()
        {

            var bluetoothStatus = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
            if (bluetoothStatus != PermissionStatus.Granted)
            {
                bluetoothStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
            }

            if (bluetoothStatus != PermissionStatus.Granted)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Permission Required",
                    "Bluetooth permissions are required for this app to function properly.",
                    "OK");
                return;
            }


            // Request  (GPS) permissions
            var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (locationStatus != PermissionStatus.Granted)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Permission Required",
                    "Location permissions are required for GPS services.",
                    "OK");
                return;
            }

            await Application.Current.MainPage.DisplayAlert(
                "Permissions Granted",
                "Bluetooth and GPS permissions have been granted.",
                "OK");
        }
    }
}
