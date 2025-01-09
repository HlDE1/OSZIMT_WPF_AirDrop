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

        // BluetoothService bluetoothService = new BluetoothService();

        DeviceBroadcastReceiver deviceReceiver = new DeviceBroadcastReceiver();
        BluetoothDeviceManager deviceManager = new BluetoothDeviceManager();


        public MainPage()
        {
            InitializeComponent();

            PermissionHelper.RequestBluetoothAndGpsPermissions();
            deviceReceiver.DeviceFound += DeviceReceiver_DeviceFound;
            /*
            bluetoothService.adapter.ScanTimeoutElapsed += (s, a) =>
            {
                Debug.WriteLine("ScanTimeoutElapsed");
                StartScanBtn.Text = "Start Scan";

            };
            bluetoothService.adapter.DeviceConnected += Adapter_DeviceConnected;
            bluetoothService.adapter.DeviceBondStateChanged += Adapter_DeviceBondStateChanged;
            //bluetoothService.adapter.

            bluetoothService.InitializeBluetooth(false,
           (deviceArgs) =>
           {
               bluetoothDevices.Add(deviceArgs.Device.Name);
               BluetoothDevicesList.ItemsSource = null;
               BluetoothDevicesList.ItemsSource = bluetoothDevices;
               Debug.WriteLine($"deviceArgs {deviceArgs.Device.Name} {deviceArgs.Device.BondState} {deviceArgs.Device.IsConnectable} {deviceArgs.Device.Id}");
           },
           (advertisedArgs) =>
           {
               // BluetoothDevicesList.ItemsSource = null;
               //  BluetoothDevicesList.ItemsSource = bluetoothDevices;
               //  Log($"advertisedArgs: {advertisedArgs.Device.Name}");
           });
            */

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
            //  RefreshListView(BoundBluetoothDevicesList, bluetoothService.adapter.BondedDevices);
        }

        private void Adapter_DeviceConnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            Debug.WriteLine($"Device Connected: {e.Device.Name}");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //  RefreshListView(BoundBluetoothDevicesList, bluetoothService.adapter.BondedDevices);
        }

        private void OnStartScanClicked(object sender, EventArgs e)
        {
            TestBluetooth();
            //return;
            //if (bluetoothService.adapter.IsScanning)
            //{
            //    bluetoothService.adapter.StopScanningForDevicesAsync();
            //    StartScanBtn.Text = "Start Scan";
            //    return;
            //}

            //bluetoothService.adapter.StartScanningForDevicesAsync();
            //StartScanBtn.Text = "Scanning...";

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

            var boundedDevices = deviceManager.GetBondedDevices();
            List<string> bondedDeviceNames = new List<string>();

            foreach (var device in boundedDevices)
            {
                bondedDeviceNames.Add(device.Name);
            }

            RefreshListView(BoundBluetoothDevicesList, bondedDeviceNames);


            bool startedScan = deviceManager._bluetoothAdapter.StartDiscovery();
            deviceReceiver.isReceiverRegistered = true;
            StartScanBtn.Text = "Scanning...";

            var filter = new IntentFilter(); //BluetoothDevice.ActionFound
            filter.AddAction(BluetoothDevice.ActionAclConnected);
            filter.AddAction(BluetoothDevice.ActionAclDisconnected);

            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothDevice.ActionBondStateChanged);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            Android.App.Application.Context.RegisterReceiver(deviceReceiver, filter);
            // Android.App.Application.Context.UnregisterReceiver(deviceReceiver);

        }

        private void BluetoothDevicesList_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {

        }


    }

}
