using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum WristbandError {
    INITIALIZATION_FAILED,
    CONNECTION_FAILED,
    READ_STEPS_FAILED,
    SET_WRISTBAND_FAILED,
    CLEAR_HISTORY_FAILED,
    INVALID_NUMBER_OF_STEPS,
    NOTIFICATION_SUBSCRTIPTION_FAILED,
    UNKNOWN_DEVICE_DETECTED,
}

public interface IWristbandObserver {
    void OnBluetoothInitialized();
    void OnBluetoothDeInitialized();
    void OnError(WristbandError error, string info);
    void OnWristbandDebugMessage (string msg);
    void OnWristbandConnected (string pheriperalID, string pheriperalName, WristbandProfile profile);
    void OnWristbandDisconnected ();
    void OnWristbandProfileFound (string pheriperalID, string pheriperalName);
    void OnWristbandUnknownProfile (string pheriperalID, string pheriperalName);
    void OnWristbandStepsCollected (StepsData stepsData);
    void OnWristbandWriteComplete ();
}

public class WristbandObservable {
    List<IWristbandObserver> observers;

    public void SubscibeObserver (IWristbandObserver observer) {
        observers.Add(observer);
    }

    public void UnsubscibeObserver (IWristbandObserver observer) {
        observers.Remove(observer);
    }

    /*
        WRISTBAND EVENTS
    */
    protected void OnInitComplete () {
        foreach (IWristbandObserver observer in observers) {
            observer.OnBluetoothInitialized();
        }
    }
    protected void OnDeInitComplete () {
        foreach (IWristbandObserver observer in observers) {
            observer.OnBluetoothDeInitialized();
        }
    }
    protected void OnERROR (WristbandError error, string info) {
        foreach (IWristbandObserver observer in observers) {
            observer.OnError(error, info);
        }
    }
    protected void OnDebugMSG (string msg) {
        foreach (IWristbandObserver observer in observers) {
            observer.OnWristbandDebugMessage(msg);
        }
    }
    protected void OnConnected (string pheriperalID, string pheriperalName, WristbandProfile profile) {
        foreach (IWristbandObserver observer in observers) {
            observer.OnWristbandConnected(pheriperalID, pheriperalName, profile);
        }
    }
    protected void OnDisconnected () {
        foreach (IWristbandObserver observer in observers) {
            observer.OnWristbandDisconnected();
        }
    }
    protected void OnMatchingProfile (string pheriperalID, string pheriperalName) {
        foreach (IWristbandObserver observer in observers) {
            observer.OnWristbandProfileFound(pheriperalID, pheriperalName);
        }
    }
    protected void OnUnknownProfile (string pheriperalID, string pheriperalName) {
        foreach (IWristbandObserver observer in observers) {
            observer.OnWristbandUnknownProfile(pheriperalID, pheriperalName);
        }
    }
    protected void OnStepsCollected (StepsData stepsData) {
        foreach (IWristbandObserver observer in observers) {
            observer.OnWristbandStepsCollected(stepsData);
        }
    }
    protected void OnWriteComplete () {
        foreach (IWristbandObserver observer in observers) {
            observer.OnWristbandWriteComplete();
        }
    }
}

public class WristbandProtocol : WristbandObservable
{
    private BluetoothDeviceScript bluetoothDeviceScript;
    private WristbandProfile connectedWristbandProfile;
    private StepsData steps = new StepsData();
    private WristbandServices services;

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

    /*

    ------------------------------------ 
    INIT/CONNECT:
    ------------------------------------ 
    */
    public void InitialyzeCentral ()
    {
        if (bluetoothDeviceScript == null)
            bluetoothDeviceScript = BluetoothLEHardwareInterface.Initialize(true, false, InitCompleteHandler, InitErrorHandler);
        else
            InitCompleteHandler();
    }

    public void DeiIitialyzeCentral ()
    {
        if (IsInitialized()) {
            busy = true;
            BluetoothLEHardwareInterface.DeInitialize(() => { 
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
            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(servicesToScanFor, ScanForPeripheralsWithServicesResponseHandler);
        }
        else {
            OnERROR(WristbandError.CONNECTION_FAILED, "Bluetooth Central Error, Rebooting Central...");
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
        connectedWristbandProfile = null;

        if (bluetoothDeviceScript != null && bluetoothDeviceScript.DiscoveredDeviceList != null && bluetoothDeviceScript.DiscoveredDeviceList.Count > 0) {
            busy = true;
            BluetoothLEHardwareInterface.DisconnectPeripheral(bluetoothDeviceScript.DiscoveredDeviceList[0], DisconnectDeviceComplete);
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
            BluetoothLEHardwareInterface.ReadCharacteristic(bluetoothDeviceScript.DiscoveredDeviceList[0], connectedWristbandProfile.id, getCharacteristicByTag(WristbandCharacteristicTag.B).id, ReadCharacteristicsHandler);
        }
        else
            OnERROR(WristbandError.READ_STEPS_FAILED, "ReadStepsAndExercizeTime :: couldn't read steps -> possible reason(s): " + GetUnableToReadError(true, true, true, true, true, false));
    }

    public void SetClockAndUserSettings (byte [] data)
    {
        if (IsInitialized() && HasProfile() && IsConnected() && HasDiscoveredDevice() && !IsBusy())
        {
            busy = true;
            BluetoothLEHardwareInterface.WriteCharacteristic(bluetoothDeviceScript.DiscoveredDeviceList[0], connectedWristbandProfile.id, getCharacteristicByTag(WristbandCharacteristicTag.A).id, data, data.Length, true, WriteCharacteristicHandler);
        }
        else
            OnERROR(WristbandError.SET_WRISTBAND_FAILED, "SetClockAndUserSettings :: couldn't write settings -> possible reason(s): " + GetUnableToReadError(true, true, true, true, true, false));
    }

    public void ClearHistory ()
    {
        if (IsInitialized() && HasProfile() && IsConnected() && HasDiscoveredDevice() && HasHistory() && !IsBusy())
        {
            busy = true;
            byte[] data = new byte[1] { 0xA1 };   
            BluetoothLEHardwareInterface.WriteCharacteristic(bluetoothDeviceScript.DiscoveredDeviceList[0], connectedWristbandProfile.id, getCharacteristicByTag(WristbandCharacteristicTag.A).id, data, data.Length, true, WriteCharacteristicHandler);
        }
        else
            OnERROR(WristbandError.CLEAR_HISTORY_FAILED, "ClearHistory :: couldn't clear history -> possible reason(s): " + GetUnableToReadError(true, true, true, true, true, true));
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
            OnERROR(WristbandError.INVALID_NUMBER_OF_STEPS, "ReadCharacteristicsHandler :: Invalid number of steps recieved from wristband: " + steps.stepsToday());

            MonoBehaviour mb = new MonoBehaviour();
            new MonoBehaviour().StartCoroutine(WaitForSecs(0.1f, () => {
                numRetries++;
                BluetoothLEHardwareInterface.ReadCharacteristic(pheriperalID, connectedWristbandProfile.id, "0000FED2-494C-4F47-4943-544543480000", ReadCharacteristicsHandler);
            }));

            //wait for 100 miliseconds
            /*
            GameManager.Instance.Wait(0.1f, () =>
            {
                //retry retrieving steps from wristband
                numRetries++;
                BluetoothLEHardwareInterface.ReadCharacteristic(pheriperalID, connectedWristbandProfile.id, "0000FED2-494C-4F47-4943-544543480000", ReadCharacteristicsHandler);
            });
            */
        }
        else if (steps.stepsToday() > 10000000)
        {
            busy = false;
            OnERROR(WristbandError.INVALID_NUMBER_OF_STEPS, "ReadCharacteristicsHandler :: maximum number of retries on 'invalid number of steps' reached, loop interrupted!" + steps.stepsToday()+" steps");
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
			BluetoothLEHardwareInterface.WriteCharacteristic(
                bluetoothDeviceScript.DiscoveredDeviceList[0], 
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

        if (connectedWristbandProfile != null) OnConnected(pheriperalID, pheriperalName, connectedWristbandProfile);
        else
        {
            OnERROR(WristbandError.NOTIFICATION_SUBSCRTIPTION_FAILED, "NotificationHandler :: unknown profile -> device-id=" + pheriperalID + ", device-name=" + pheriperalName);
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
        OnERROR(WristbandError.INITIALIZATION_FAILED, error);
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
            OnERROR(WristbandError.UNKNOWN_DEVICE_DETECTED, "Peripheral found but has the wrong -> ID: " + id + ", name: " + name);
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
        if (needsHistory && !HasHistory()) output += "No availeble history! ";

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

    private IEnumerator WaitForSecs (float secs, Action onComplete)
    {
        yield return new WaitForSeconds(secs);
        onComplete();
    }
}