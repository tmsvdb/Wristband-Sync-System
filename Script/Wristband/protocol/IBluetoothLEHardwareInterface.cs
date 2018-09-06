
using System;

namespace Wristband {

    public interface IBluetoothLEHardwareInterface {

        /*
            Initialize the Bluetooth system as either a central, peripheral or both. Acting as a 
            peripheral is only available for iOS. When completed the action callback will be 
            executed. If there is an error the errorAction callback will be executed.
        */
        IBluetoothDeviceScript Initialize(
            bool asCentral,
            bool asPeripheral,
            Action action,
            Action<string> errorAction
        );

        /*
            DeInitialize the Bluetooth system. When completed the action callback will be executed.
        */
        void DeInitialize(
            Action action
        );

        /*
            This method puts the device into a scan mode looking for any peripherals that support 
            the service UUIDs in the serviceUUIDs parameter array. If serviceUUIDs is NULL all 
            Bluetooth LE peripherals will be discovered. As devices are discovered the action 
            callback will be called with the ID and name of the peripheral.

            The default value for the actionAdvertisingInfo callback is null for backwards 
            compatibility. If you supply a callback for this parameter it will be called each time 
            advertising data is received from a device. You will receive the ID and address of the 
            device, the RSSI and the manufacturer specific data from the advertising packet.

            The rssiOnly parameter will allow scanned devices that don’t have manufacturer specific 
            data to still send the RSSI value. The reason this defaults to false is for backwards 
            compatibility.

            The clearPeripheralList is only used in iOS, but is here for cross platform compatibility 
            in the api. 
        */
        void ScanForPeripheralsWithServices(
            string[] serviceUUIDs,
            Action<string, string> action
        );

        /*
            This method stops the scanning mode initiated using the ScanForPeripheralsWithServices 
            method call.
        */
        void StopScan();

        /*
            This method will disconnect a peripheral by name. When the disconnection is complete the
            action callback is called with the ID of the peripheral.
        */
        void DisconnectPeripheral(
            string name,
            Action<string> action
        );

        /*
            This method will initiate a read of a characteristic using the name of the peripheral, 
            the service and characteristic to be read. If the read is successful the action callback
            is called with the UUID of the characteristic and the raw bytes of the read. 
        */
        void ReadCharacteristic(
            string name, 
            string service, 
            string characteristic,
            Action<string, byte[]> action
        );

        /*
            This method will initiate a write of a characteristic by the name of the peripheral and
            the service and characteristic to be written. The value to write is a byte buffer with 
            the length indicated in the data and length parameters. The withResponse parameter 
            indicates when the user wants a response after the write is completed. If a response is 
            requested then the action callback is called with the message from the Bluetooth system 
            on the result of the write operation.
        */
        void WriteCharacteristic(
            string name, 
            string service, 
            string characteristic, 
            byte[] data,
            int length,
            bool withResponse,
            Action<string> action
        );

        /*
            This method attempts to connect to the named peripheral. If the connection is successful
            the connectAction will be called with the name of the peripheral connected to. Once 
            connected the serviceAction is called for each service the peripheral supports. Each 
            service is enumerated and the characteristics supported by each service are indicated by 
            calling the characteristicAction callback.
            
            The default value for the disconnectAction is null for backwards compatibility. If you 
            supply a callback for this parameter it will be called whenever the connected device 
            disconnects. Keep in mind that if you also supply a callback for the DisconnectPeripheral
            command below both callbacks will be called.
        */
        void ConnectToPeripheral(
            string name,
            Action<string> connectAction,
            Action<string, string> serviceAction,
            Action<string, string, string> characteristicAction,
            Action<string> dissconnectAction = null
        );

        /*
            This method will initiate a write of a characteristic by the name of the peripheral and
            the service and characteristic to be written. The value to write is a byte buffer with
            the length indicated in the data and length parameters. The withResponse parameter
            indicates when the user wants a response after the write is completed. If a response is
            requested then the action callback is called with the message from the Bluetooth system
            on the result of the write operation.
        */
        void SubscribeCharacteristic(
            string name, 
            string service, 
            string characteristic, 
            Action<string> notificationAction,
            Action<string, byte[]> action
        );
    } 
}