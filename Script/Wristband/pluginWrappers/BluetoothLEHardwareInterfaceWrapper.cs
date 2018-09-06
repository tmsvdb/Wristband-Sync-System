using System;

namespace Wristband {

public class BluetoothLEHardwareInterfaceWrapper : IBluetoothLEHardwareInterface
    {
        public IBluetoothDeviceScript Initialize(
            bool asCentral,
            bool asPeripheral,
            Action action,
            Action<string> errorAction
        ) {
            return new BluetoothDeviceScriptWrapper(BluetoothLEHardwareInterface.Initialize(asCentral, asPeripheral, action, errorAction));
        }

        public void DeInitialize(
            Action action
        ) {
            BluetoothLEHardwareInterface.DeInitialize(action);
        }

        public void ScanForPeripheralsWithServices(
            string[] serviceUUIDs,
            Action<string, string> action
        ) {
            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(serviceUUIDs, action);
        }

        public void StopScan() {
            BluetoothLEHardwareInterface.StopScan();
        }

        public void DisconnectPeripheral(
            string name,
            Action<string> action
        ) {
            BluetoothLEHardwareInterface.DisconnectPeripheral(name, action);
        }

        public void ReadCharacteristic(
            string name, 
            string service, 
            string characteristic,
            Action<string, byte[]> action
        ) {
            BluetoothLEHardwareInterface.ReadCharacteristic(name, service, characteristic, action);
        }

        public void WriteCharacteristic(
            string name, 
            string service, 
            string characteristic, 
            byte[] data,
            int length,
            bool withResponse,
            Action<string> action
        ) {
            BluetoothLEHardwareInterface.WriteCharacteristic(name, service, characteristic, data, length, withResponse, action);
        }

        public void ConnectToPeripheral(
            string name,
            Action<string> connectAction,
            Action<string, string> serviceAction,
            Action<string, string, string> characteristicAction,
            Action<string> dissconnectAction = null
        ) {
            BluetoothLEHardwareInterface.ConnectToPeripheral(name, connectAction, serviceAction, characteristicAction, dissconnectAction);
        }

        public void SubscribeCharacteristic(
            string name, 
            string service, 
            string characteristic, 
            Action<string> notificationAction,
            Action<string, byte[]> action
            ) {
            BluetoothLEHardwareInterface.SubscribeCharacteristic(name, service, characteristic, notificationAction, action);
        }
    }
}