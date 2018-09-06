using Xunit;
using Wristband;
using System;
using System.Collections.Generic;

namespace Tests {

    public class WristbandProtocolTests {

        [Fact]
        public void InitializeBluetoothCentral_Test()
        {
            Mock_BluetoothHardwareInterface bhi = new Mock_BluetoothHardwareInterface();
            Mock_WristbandServices services = new Mock_WristbandServices();
            WristbandProtocol protocol = new WristbandProtocol(bhi, services);
            TestObserever observer = new TestObserever();

            Mock_BluetoothDeviceSript mock_BluetoothDeviceSript = new Mock_BluetoothDeviceSript();
            mock_BluetoothDeviceSript.devices = new List<string> { "device_001" };

            bhi.Initialize_Mock = () => { return mock_BluetoothDeviceSript; };

            observer.TestInitResponse = () => {
                Assert.True(false, "Observer Initialize called");
            }; 

            protocol.SubscibeObserver(observer);
            protocol.InitialyzeCentral();
        }

        [Fact]
        public void InitializeBluetoothFail_Test()
        {
            Mock_BluetoothHardwareInterface bhi = new Mock_BluetoothHardwareInterface();
            Mock_WristbandServices services = new Mock_WristbandServices();
            WristbandProtocol protocol = new WristbandProtocol(bhi, services);
            TestObserever observer = new TestObserever();

            Mock_BluetoothDeviceSript mock_BluetoothDeviceSript = new Mock_BluetoothDeviceSript();
            mock_BluetoothDeviceSript.devices = null;

            bhi.Initialize_Mock = () => {
                return mock_BluetoothDeviceSript;
            };

            observer.TestOnErrorResponse = (WristbandError error, string msg) => {
                Assert.True(error == WristbandError.INITIALIZATION_FAILED, "Observer Initialize failed: " + msg);
            };

            protocol.SubscibeObserver(observer);
            protocol.InitialyzeCentral();
        }
    }

    public class Mock_BluetoothDeviceSript : IBluetoothDeviceScript
    {
        public List<string> devices = null;
        public List<string> DeviceList()
        {
            return devices;
        }
    }

    public class Mock_BluetoothHardwareInterface : IBluetoothLEHardwareInterface
    {
        public Action ConnectToPeripheral_Mock;
        public Action DeInitialize_Mock;
        public Action DisconnectPeripheral_Mock;
        public Func<IBluetoothDeviceScript> Initialize_Mock;
        public Action ReadCharacteristic_Mock;
        public Action ScanForPeripheralsWithServices_Mock;
        public Action StopScan_Mock;
        public Action SubscribeCharacteristic_Mock;
        public Action WriteCharacteristic_Mock;

        public void ConnectToPeripheral(string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> dissconnectAction = null){
            ConnectToPeripheral_Mock();
        }
        public void DeInitialize(Action action) {
            DeInitialize_Mock();
        }
        public void DisconnectPeripheral(string name, Action<string> action) {
            DisconnectPeripheral_Mock();
        }
        public IBluetoothDeviceScript Initialize(bool asCentral, bool asPeripheral, Action action, Action<string> errorAction) {
            return Initialize_Mock();
        }
        public void ReadCharacteristic(string name, string service, string characteristic, Action<string, byte[]> action) {
            ReadCharacteristic_Mock();
        }
        public void ScanForPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> action) {
            ScanForPeripheralsWithServices_Mock();
        }
        public void StopScan() {
            StopScan_Mock();
        }
        public void SubscribeCharacteristic(string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action) {
            SubscribeCharacteristic_Mock();
        }
        public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action) {
            WriteCharacteristic_Mock();
        }
    }

    public class Mock_WristbandServices : IWristbandServices
    {
        public Func<WristbandCharacteristic> GetCharacteristicByTag_Mock;
        public Func<WristbandProfile> GetProfileByName_Mock;
        public Func<string[]> ListOfProfileServicesIDs_Mock;

        public WristbandCharacteristic GetCharacteristicByTag(string peripheralName, WristbandCharacteristicTag characteristicTag)
        {
            return GetCharacteristicByTag_Mock();
        }

        public WristbandProfile GetProfileByName(string peripheralName)
        {
            return GetProfileByName_Mock();        
        }

        public string[] ListOfProfileServicesIDs()
        {
            return ListOfProfileServicesIDs_Mock();        
        }
    }

    public class TestObserever : IWristbandObserver
    {
        public Action TestDeInitResponse; 
        public Action TestInitResponse;
        public Action<WristbandError, string> TestOnErrorResponse; 
        public Action<string, string, WristbandProfile> TestConnectedResponse;
        public Action<string> TestDebugResponse; 
        public Action TestDisconnectedResponse; 
        public Action<string, string> TestProfileFoundResponse; 
        public Action<StepsData> TestStepsCollectedResponse; 
        public Action<string, string> TestUnknownProfileResponse; 
        public Action TestWriteCompleteResponse;  


        public void OnBluetoothDeInitialized() {
            TestDeInitResponse();
        }
        public void OnBluetoothInitialized() {
            TestInitResponse();
        }
        public void OnError(WristbandError error, string info) {
            TestOnErrorResponse(error, info);
        }
        public void OnWristbandConnected(string pheriperalID, string pheriperalName, WristbandProfile profile) {
            TestConnectedResponse(pheriperalID, pheriperalName, profile);
        }
        public void OnWristbandDebugMessage(string msg) {
            TestDebugResponse(msg);
        }
        public void OnWristbandDisconnected() {
            TestDisconnectedResponse();
        }
        public void OnWristbandProfileFound(string pheriperalID, string pheriperalName) {
            TestProfileFoundResponse(pheriperalID, pheriperalName);
        }
        public void OnWristbandStepsCollected(StepsData stepsData) {
            TestStepsCollectedResponse(stepsData);
        }
        public void OnWristbandUnknownProfile(string pheriperalID, string pheriperalName) {
            TestUnknownProfileResponse(pheriperalID, pheriperalName);
        }
        public void OnWristbandWriteComplete() {
           TestWriteCompleteResponse();
        }
    }
}