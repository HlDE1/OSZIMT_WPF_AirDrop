using Android.App;
using Android.Bluetooth;
using Android.Runtime;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;
using WinDropApp.Platforms.Android.Services;
using WinDropApp.Resources.Interfaces;
namespace WinDropApp
{
    [Application]
    public class MainApplication : MauiApplication
    {


        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            Debug.WriteLine("Android application constructor...");
        }


        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override void OnCreate()
        {
            base.OnCreate();

            Debug.WriteLine("Android application starting...");
            PermissionHelper.RequestBluetoothAndGpsPermissions();
            InitializeBluetooth();
        }

        private async void InitializeBluetooth()
        {
            try
            {
                //TODO: ask for permission

                IBluetoothLE ble = CrossBluetoothLE.Current;
                IAdapter adapter = CrossBluetoothLE.Current.Adapter;
                //adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.Balanced;
                //adapter.ScanTimeout = 1000 * 30;
               // adapter.ScanMatchMode = ScanMatchMode.AGRESSIVE;

                Debug.WriteLine($"Bluetooth Connected Devices: {adapter?.ConnectedDevices.Count}");
                Debug.WriteLine($"Bluetooth Bonded Devices: {adapter.BondedDevices.Count}");
                Debug.WriteLine($"Bluetooth Discovered Devices: {adapter.DiscoveredDevices.Count}");

                ble.StateChanged += (s, e) =>
                {
                    Debug.WriteLine($"The Bluetooth state changed to {e.NewState}");
                };

                // Device discovered event
                adapter.DeviceDiscovered += (s, a) =>
                {
                    var deviceName = string.IsNullOrEmpty(a.Device.Name) ? "Unknown Device" : a.Device.Name;
                    Debug.WriteLine($"Bluetooth Callback: DeviceDiscovered: {deviceName}");
                };

                // Device advertised event
                adapter.DeviceAdvertised += (s, a) =>
                {
                    var deviceName = string.IsNullOrEmpty(a.Device.Name) ? "Unknown Device" : a.Device.Name;
                    Debug.WriteLine($"Bluetooth Callback: DeviceAdvertised: {deviceName}");
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
                await adapter.StartScanningForDevicesAsync();
                Debug.WriteLine("Bluetooth: Scanning started.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing Bluetooth: {ex.Message}");
            }
        }
    }

}
