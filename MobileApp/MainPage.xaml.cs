using Android.Bluetooth;
using Android.Content;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WinDropApp.Platforms.Android.Services;
using WinDropApp.Resources.Interfaces;

namespace WinDropApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private ObservableCollection<string> _devices;
        private List<BluetoothDevice> _devices2 = new();

        List<string> bluetoothDevices = new List<string>();
        DeviceBroadcastReceiver deviceReceiver = new DeviceBroadcastReceiver();
        BluetoothDeviceManager deviceManager = new BluetoothDeviceManager();


        public MainPage()
        {
            InitializeComponent();
            PermissionHelper.RequestBluetoothAndGpsPermissions();
            deviceReceiver.DeviceFound += DeviceReceiver_DeviceFound;
        }

        private void DeviceReceiver_DeviceFound(BluetoothDevice obj)
        {
            if (_devices2.Any(x => x.Address == obj.Address))
                return;

            _devices2.Add(obj);

            Debug.WriteLine("DeviceReceiver_DeviceFound: " + _devices2.Select(x => x.Name));
            RefreshListView(BluetoothDevicesList, _devices2.Select(x => x.Name).ToList());
        }

        void RefreshListView<T>(ListView listview, T data) where T : class, IEnumerable
        {
            listview.ItemsSource = null;
            listview.ItemsSource = data;
        }

        private void Adapter_DeviceBondStateChanged(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceBondStateChangedEventArgs e)
        {
            Debug.WriteLine($"DeviceBondStateChanged: {e.Device.Name}");
        }

        private void Adapter_DeviceConnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            Debug.WriteLine($"Device Connected: {e.Device.Name}");
        }

        private void OnStartScanClicked(object sender, EventArgs e)
        {
            TestBluetooth();
        }


        void TestBluetooth()
        {

            if (deviceReceiver.isReceiverRegistered && deviceManager._bluetoothAdapter.IsDiscovering)
            {
                Android.App.Application.Context.UnregisterReceiver(deviceReceiver);
                deviceReceiver.isReceiverRegistered = false;
                StartScanBtn.Text = "Start Scan";
                return;
            }

            if (deviceManager._bluetoothAdapter == null || !deviceManager._bluetoothAdapter.IsEnabled)
            {
                Debug.WriteLine("Bluetooth is not enabled.");
                return;
            }

            List<BluetoothDevice> boundedDevices = deviceManager.GetBondedDevices();
            List<string> bondedDeviceNames = new List<string>();

            foreach (var device in boundedDevices)
            {
                bondedDeviceNames.Add(device.Name);
            }

            RefreshListView(BoundBluetoothDevicesList, bondedDeviceNames);


            bool startedScan = deviceManager._bluetoothAdapter.StartDiscovery();
            deviceReceiver.isReceiverRegistered = true;
            StartScanBtn.Text = "Scanning...";

            IntentFilter filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionAclConnected);
            filter.AddAction(BluetoothDevice.ActionAclDisconnected);

            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            Android.App.Application.Context.RegisterReceiver(deviceReceiver, filter);
        }
    }
}
