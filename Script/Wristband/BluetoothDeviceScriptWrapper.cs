
using System.Collections.Generic;

namespace Wristband {
    public class BluetoothDeviceScriptWrapper: IBluetoothDeviceScript 
    {
        private BluetoothDeviceScript bluetoothDeviceScript;
        
        public BluetoothDeviceScriptWrapper (BluetoothDeviceScript bluetoothDeviceScript) {
            this.bluetoothDeviceScript = bluetoothDeviceScript;
        }

        public List<string> DeviceList () {
            return bluetoothDeviceScript.DiscoveredDeviceList;
        }
    }
}