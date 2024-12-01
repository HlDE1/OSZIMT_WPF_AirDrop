using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDropApp.Resources.Interfaces
{
    public interface IBluetoothService
    {
        List<string> GetPairedDevices();
    }

}
