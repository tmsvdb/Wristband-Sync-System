using Xunit;
using Xunit.Abstractions;
using Wristband;
using System;
using System.Collections.Generic;

namespace Tests {

    public class WristbandProtocolTests {

        private readonly ITestOutputHelper output;
        public WristbandProtocolTests(ITestOutputHelper outputHelper) {
            output = outputHelper;
        }

        [Fact]
        public void ObservableSubscription_Test()
        {
            output.WriteLine("ObservableSubscription_Test");
            TestObservable observable = new TestObservable();
            TestObserever observer = new TestObserever();

            observable.SubscibeObserver(observer);
            List<IWristbandObserver> observers = observable.SubscribtionList;

            Assert.True(observers.Count > 0, "observers.Count should return exactly one subscription");
            Assert.True(observers.IndexOf(observer) != -1, "observers should contain the observer object");

            bool init, deinit, error, connect, debug, disconnect, profile, steps, unknown, write; 
            init = deinit = error = connect = debug = disconnect = profile = steps = unknown = write = false;

            observer.TestInitResponse = () => { 
                init = true; output.WriteLine("> TestObserever.OnBluetoothDeInitialized() :: called"); 
            };
            observer.TestDeInitResponse = () => { 
                deinit = true; output.WriteLine("> TestObserever.OnBluetoothInitialized() :: called"); 
            };
            observer.TestOnErrorResponse = (WristbandProtocolError werror, string msg) => { 
                error = true; output.WriteLine("> TestObserever.OnError() :: called - " + msg); 
            };
            observer.TestConnectedResponse = (string pheriperalID, string pheriperalName, WristbandProfile wprofile) => { 
                connect = true; output.WriteLine("> TestObserever.OnWristbandConnected() :: called"); 
            };
            observer.TestDebugResponse = (string msg) => { 
                debug = true; output.WriteLine("> TestObserever.OnWristbandDebugMessage() :: called"); 
            };
            observer.TestDisconnectedResponse = () => { 
                disconnect = true; output.WriteLine("> TestObserever.OnWristbandDisconnected() :: called"); 
            };
            observer.TestProfileFoundResponse = (string pheriperalID, string pheriperalName) => { 
                profile = true; output.WriteLine("> TestObserever.OnWristbandProfileFound() :: called"); 
            };
            observer.TestStepsCollectedResponse = (StepsData stepsData) => { 
                steps = true; output.WriteLine("> TestObserever.OnWristbandStepsCollected() :: called"); 
            };
            observer.TestUnknownProfileResponse = (string pheriperalID, string pheriperalName) => { 
                unknown = true; output.WriteLine("> TestObserever.OnWristbandUnknownProfile() :: called"); 
            };
            observer.TestWriteCompleteResponse = () => { 
                write = true; output.WriteLine("> TestObserever.OnWristbandWriteComplete() :: called"); 
            };

            observable.OnInitComplete_Test();
            observable.OnDeInitComplete_Test();
            observable.OnERROR_Test(WristbandProtocolError.INITIALIZATION_FAILED, "Debug Error");
            observable.OnConnected_Test("001", "002", BoosthWristbandServices.DebugProfile);
            observable.OnDebugMSG_Test("Debug Message");
            observable.OnDisconnected_Test();
            observable.OnMatchingProfile_Test("001", "002");
            observable.OnUnknownProfile_Test("001", "002");
            observable.OnStepsCollected_Test(new StepsData());
            observable.OnWriteComplete_Test();

            Assert.True(init); 
            Assert.True(deinit);
            Assert.True(error);
            Assert.True(connect);
            Assert.True(debug);
            Assert.True(disconnect);
            Assert.True(profile);
            Assert.True(steps);
            Assert.True(unknown);
            Assert.True(write); 

            observable.UnsubscibeObserver(observer);
            observers = observable.SubscribtionList;

            Assert.True(observers.Count == 0, "observers.Count should return exactly zero subscriptions after unsubscribing");
            Assert.True(observers.IndexOf(observer) == -1, "observers should not contain the observer object anymore, after unsubscribing");
        }

        [Fact]
        public void InitializeBluetoothCentral_Test()
        {
            Mock_BluetoothHardwareInterface bhi = new Mock_BluetoothHardwareInterface();
            Mock_WristbandServices services = new Mock_WristbandServices();
            WristbandProtocol protocol = new WristbandProtocol(bhi, services);
            TestObserever observer = new TestObserever();

            // Bluetooth hardware interface behaviour
            // ---------------------------
            bhi.Initialize_Mock = Initialize_BHI_Success;

            // track observer event states
            // ---------------------------
            bool initFired = false;
            bool errorFired = false;

            observer.TestInitResponse = () => {
                output.WriteLine("> TestObserever.OnBluetoothInitialized() :: called");
                initFired = true;
            }; 

            observer.TestOnErrorResponse = (WristbandProtocolError error, string msg) => {
                output.WriteLine("> TestObserever.OnError() :: Error - " + msg);
                errorFired = true;
            };

            // run protocol
            // ---------------------------
            protocol.SubscibeObserver(observer);
            protocol.InitialyzeCentral();

            // test results
            // ---------------------------
            Assert.True(initFired, "Observer should fire OnBluetoothInitialized");
            Assert.False(errorFired, "Observer should not fire OnError");

            // protocol state tests
            Assert.True(protocol.IsInitialized(), "protocol should be in initialized state");
            Assert.False(protocol.HasProfile (), "protocol should not have a profile");
            Assert.False(protocol.HasDiscoveredDevice(), "protocol should not be have disovered divices");
            Assert.False(protocol.IsConnected(), "protocol should not be connected");
            Assert.False(protocol.HasHistory(), "protocol should not have a history");
            Assert.False(protocol.IsBusy(), "protocol should not be busy");
        }

        [Fact]
        public void InitializeBluetoothFail_Test()
        {
            Mock_BluetoothHardwareInterface bhi = new Mock_BluetoothHardwareInterface();
            Mock_WristbandServices services = new Mock_WristbandServices();
            WristbandProtocol protocol = new WristbandProtocol(bhi, services);
            TestObserever observer = new TestObserever();

            // Bluetooth hardware interface behaviour
            // ---------------------------
            bhi.Initialize_Mock = Initialize_BHI_Fail;

            // track observer event states
            // ---------------------------
            bool initFired = false;
            bool errorFired = false;

            observer.TestInitResponse = () => {
                output.WriteLine("> TestObserever.OnBluetoothInitialized() :: called");
                initFired = true;
            }; 

            observer.TestOnErrorResponse = (WristbandProtocolError error, string msg) => {
                output.WriteLine("> TestObserever.OnError() :: Error - " + msg);
                errorFired = true;
            };

            // run protocol
            // ---------------------------
            protocol.SubscibeObserver(observer);
            protocol.InitialyzeCentral();

            // test results
            // ---------------------------
            Assert.False(initFired, "Observer should not fire OnBluetoothInitialized");
            Assert.True(errorFired, "Observer should fire OnError");

            // protocol state tests
            Assert.False(protocol.IsInitialized(), "protocol should not be in initialized state");
            Assert.False(protocol.HasProfile (), "protocol should not have a profile");
            Assert.False(protocol.HasDiscoveredDevice(), "protocol should not be have disovered divices");
            Assert.False(protocol.IsConnected(), "protocol should not be connected");
            Assert.False(protocol.HasHistory(), "protocol should not have a history");
            Assert.False(protocol.IsBusy(), "protocol should not be busy");
        }

        [Fact]
        public void DeInitializeBluetoothCentral_Test()
        {
            Mock_BluetoothHardwareInterface bhi = new Mock_BluetoothHardwareInterface();
            Mock_WristbandServices services = new Mock_WristbandServices();
            WristbandProtocol protocol = new WristbandProtocol(bhi, services);
            TestObserever observer = new TestObserever();

            // Bluetooth hardware interface behaviour
            // ---------------------------
            bhi.DeInitialize_Mock = (Action action) => { action(); };
            bhi.Initialize_Mock = Initialize_BHI_Success;

            // track observer event states
            // ---------------------------
            bool initFired = false;
            bool deinitFired = false;
            bool errorFired = false;

            observer.TestInitResponse = () => {
                output.WriteLine("> TestObserever.OnBluetoothInitialized() :: called");
                initFired = true;
            }; 

            observer.TestDeInitResponse = () => {
                output.WriteLine("> TestObserever.OnBluetoothDeInitialized() :: called");
                deinitFired = true;
            }; 

            observer.TestOnErrorResponse = (WristbandProtocolError error, string msg) => {
                output.WriteLine("> TestObserever.OnError() :: Error - " + msg);
                errorFired = true;
            };

            // run protocol
            // ---------------------------
            protocol.SubscibeObserver(observer);
            protocol.InitialyzeCentral();

            Assert.True(initFired && !deinitFired, "Observer Initialize should fire before Deinitialize");
            Assert.True(protocol.IsInitialized(), "protocol should be in initialized state before we deinitialize");

            protocol.DeInitialyzeCentral();

            // test results
            // ---------------------------
            Assert.True(deinitFired, "Observer should fire OnBluetoothDeInitialized");
            Assert.False(errorFired, "Observer should not fire OnError");

            // protocol state tests
            Assert.False(protocol.IsInitialized(), "protocol should not be in initialized state");
            Assert.False(protocol.HasProfile (), "protocol should not have a profile");
            Assert.False(protocol.HasDiscoveredDevice(), "protocol should not be have disovered divices");
            Assert.False(protocol.IsConnected(), "protocol should not be connected");
            Assert.False(protocol.HasHistory(), "protocol should not have a history");
            Assert.False(protocol.IsBusy(), "protocol should not be busy");
        }

        [Fact]
        public void ScanForDeviceAndAutoConnect_Test()
        {
            Mock_BluetoothHardwareInterface bhi = new Mock_BluetoothHardwareInterface();
            Mock_WristbandServices services = new Mock_WristbandServices();
            WristbandProtocol protocol = new WristbandProtocol(bhi, services);
            TestObserever observer = new TestObserever();

            // Bluetooth hardware interface behaviour
            // ---------------------------
            BluetoothHardwareInterface_ScanDevices(bhi, services);
            bhi.Initialize_Mock = Initialize_BHI_Success;

            // track observer event states
            // ---------------------------
            bool connectFired = false;
            bool errorFired = false;
            bool profileFoundFired = false;
            bool unknownProfile = false;

            observer.TestInitResponse = () => {
                output.WriteLine("> TestObserever.OnBluetoothInitialized() :: called");
            }; 
            observer.TestConnectedResponse = (string pheriperalID, string pheriperalName, WristbandProfile profile) => {
                output.WriteLine("> TestObserever.OnWristbandConnected() :: called -name:" + pheriperalName + ", -id:" + pheriperalID);
                connectFired = true;
            }; 
            observer.TestOnErrorResponse = (WristbandProtocolError error, string msg) => {
                output.WriteLine("> TestObserever.OnError() :: Error - " + msg);
                errorFired = true;
            };
            observer.TestProfileFoundResponse = (string id, string name) => {
                output.WriteLine("> TestObserever.OnWristbandProfileFound() :: called -name:" + name + ", -id:" + id);
                profileFoundFired = true;
            };
            observer.TestUnknownProfileResponse = (string id, string name) => {
                output.WriteLine("> TestObserever.OnWristbandUnknownProfile() :: called -name:" + name + ", -id:" + id);
                unknownProfile = true;
            };

            // run protocol
            // ---------------------------
            protocol.SubscibeObserver(observer);
            protocol.InitialyzeCentral();

            Assert.True(!errorFired && !connectFired, "Observer Initialize should fire before scanning for devices");
            Assert.True(protocol.IsInitialized(), "protocol should be in initialized state before scanning for devices");
            Assert.False(protocol.HasProfile (), "protocol should not have a profile before scanning for devices");
            Assert.False(protocol.HasDiscoveredDevice(), "protocol should not be have disovered divices before scanning for devices");
            Assert.False(protocol.IsConnected(), "protocol should not be connected before scanning for devices");

            protocol.ScanForDeviceAndAutoConnect();

            // test results
            // ---------------------------
            Assert.True(connectFired, "Observer should fire OnWristbandConnected");
            Assert.False(errorFired, "Observer should not fire OnError");
            Assert.True(profileFoundFired, "Observer should fire OnWristbandProfileFound");
            Assert.False(unknownProfile, "Observer should not fire OnWristbandUnknownProfile");

            // protocol state tests
            Assert.True(protocol.IsInitialized(), "protocol should be in initialized state");
            Assert.True(protocol.HasProfile (), "protocol should have a profile");
            Assert.True(protocol.HasDiscoveredDevice(), "protocol should have disovered divices");
            Assert.True(protocol.IsConnected(), "protocol should be connected");
            Assert.False(protocol.HasHistory(), "protocol should not have a history");
            Assert.False(protocol.IsBusy(), "protocol should not be busy");        
        }

/*
StopScan()
Disconnect()
Abort()
ReadStepsAndExercizeTime()
SetClockAndUserSettings (byte [] data)
ClearHistory ()
ReadCharacteristicsHandler(string str, byte[] dat)
*/

        /*
            MOCK OBJECTS
        */
        void BluetoothHardwareInterface_ScanDevices (Mock_BluetoothHardwareInterface bhi, Mock_WristbandServices services) {

            bhi.ScanForPeripheralsWithServices_Mock = (string[] serviceUUIDs, Action<string, string> action) => {
                // fire device found: device name and id
                output.WriteLine("> BluetoothHardwareInterface.ScanForPeripheralsWithServices() :: called");
                action("PCI\\VEN_1000&DEV_0001&SUBSYS_00000000&REV_02", "PR102");
            };
            bhi.ConnectToPeripheral_Mock = (string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction) => { 
                // characteristicAction: string device_id, string service_id, string characteristic_id
                // represents one of the bleutooth devices found 
                output.WriteLine("> BluetoothHardwareInterface.ConnectToPeripheral() :: called");
                characteristicAction("PR102", "0000FED0-494C-4F47-4943-544543480000", "0000FED1-494C-4F47-4943-544543480000"); 
            };
            bhi.SubscribeCharacteristic_Mock = (string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action) => {
                // subscribe for device notifications                
                output.WriteLine("> BluetoothHardwareInterface.SubscribeCharacteristic() :: called");
                notificationAction("Notification Debug message");
            };
            services.ListOfProfileServicesIDs_Mock = () => {
                return new string[1] { "0000FED0-494C-4F47-4943-544543480000" };
            };
            services.GetProfileByName_Mock = (string name) => {
                output.WriteLine("> WristbandServices.GetProfileByName() :: called -name:" + name );
                return BoosthWristbandServices.DebugProfile;
            };
        }

        IBluetoothDeviceScript Initialize_BHI_Success (bool asCentral, bool asPeripheral, Action action, Action<string> errorAction) {
            output.WriteLine("> Initialize_BHI_Success () :: called");
            action();
            return new Mock_BluetoothDeviceSript() as IBluetoothDeviceScript;
        }

        IBluetoothDeviceScript Initialize_BHI_Fail (bool asCentral, bool asPeripheral, Action action, Action<string> errorAction) {
            output.WriteLine("> Initialize_BHI_Fail () :: called");
            errorAction("Debug Failure");
            return null;
        }

        public class TestObservable : WristbandObservable {
            public void OnInitComplete_Test () {
                OnInitComplete();
            }
            public void OnDeInitComplete_Test () {
                OnDeInitComplete();
            }
            public void OnERROR_Test (WristbandProtocolError error, string info) {
                OnERROR(error, info);
            }
            public void OnDebugMSG_Test (string msg) {
                OnDebugMSG(msg);
            }
            public void OnConnected_Test (string pheriperalID, string pheriperalName, WristbandProfile profile) {
                OnConnected(pheriperalID, pheriperalName, profile);
            }
            public void OnDisconnected_Test () {
                OnDisconnected();
            }
            public void OnMatchingProfile_Test (string pheriperalID, string pheriperalName) {
                OnMatchingProfile(pheriperalID, pheriperalName);
            }
            public void OnUnknownProfile_Test (string pheriperalID, string pheriperalName) {
                OnUnknownProfile(pheriperalID, pheriperalName);
            }
            public void OnStepsCollected_Test (StepsData stepsData) {
                OnStepsCollected(stepsData);
            }
            public void OnWriteComplete_Test () {
                OnWriteComplete();
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
            public Action<string, Action<string>, Action<string, string>, Action<string, string, string>> ConnectToPeripheral_Mock;
            public Action<Action> DeInitialize_Mock;
            public Action<string, Action<string>> DisconnectPeripheral_Mock;
            public Func<bool, bool, Action, Action<string>, IBluetoothDeviceScript> Initialize_Mock;
            public Action<string, string, string, Action<string, byte[]>> ReadCharacteristic_Mock;
            public Action<string[], Action<string, string>> ScanForPeripheralsWithServices_Mock;
            public Action StopScan_Mock;
            public Action<string, string, string, Action<string>, Action<string, byte[]>> SubscribeCharacteristic_Mock;
            public Action<string, string, string, byte[], int, bool, Action<string>> WriteCharacteristic_Mock;

            public void ConnectToPeripheral(string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> dissconnectAction = null){
                ConnectToPeripheral_Mock(name, connectAction, serviceAction, characteristicAction);
            }
            public void DeInitialize(Action action) {
                DeInitialize_Mock(action);
            }
            public void DisconnectPeripheral(string name, Action<string> action) {
                DisconnectPeripheral_Mock(name, action);
            }
            public IBluetoothDeviceScript Initialize(bool asCentral, bool asPeripheral, Action action, Action<string> errorAction) {
                return Initialize_Mock(asCentral, asPeripheral, action, errorAction);
            }
            public void ReadCharacteristic(string name, string service, string characteristic, Action<string, byte[]> action) {
                ReadCharacteristic_Mock(name, service, characteristic, action);
            }
            public void ScanForPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> action) {
                ScanForPeripheralsWithServices_Mock(serviceUUIDs, action);
            }
            public void StopScan() {
                StopScan_Mock();
            }
            public void SubscribeCharacteristic(string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action) {
                SubscribeCharacteristic_Mock(name, service, characteristic, notificationAction, action);
            }
            public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action) {
                WriteCharacteristic_Mock(name, service, characteristic, data, length, withResponse, action);
            }
        }

        public class Mock_WristbandServices : IWristbandServices
        {
            public Func<WristbandCharacteristic> GetCharacteristicByTag_Mock;
            public Func<string, WristbandProfile> GetProfileByName_Mock;
            public Func<string[]> ListOfProfileServicesIDs_Mock;

            public WristbandCharacteristic GetCharacteristicByTag(string peripheralName, WristbandCharacteristicTag characteristicTag)
            {
                return GetCharacteristicByTag_Mock();
            }

            public WristbandProfile GetProfileByName(string peripheralName)
            {
                return GetProfileByName_Mock(peripheralName);        
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
            public Action<WristbandProtocolError, string> TestOnErrorResponse; 
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
            public void OnError(WristbandProtocolError error, string info) {
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
}