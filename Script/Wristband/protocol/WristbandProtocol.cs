using System.Collections;
using System.Collections.Generic;
using System;

namespace Wristband {

    public enum WristbandProtocolError {
        INITIALIZATION_FAILED,
        CONNECTION_FAILED,
        READ_STEPS_FAILED,
        SET_WRISTBAND_FAILED,
        CLEAR_HISTORY_FAILED,
        INVALID_NUMBER_OF_STEPS,
        NOTIFICATION_SUBSCRTIPTION_FAILED,
        UNKNOWN_DEVICE_DETECTED,
    }
    
    public class WristbandProtocol : WristbandObservable
    {
        // dependencies
        private IBluetoothLEHardwareInterface bluetoothLEHardwareInterface;
        private IWristbandServices services;

        // runtime data
        private IBluetoothDeviceScript bluetoothDeviceScript = null;
        private WristbandProfile connectedWristbandProfile = null;
        private StepsData steps = new StepsData();

        private int days_returned = 0;
        private int numRetries = 0;
        private string pheriperalID;
        private string pheriperalName;

        private bool device_found = false;
        private bool device_connected = false;
        private bool history_retieved = false;
        private bool busy = false;

        public WristbandProtocol (
            IBluetoothLEHardwareInterface bluetoothLEHardwareInterface,
            IWristbandServices services
            )
        {
            this.bluetoothLEHardwareInterface = bluetoothLEHardwareInterface;
            this.services = services;
        }

        /*
        ==================================== 
        PUBLIC INTERFACE:
        ==================================== 
        
        ------------------------------------
        PROTOCOL STATES:
        ------------------------------------ 
        */
        public bool IsInitialized () { return (bluetoothDeviceScript != null); }
        public bool HasProfile () { return (connectedWristbandProfile != null); }
        public bool HasDiscoveredDevice() { return (device_found == true); }
        public bool IsConnected() { return (device_connected == true); }
        public bool HasHistory() { return (history_retieved == true); }
        public bool IsBusy() { return (busy == true); }
        

        /*
        ------------------------------------
        CONNECTED PROFILE DATA:
        ------------------------------------ 
        */
        public WristbandProfileType ConnectedProfileType { get { return connectedWristbandProfile.description; } }
        public StepsData GetSteps { get { return steps ; } }
        public string[] DeviceList { get { return IsInitialized () ? bluetoothDeviceScript.DeviceList().ToArray() : new string[0]; } }

        /*

        ------------------------------------ 
        INIT/CONNECT:
        ------------------------------------ 
        */
        public void InitialyzeCentral ()
        {
            if (bluetoothDeviceScript == null)
                bluetoothDeviceScript = bluetoothLEHardwareInterface.Initialize(true, false, InitCompleteHandler, InitErrorHandler);
            else
                InitCompleteHandler();
        }

        public void DeInitialyzeCentral ()
        {
            bluetoothDeviceScript = null;

            if (IsInitialized()) {
                busy = true;
                bluetoothLEHardwareInterface.DeInitialize(() => { 
                    busy = false; 
                    OnDeInitComplete ();
                });
            }
            else 
                OnDeInitComplete ();
        }

        public void ScanForDeviceAndAutoConnect ()
        {
            device_found = false;
            device_connected = false;
            history_retieved = false;

            if (IsInitialized()) {
                busy = true;
                string[] servicesToScanFor = services.ListOfProfileServicesIDs();
                bluetoothLEHardwareInterface.ScanForPeripheralsWithServices(servicesToScanFor, ScanForPeripheralsWithServicesResponseHandler);
            }
            else {
                OnERROR(WristbandProtocolError.CONNECTION_FAILED, "Bluetooth Central Error, Rebooting Central...");
            }
        }

        public void StopScan ()
        {
            if (IsInitialized() && IsBusy()) {
                busy = false;
                bluetoothLEHardwareInterface.StopScan();
            }
        }

        public void Disconnect()
        {
            connectedWristbandProfile = null;

            if (bluetoothDeviceScript != null && bluetoothDeviceScript.DeviceList() != null && bluetoothDeviceScript.DeviceList().Count > 0) {
                busy = true;
                bluetoothLEHardwareInterface.DisconnectPeripheral(bluetoothDeviceScript.DeviceList()[0], DisconnectDeviceComplete);
            }
            else {
                OnDisconnected();
            }
        }

        public void Abort ()
        {
            StopScan();
            Disconnect();
        }

        /*
        ------------------------------------ 
        READ/WRITE:
        ------------------------------------ 
        */
        public void ReadStepsAndExercizeTime() {
            steps.Clear();
            numRetries = 0;

            if (IsInitialized() && HasProfile() && IsConnected() && HasDiscoveredDevice() && !IsBusy())
            {
                busy = true;
                bluetoothLEHardwareInterface.ReadCharacteristic(bluetoothDeviceScript.DeviceList()[0], connectedWristbandProfile.id, getCharacteristicByTag(WristbandCharacteristicTag.B).id, ReadCharacteristicsHandler);
            }
            else
                OnERROR(WristbandProtocolError.READ_STEPS_FAILED, "ReadStepsAndExercizeTime :: couldn't read steps -> possible reason(s): " + GetUnableToReadError(true, true, true, true, true, false));
        }

        public void SetClockAndUserSettings (byte [] data)
        {
            if (IsInitialized() && HasProfile() && IsConnected() && HasDiscoveredDevice() && !IsBusy())
            {
                busy = true;
                bluetoothLEHardwareInterface.WriteCharacteristic(bluetoothDeviceScript.DeviceList()[0], connectedWristbandProfile.id, getCharacteristicByTag(WristbandCharacteristicTag.A).id, data, data.Length, true, WriteCharacteristicHandler);
            }
            else
                OnERROR(WristbandProtocolError.SET_WRISTBAND_FAILED, "SetClockAndUserSettings :: couldn't write settings -> possible reason(s): " + GetUnableToReadError(true, true, true, true, true, false));
        }

        public void ClearHistory ()
        {
            if (IsInitialized() && HasProfile() && IsConnected() && HasDiscoveredDevice() && HasHistory() && !IsBusy())
            {
                busy = true;
                byte[] data = new byte[1] { 0xA1 };   
                bluetoothLEHardwareInterface.WriteCharacteristic(bluetoothDeviceScript.DeviceList()[0], connectedWristbandProfile.id, getCharacteristicByTag(WristbandCharacteristicTag.A).id, data, data.Length, true, WriteCharacteristicHandler);
            }
            else
                OnERROR(WristbandProtocolError.CLEAR_HISTORY_FAILED, "ClearHistory :: couldn't clear history -> possible reason(s): " + GetUnableToReadError(true, true, true, true, true, true));
        }

        /*
        ==================================== 
        RESPONSE HANDLERS:
        ====================================
        
        ------------------------------------
        READ ALL STEPS
        ------------------------------------
        */
        
        private void ReadCharacteristicsHandler(string str, byte[] dat)
        {
            steps.Clear();
            steps.setTodayData(dat);

            days_returned = 0;

            if (steps.stepsToday() > 10000000 && numRetries < 10)
            {
                steps.Clear();
                //OnERROR(WristbandProtocolError.INVALID_NUMBER_OF_STEPS, "ReadCharacteristicsHandler :: Invalid number of steps recieved from wristband: " + steps.stepsToday());
                OnERROR(WristbandProtocolError.INVALID_NUMBER_OF_STEPS, "ReadCharacteristicsHandler :: Invalid number of steps recieved from wristband -> All steps cleared!" );

                // TODO: Delay of one second proved to work better in the older version of the sync app
                // currently removed because i didn't want to start multithreading just yet.
                // so at least worth testing.

                /*
                MonoBehaviour mb = new MonoBehaviour();
                new MonoBehaviour().StartCoroutine(WaitForSecs(0.1f, () => {
                    numRetries++;
                    bluetoothLEHardwareInterface.ReadCharacteristic(pheriperalID, connectedWristbandProfile.id, "0000FED2-494C-4F47-4943-544543480000", ReadCharacteristicsHandler);
                }));
                */

                //wait for 100 miliseconds
                /*
                GameManager.Instance.Wait(0.1f, () =>
                {
                    //retry retrieving steps from wristband
                    numRetries++;
                    bluetoothLEHardwareInterface.ReadCharacteristic(pheriperalID, connectedWristbandProfile.id, "0000FED2-494C-4F47-4943-544543480000", ReadCharacteristicsHandler);
                });
                */

                numRetries++;
                bluetoothLEHardwareInterface.ReadCharacteristic(pheriperalID, connectedWristbandProfile.id, "0000FED2-494C-4F47-4943-544543480000", ReadCharacteristicsHandler);

            }
            else if (steps.stepsToday() > 10000000)
            {
                busy = false;
                OnERROR(WristbandProtocolError.INVALID_NUMBER_OF_STEPS, "ReadCharacteristicsHandler :: maximum number of retries on 'invalid number of steps' reached, loop interrupted!" + steps.stepsToday()+" steps");
            }
            else
            {
                OnDebugMSG("ReadCharacteristicsHandler :: Start recursive history retrieval");
                RecursiveStepsRequest("0000FED1-494C-4F47-4943-544543480000");
            }
        }

        /*
        ------------------------------------
        NOTIFICATION HANDLERS:
        ------------------------------------
        */
        private void RecursiveStepsRequest(string characteristic)
        {
            OnDebugMSG("RecursiveStepsRequest :: day " + days_returned.ToString() + "/" + steps.nrOfStepsRecords());

            if (days_returned < steps.nrOfStepsRecords())
            {
                // If there are history records and the requested day is smaller that the amount of history records
                byte[] data = new byte[1] { System.Convert.ToByte(days_returned) };
                bluetoothLEHardwareInterface.WriteCharacteristic(
                    bluetoothDeviceScript.DeviceList()[0], 
                    connectedWristbandProfile.id, 
                    characteristic, 
                    data, 
                    data.Length, 
                    true, 
                    WriteCharacteristicResponse
                );
            }
            else
            {
                // If this is not the firsttime and we have looped through all history records
                busy = false;
                history_retieved = true;
                OnStepsCollected(steps);
            }
        }

        private void NotificationUpdateHandler(string str, byte[] dat)
        {
            steps.setHistoryData(days_returned, dat);
            days_returned++;
            RecursiveStepsRequest(str);
        }

        private void NotificationHandler(string str)
        {
            OnDebugMSG("NotificationHandler :: Subscribtion successfull @:" + str);

            connectedWristbandProfile = services.GetProfileByName(pheriperalName);
            //if (OnMatchingProfile != null) OnMatchingProfile(profile);

            if (connectedWristbandProfile != null) 
                OnConnected(pheriperalID, pheriperalName, connectedWristbandProfile);
            else
            {
                OnERROR(WristbandProtocolError.NOTIFICATION_SUBSCRTIPTION_FAILED, "NotificationHandler :: unknown profile -> device-id=" + pheriperalID + ", device-name=" + pheriperalName);
                OnUnknownProfile(pheriperalID, pheriperalName);
            }
        }

        private void WriteCharacteristicResponse (string str)
        {
            OnDebugMSG("OrganixGATTprotocol.WriteCharacteristicRespose :: request successfull @:" + str);
        }

        private void WriteCharacteristicHandler(string str)
        {
            busy = false;
            OnWriteComplete();
        }

        private void UnsubscribeCompleteHandler (string str)
        {
            busy = false;
            OnStepsCollected(steps);
        }

        /*
        ------------------------------------
        INIT DEVICE HANDLERS
        ------------------------------------
        */
        private void DeInitCompleteHandler()
        {
            busy = false;
        }

        private void InitCompleteHandler()
        {
            busy = false;
            OnInitComplete();
        }

        private void InitErrorHandler(string error)
        {
            busy = false;
            OnERROR(WristbandProtocolError.INITIALIZATION_FAILED, error);
        }

        private void ScanForPeripheralsWithServicesResponseHandler(string id, string name)
        {
            if (name == "PR102" || name == "B002")
            {
                pheriperalID = id;
                pheriperalName = name;
                bluetoothLEHardwareInterface.ConnectToPeripheral(id, ConnectToPeripheralConnectHandler, ConnectToPeripheralServiceHandler, ConnectToPeripheralCharacteristicHandler);

                device_found = true;
                this.OnMatchingProfile(id, name);
            }
            else
            {
                OnERROR(WristbandProtocolError.UNKNOWN_DEVICE_DETECTED, "Peripheral found but has the wrong -> ID: " + id + ", name: " + name);
            }
        }

        private void RetrieveListOfPeripheralsWithServicesResponseHandler(string id, string name)
        {
            pheriperalID = id;
            pheriperalName = name;
            busy = false;
        }

        private void ConnectToPeripheralConnectHandler(string device_id)
        {
            device_connected = true;
        }

        private void ConnectToPeripheralServiceHandler(string device_id, string service_id) { }

        private void ConnectToPeripheralCharacteristicHandler(string device_id, string service_id, string characteristic_id)
        {
            string target_service_id = "0000FED0-494C-4F47-4943-544543480000";
            string target_characteristic_id = "0000FED1-494C-4F47-4943-544543480000";

            if (device_id == pheriperalID && service_id.ToUpper() == target_service_id && characteristic_id.ToUpper() == target_characteristic_id)
            {
                bluetoothLEHardwareInterface.SubscribeCharacteristic(pheriperalID, target_service_id, target_characteristic_id, NotificationHandler, NotificationUpdateHandler);
            }
        }

        private void DisconnectDeviceComplete(string obj)
        {
            device_found = false;
            device_connected = false;
            history_retieved = false;
            busy = false;

            OnDisconnected();
        }

        /*
        ==================================== 
        HELPER METHODES:
        ==================================== 
        */
        private string GetUnableToReadError (bool needNotBeBusy, bool needsInit, bool needsProfile, bool needsDiscovered, bool needsConnection, bool needsHistory)
        {
            string output = "";

            if (needNotBeBusy && IsBusy()) output += "Is busy at the moment!";
            if (needsInit && !IsInitialized()) output += "Not initialized! ";
            if (needsProfile && !HasProfile()) output += "Has no matching profile! ";
            if (needsDiscovered && !HasDiscoveredDevice()) output += "Device is not discovered! ";
            if (needsConnection && !IsConnected()) output += "Device is not connected! ";
            if (needsHistory && !HasHistory()) output += "No available history! ";

            return output;
        }

        private WristbandCharacteristic getCharacteristicByTag (WristbandCharacteristicTag tag)
        {
            return services.GetCharacteristicByTag(connectedWristbandProfile.peripheralName, tag);
        }

        static string ToHex(byte[] bytes)
        {
            char[] c = new char[(bytes.Length * 3) + 1];
            c[0] = (char)('|');

            byte b;

            for (int bx = 0, cx = 1; bx < bytes.Length; ++bx, ++cx)
            {
                b = ((byte)(bytes[bx] >> 4));
                c[cx] = (char)(b > 9 ? b - 10 + 'A' : b + '0');

                b = ((byte)(bytes[bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b - 10 + 'A' : b + '0');

                c[++cx] = (char)('|');
            }

            return new string(c);
        }
    }
}