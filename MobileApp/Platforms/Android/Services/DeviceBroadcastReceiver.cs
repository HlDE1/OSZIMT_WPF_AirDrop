using Android.App;
using Android.Bluetooth;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDropApp.Platforms.Android.Services
{
    [BroadcastReceiver(Enabled = true)]
    // [IntentFilter(new[] { "com.xamarin.example.WinDrop" })]
    public class DeviceBroadcastReceiver : BroadcastReceiver
    {
        public event Action<BluetoothDevice>? DeviceFound;
        public bool isReceiverRegistered = false;

        public DeviceBroadcastReceiver()
        {

        }

        public override void OnReceive(Context context, Intent intent)
        {
            Debug.WriteLine($"DeviceBroadcastReceiver received an intent: {intent}");
            Debug.WriteLine($"DeviceFoundevent: {DeviceFound == null}");
            // if (intent.Action == BluetoothDevice.ActionFound || intent.Action == BluetoothDevice.ActionBondStateChanged)
            {
                var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                if (device == null || string.IsNullOrEmpty(device.Name))
                    return;

                DeviceFound?.Invoke(device);

            }
        }
    }
}
