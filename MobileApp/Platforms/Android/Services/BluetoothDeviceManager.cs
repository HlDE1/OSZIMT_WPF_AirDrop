using Android.Bluetooth;
using Android.Content;
using Android.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDropApp.Platforms.Android.Services
{
    public class BluetoothDeviceManager
    {
        public BluetoothAdapter _bluetoothAdapter;

        public BluetoothDeviceManager()
        {
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        }


        public List<BluetoothDevice> GetBondedDevices()
        {
            if (_bluetoothAdapter == null)
            {
                Log.Warn("BluetoothDeviceManager", "Bluetooth adapter is null.");
                return new List<BluetoothDevice>();
            }

            return _bluetoothAdapter.BondedDevices?.ToList() ?? new List<BluetoothDevice>();
        }

    }
}
