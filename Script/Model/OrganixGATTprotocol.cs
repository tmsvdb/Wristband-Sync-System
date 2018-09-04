using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WristbandProtocol
{
    private BluetoothDeviceScript bluetoothDeviceScript;
    private OrganixGATTProfile profile;
    private StepsData steps = new StepsData();

    private int days_returned = 0;
    private int numRetries = 0;
    private string pheriperalID;
    private string pheriperalName;

    private bool device_found = false;
    private bool device_connected = false;
    private bool history_retieved = false;
    private bool busy = false;

    /*
    ==================================== 
    CUSTOM EVENTS:
    ==================================== 
    */

    public WistbandEventManager eventManager;

    public WristbandProtocol () {
        eventManager = new WistbandEventManager ();
    }

/*
    private event Action OnInitComplete;
    private event Action<string> OnERROR;
    private event Action<string> OnDebugMSG;
    private event Action<string, string, OrganixGATTProfile> OnConnected;
    private event Action OnDisconnected;
    private event Action<string, string> OnMatchingProfile;
    private event Action<string, string> OnUnknownProfile;
    private event Action<StepsData> OnStepsCollected;
    //private event Action OnNotificationSubscribed;
    private event Action OnWriteComplete;
*/
    /*
    ==================================== 
    PUBLIC INTERFACE:
    ==================================== 
    
    ------------------------------------
    GET STATES:
    ------------------------------------ 
    */
   
    public bool IsInitialized () { return (bluetoothDeviceScript != null); }

    public bool HasProfile () { return (profile != null); }

    public bool HasDiscoveredDevice() { return (device_found == true); }

    public bool IsConnected() { return (device_connected == true); }

    public bool HasHistory() { return (history_retieved == true); }

    public bool IsBusy() { return (busy == true); }

    /*
    ------------------------------------ 
    INIT/CONNECT:
    ------------------------------------ 
    */

    public void RebootCentral()
    {
        Abort(() =>
        {
            BluetoothLEHardwareInterface.DeInitialize(() => 
            {
                GameManager.Instance.Wait(1, () =>
                {
                    bluetoothDeviceScript = null;
                    InitialyzeCentral();
                });
            });
        });
    }

    public void InitialyzeCentral ()
    {
        if (bluetoothDeviceScript == null)
            bluetoothDeviceScript = BluetoothLEHardwareInterface.Initialize(true, false, InitCompleteHandler, InitErrorHandler);
        else
            InitCompleteHandler();
    }

    public void ScanForDeviceAndAutoConnect ()
    {
        device_found = false;
        device_connected = false;
        history_retieved = false;

        if (IsInitialized()) {
            busy = true;
            string[] servicesToScanFor = OrganixGATTServicesManager.Instance.ListOfProfileServicesIDs();
            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(servicesToScanFor, ScanForPeripheralsWithServicesResponseHandler);
        }
        else {
            eventManager.OnERROR("Bluetooth Central Error, Rebooting Central...");
        }
    }

    public void StopScan ()
    {
        if (IsInitialized() && IsBusy()) {
            busy = false;
            BluetoothLEHardwareInterface.StopScan();
        }
    }

    public void Disconnect()
    {
        if (bluetoothDeviceScript != null && bluetoothDeviceScript.DiscoveredDeviceList != null && bluetoothDeviceScript.DiscoveredDeviceList.Count > 0) {
            busy = true;
            BluetoothLEHardwareInterface.DisconnectPeripheral(bluetoothDeviceScript.DiscoveredDeviceList[0], DisconnectDeviceComplete);
        }
        else {
            onDisconnected();
        }
    }

    public void Abort (Action abortCompleteCallback)
    {
        StopScan();
        Disconnect(abortCompleteCallback);
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
            BluetoothLEHardwareInterface.ReadCharacteristic(bluetoothDeviceScript.DiscoveredDeviceList[0], profile.id, getCharacteristicByTag(OrganixGATTCharacteristicTag.B).id, ReadCharacteristicsHandler);
        }
        else
            OnERROR("ReadStepsAndExercizeTime :: couldn't read steps -> possible reason(s): " + GetUnableToReadError(true, true, true, true, true, false));
    }

    public void SetClockAndUserSettings (byte [] data)
    {
        if (IsInitialized() && HasProfile() && IsConnected() && HasDiscoveredDevice() && !IsBusy())
        {
            busy = true;
            BluetoothLEHardwareInterface.WriteCharacteristic(bluetoothDeviceScript.DiscoveredDeviceList[0], profile.id, getCharacteristicByTag(OrganixGATTCharacteristicTag.A).id, data, data.Length, true, WriteCharacteristicHandler);
        }
        else
            OnERROR("SetClockAndUserSettings :: couldn't write settings -> possible reason(s): " + GetUnableToReadError(true, true, true, true, true, false));
    }

    public void ClearHistory ()
    {
        if (IsInitialized() && HasProfile() && IsConnected() && HasDiscoveredDevice() && HasHistory() && !IsBusy())
        {
            busy = true;
            byte[] data = new byte[1] { 0xA1 };   
            BluetoothLEHardwareInterface.WriteCharacteristic(bluetoothDeviceScript.DiscoveredDeviceList[0], profile.id, getCharacteristicByTag(OrganixGATTCharacteristicTag.A).id, data, data.Length, true, WriteCharacteristicHandler);
        }
        else
            OnERROR("ClearHistory :: couldn't clear history -> possible reason(s): " + GetUnableToReadError(true, true, true, true, true, true));
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
            OnERROR("ReadCharacteristicsHandler :: Invalid number of steps recieved from wristband: " + steps.stepsToday());

            //wait for 100 miliseconds
            GameManager.Instance.Wait(0.1f, () =>
            {
                //retry retrieving steps from wristband
                numRetries++;
                BluetoothLEHardwareInterface.ReadCharacteristic(pheriperalID, profile.id, "0000FED2-494C-4F47-4943-544543480000", ReadCharacteristicsHandler);
            });
        }
        else if (steps.stepsToday() > 10000000)
        {
            busy = false;
            OnERROR("ReadCharacteristicsHandler :: maximum number of retries on 'invalid number of steps' reached, loop interrupted!" + steps.stepsToday()+" steps");
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
			BluetoothLEHardwareInterface.WriteCharacteristic(bluetoothDeviceScript.DiscoveredDeviceList[0], profile.id, characteristic, data, data.Length, true, WriteCharacteristicResponse);
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

        profile = OrganixGATTServicesManager.Instance.GetProfileByName(pheriperalName);
        //if (OnMatchingProfile != null) OnMatchingProfile(profile);

        if (profile != null) OnConnected(pheriperalID, pheriperalName, profile);
        else
        {
            OnERROR("NotificationHandler :: unknown profile -> device-id=" + pheriperalID + ", device-name=" + pheriperalName);
            OnUnknownProfile(pheriperalID, pheriperalName);
        }
    }

	private void WriteCharacteristicResponse (string str)
	{
		AtlasServicesManager.Instance.logger.Log("OrganixGATTprotocol.WriteCharacteristicRespose :: request successfull @:" + str, Severity.debug);
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
        OnERROR(error);
    }

    private void ScanForPeripheralsWithServicesResponseHandler(string id, string name)
    {
        if (name == "PR102" || name == "B002")
        {
            pheriperalID = id;
            pheriperalName = name;
            BluetoothLEHardwareInterface.ConnectToPeripheral(id, ConnectToPeripheralConnectHandler, ConnectToPeripheralServiceHandler, ConnectToPeripheralCharacteristicHandler);

            device_found = true;
            this.OnMatchingProfile(id, name);
        }
        else
        {
            OnERROR("Peripheral found but has the wrong -> ID: " + id + ", name: " + name);
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
            BluetoothLEHardwareInterface.SubscribeCharacteristic(pheriperalID, target_service_id, target_characteristic_id, NotificationHandler, NotificationUpdateHandler);
        }
    }

    private void DisconnectDeviceComplete(string obj)
    {
        device_found = false;
        device_connected = false;
        history_retieved = false;
        busy = false;

        if(OnDisconnected != null) OnDisconnected();
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
        if (needsHistory && !HasHistory()) output += "No availeble history! ";

        return output;
    }

    private bool checkBool(bool obj, string msg)
    {
        if (obj) return true;
        OnERROR(msg);
        return false;
    }
    private bool checkObject(object obj, string msg)
    {
        if (obj != null) return true;
        OnERROR(msg);
        return false;
    }

    private OrganixGATTCharacteristic getCharacteristicByTag(OrganixGATTCharacteristicTag characteristicTag)
    {
        foreach (OrganixGATTCharacteristic characteristic in profile.characteristics)
            if (characteristic.tag == characteristicTag) return characteristic;

        return null;
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



///////////////////////////// TODO: use observer pattern //////////////////////////////


public enum WristbandEventTypes { 
    ERROR, 
    DEBUG_MSG, 
    INIT_COMPLETE, 
    CONNECTED, 
    DISCONNECTED, 
    MATCHING_PROFILE_FOUND, 
    UNKNOWN_PROFILE, 
    STEPS_COLLECTED, 
    WRITE_COMPLETE 
};

public class WristbandMessageEvent {
    public WristbandEventTypes EventType { get; private set; }
    public string EventDescription { get; private set; }
    public OrganixGATTProfile CurrentProfile { get; private set; }
    public StepsData StepsData { get; private set; }

    public WristbandMessageEvent (WristbandEventTypes eventType, string eventDescription, OrganixGATTProfile currentProfile, StepsData stepsData) {
        this.EventType = eventType;
        this.EventDescription = eventDescription;
        this.CurrentProfile = currentProfile;
        this.StepsData = stepsData;
    }
}

public delegate void WristbandEventHandler (WristbandMessageEvent e);

public class WistbandEventManager {

    public event WristbandEventHandler OnWristbandEvent;

    private WristbandProtocol wristbandProtocol;

    public WistbandEventManager (WristbandProtocol wristbandProtocol) {
        this.wristbandProtocol = wristbandProtocol;
    }

    /*
        WRISTBAND EVENTS
    */
    private void  OnInitComplete () {
        OnWristbandEvent(GenerateWristbandEvent(WristbandEventTypes.INIT_COMPLETE, ""));
    }
    private void OnERROR (string msg) {
        OnWristbandEvent(GenerateWristbandEvent(WristbandEventTypes.ERROR, msg));
    }
    private void OnDebugMSG (string msg) {
        OnWristbandEvent(GenerateWristbandEvent(WristbandEventTypes.DEBUG_MSG, msg));
    }
    private void OnConnected (string, string, OrganixGATTProfile) {
        OnWristbandEvent(GenerateWristbandEvent(WristbandEventTypes.CONNECTED, ""));
    }
    private void OnDisconnected () {
        OnWristbandEvent(GenerateWristbandEvent(WristbandEventTypes.DISCONNECTED, ""));
    }
    private void OnMatchingProfile (string, string) {
        OnWristbandEvent(GenerateWristbandEvent(WristbandEventTypes.MATCHING_PROFILE_FOUND, ""));
    }
    private void OnUnknownProfile (string pheriperalID, string pheriperalName) {
        OnWristbandEvent(GenerateWristbandEvent(WristbandEventTypes.UNKNOWN_PROFILE,));
    }
    private void OnStepsCollected (StepsData stepsData) {
        OnWristbandEvent(GenerateWristbandEvent(WristbandEventTypes.STEPS_COLLECTED, ""));
    }
    private void OnWriteComplete () {
        OnWristbandEvent(GenerateWristbandEvent(WristbandEventTypes.WRITE_COMPLETE, ""));
    }

    private WristbandMessageEvent GenerateWristbandEvent (WristbandEventTypes type, string msg) {
        return new WristbandMessageEvent(type, msg, this.profile, this.steps);
    }
}
