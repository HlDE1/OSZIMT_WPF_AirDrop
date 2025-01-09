using Android.Bluetooth;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE;
using WinDropApp.Resources.Interfaces;
using System.Diagnostics;
using Plugin.BLE.Abstractions.EventArgs;

namespace WinDropApp.Platforms.Android.Services
{
    public class BluetoothService
    {
        public IBluetoothLE ble = CrossBluetoothLE.Current;
        public IAdapter adapter = CrossBluetoothLE.Current.Adapter;

        public async void InitializeBluetooth(bool startScan, Action<DeviceEventArgs>? deviceDiscovered, Action<DeviceEventArgs>? deviceAdvertised)
        {
            try
            {

                adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.Balanced;
                adapter.ScanTimeout = 10000 * 30; //in miliseconds
                adapter.ScanMatchMode = ScanMatchMode.AGRESSIVE;

                Debug.WriteLine($"Bluetooth Connected Devices: {adapter?.ConnectedDevices.Count}");
                Debug.WriteLine($"Bluetooth Bonded Devices: {adapter?.BondedDevices.Count}");
                Debug.WriteLine($"Bluetooth Discovered Devices: {adapter?.DiscoveredDevices.Count}");

                ble.StateChanged += (s, e) =>
                {
                    Debug.WriteLine($"The Bluetooth state changed to {e.NewState}");
                };

     

                // Device discovered event
                adapter.DeviceDiscovered += (s, a) =>
                {
                    //  if (string.IsNullOrEmpty(a.Device.Name) || string.IsNullOrWhiteSpace(a.Device.Name))
                    //    return;

                    if (deviceDiscovered != null)
                        deviceDiscovered(a);
                };

                // Device advertised event
                adapter.DeviceAdvertised += (s, a) =>
                {
                    // if (string.IsNullOrEmpty(a.Device.Name) || string.IsNullOrWhiteSpace(a.Device.Name))
                    //     return;

                    if (deviceAdvertised != null)
                        deviceAdvertised(a);

                    Debug.WriteLine($"Bluetooth Callback: DeviceAdvertised: {a.Device.Name}");
                };

                adapter.DeviceBondStateChanged += (s, a) =>
                {
                    var deviceName = string.IsNullOrEmpty(a.Device.Name) ? "Unknown Device" : a.Device.Name;
                    Debug.WriteLine($"Bluetooth Callback: DeviceBondStateChanged: {deviceName}");
                };

                // Check Bluetooth state
                if (ble.State == BluetoothState.Off)
                {
                    Debug.WriteLine("Bluetooth is turned off. Please enable it.");
                    return;
                }

                Debug.WriteLine("Bluetooth: Starting scanning for devices...");

                if (startScan)
                    await adapter.StartScanningForDevicesAsync();

                //await adapter.ConnectToDeviceAsync();
                Debug.WriteLine("Bluetooth: Scanning started.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing Bluetooth: {ex.Message}");
            }
        }

    }
}
