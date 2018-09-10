using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Wristband.WristbandManager))]
public class WristbandManagerInspector : Editor 
{
    /*
        TEST DATA
    */
    bool isKids = true;
    int steps_target = 10000;
    int distance_per_step = 500000; // micrometer
    float weight = 50.0f; // kg?
    float height = 1.6f; // meters? 
    string gender = "m"; // ['f','m','o']
    string date_of_birth = "1968-07-03"; // '1968-07-03'

    /*
        Custom inspector
    */
    public override void OnInspectorGUI()
    {
        Wristband.WristbandManager w = (Wristband.WristbandManager)target;

        // poll data
        bool[] states = w.WristbandStates();
        List<Wristband.WristbandHistoryEvent> history = w.EventHistory;
        string[] devices = w.WristbandDeviceList();

        // show wristband states
        EditorGUILayout.LabelField("Wristband States", EditorStyles.boldLabel);
        GUILayout.BeginVertical("GroupBox");
        EditorGUILayout.Toggle("IsInitialized", states[0]);
        EditorGUILayout.Toggle("IsConnected", states[1]);
        EditorGUILayout.Toggle("HasDiscoveredDevice", states[2]);
        EditorGUILayout.Toggle("HasProfile", states[3]);
        EditorGUILayout.Toggle("HasHistory", states[4]);
        EditorGUILayout.Toggle("IsBusy", states[5]);
        GUILayout.EndVertical();

        // controle wristband
        EditorGUILayout.LabelField("Wristband Control", EditorStyles.boldLabel);
        GUILayout.BeginVertical("GroupBox");
        if(GUILayout.Button("Initialize"))
            w.InitialyzeCentral();
        if(GUILayout.Button("DeInitialize"))
            w.DeInitialyzeCentral();
        if(GUILayout.Button("Connect"))
            w.ConnectToWristband();
        if(GUILayout.Button("Disconnect"))
            w.DisconnectFromWristband();
        if(GUILayout.Button("Abort"))
            w.AbortWristband();
        if(GUILayout.Button("StopScan"))
            w.StopScan ();
        if(GUILayout.Button("GetSteps"))
            w.GetStepsFromWristband();
        if(GUILayout.Button("RemoveHistory"))
            w.RemoveHistoryFormWristband();
        GUILayout.EndVertical();

        // write data to wristband
        EditorGUILayout.LabelField("Write Wristband Settings", EditorStyles.boldLabel);
        GUILayout.BeginVertical("GroupBox");
        isKids = EditorGUILayout.Toggle("Is kids", isKids);
        steps_target = EditorGUILayout.IntField("Steps target", steps_target);
        distance_per_step = EditorGUILayout.IntField("Distance per step (in micrometers)", distance_per_step);
        weight = EditorGUILayout.FloatField("Weight (kg?)", weight);
        height = EditorGUILayout.FloatField("Height (meters?)", height);
        gender = EditorGUILayout.TextField("Gender ('m', 'f', 'o')", gender);
        date_of_birth = EditorGUILayout.TextField("Date of birth (yyyy-mm-dd)", date_of_birth);
        if(GUILayout.Button("Write to wristband"))
            w.WriteWristbandSettings(
                w.CreateNewWristbandSettings (
                    isKids,
                    steps_target,
                    distance_per_step,
                    weight,
                    height,
                    gender,
                    date_of_birth
                )
            );
        GUILayout.EndVertical();

        // connected devices
        EditorGUILayout.LabelField("Connected Devices", EditorStyles.boldLabel);
        GUILayout.BeginVertical("GroupBox");
        if (devices == null || devices.Length == 0)
            EditorGUILayout.HelpBox("No devices found", MessageType.Warning);
        else 
            foreach(string device in devices)
                EditorGUILayout.HelpBox(device, MessageType.None);
        GUILayout.EndVertical();

        // show event history
        EditorGUILayout.LabelField("Wristband Event History", EditorStyles.boldLabel);
        GUILayout.BeginVertical("GroupBox");
        if (history == null || history.Count == 0)
            EditorGUILayout.HelpBox("No events in history", MessageType.Warning);
        else {
            int startpos = history.Count > 9 ? history.Count-10 : 0;
            for(int i = startpos; i < history.Count; i++) {
                EditorGUILayout.HelpBox(history[i].msg, history[i].isError ? MessageType.Error : MessageType.Info);
            }
        }
        GUILayout.EndVertical();
    }
}
#endif


namespace Wristband {

    public class WristbandHistoryEvent {
        
        public string msg = "";
        public bool isError = false;

        public WristbandHistoryEvent (string msg, bool isError) {
            this.msg = msg;
            this.isError = isError;
        }
    }

    public class WristbandManager : SingletonComponent<WristbandManager>, IWristbandObserver {

        // wristband properties
        // ---------------------------------
        public List<WristbandHistoryEvent> EventHistory { get; private set; }

        private WristbandProtocol wristbandProtocol;
        private StepsDayType[] LocalStepsHistory;
        private UserData userData = new UserData();

        /*
        ====================================
        STARTUP SETTINGS:
        ====================================
        */
        void Start() {
            wristbandProtocol = new WristbandProtocol(
                new BluetoothLEHardwareInterfaceWrapper(), 
                new BoosthWristbandServices()
            );
            EventHistory = new List<WristbandHistoryEvent>();
            wristbandProtocol.SubscibeObserver(this);
            wristbandProtocol.InitialyzeCentral();
        }

        public string GenerateShortID() {
            return UnityEngine.Random.Range(0, 16777215).ToString("X6");
        }

        /*
        ====================================
        CREATE WRISTBAND DATA:
        ====================================
        */
        public byte[] CreateNewWristbandSettingsFromUserData() {
            return CreateNewWristbandSettings(
                IsKids(),
                userData.StepsTarget(),
                userData.DistancePerStep(),
                userData.Weight(),
                userData.Height(),
                userData.Gender(),
                userData.DateOfBirth()
            ) ;
        }

        public byte[] CreateNewWristbandSettings(
            bool isKids, 
            int steps_target,
            int distance_per_step,
            float weight,
            float height,
            string gender,
            string date_of_birth
        ) {
            return ByteConverter.CreateNewWristbandSettings (
                isKids, 
                steps_target, 
                distance_per_step, 
                weight, 
                height,
                gender, 
                date_of_birth
            );
        }

        /*
        ====================================
        WRISTBAND PROTOCOL COMMANDS:
        ====================================
        */

        // initialize bluetooth interface & wristband protocol
        public void InitialyzeCentral() {
            wristbandProtocol.InitialyzeCentral();
        }

        public void DeInitialyzeCentral() {
            wristbandProtocol.DeInitialyzeCentral();
        }

        // initialize connect to wristband
        public void ConnectToWristband() {
            wristbandProtocol.ScanForDeviceAndAutoConnect();
        }

        public void DisconnectFromWristband() {
            wristbandProtocol.Disconnect();
        }

        public void AbortWristband() {
            wristbandProtocol.Abort();
        }

        public void StopScan () {
            wristbandProtocol.StopScan();
        }

        public void GetStepsFromWristband() {
            wristbandProtocol.ReadStepsAndExercizeTime();
        }

        public void RemoveHistoryFormWristband() {
            wristbandProtocol.ClearHistory();
        }

        public void WriteWristbandSettings(byte[] data) {
            wristbandProtocol.SetClockAndUserSettings(data);
        }

        public StepsData WristbandSteps {
            get { return wristbandProtocol.GetSteps; }
        }

        public string[] WristbandDeviceList() {
            if (wristbandProtocol == null) return new string[0];
            return wristbandProtocol.DeviceList;
        }

        /*
        ====================================
        WRISTBAND PROTOCOL STATES:
        ====================================
        */
        public bool[] WristbandStates () {
            if (wristbandProtocol == null)
                return new bool [6] {false, false, false, false, false, false}; 

            return new bool[6] {
                wristbandProtocol.IsInitialized (),
                wristbandProtocol.IsConnected(),
                wristbandProtocol.HasDiscoveredDevice(),
                wristbandProtocol.HasProfile (),
                wristbandProtocol.HasHistory(),
                wristbandProtocol.IsBusy()
            };
        }

        public bool IsInitialized {
            get { return wristbandProtocol.IsInitialized (); }
        }
        public bool IsConnected {
            get { return wristbandProtocol.IsConnected (); }
        }
        public bool HasDiscoveredDevice {
            get { return wristbandProtocol.HasDiscoveredDevice (); }
        }
        public bool HasProfile {
            get { return wristbandProtocol.HasProfile (); }
        }
        public bool HasHistory {
            get { return wristbandProtocol.HasHistory (); }
        }
        public bool IsBusy {
            get { return wristbandProtocol.IsBusy (); }
        }

        /*
        ====================================
        WRISTBAND PROTOCOL EVENT HANDLERS:
        ====================================
        */
        public void OnBluetoothInitialized() {
            EventHistory.Add(new WristbandHistoryEvent("OnBluetoothInitialized", false));
        }

        public void OnBluetoothDeInitialized() {
            EventHistory.Add(new WristbandHistoryEvent("OnBluetoothDeInitialized", false));
        }

        public void OnError(WristbandProtocolError error, string info) {
            EventHistory.Add(new WristbandHistoryEvent("OnError > " + info, true));
        }

        public void OnWristbandDebugMessage(string msg) {
            EventHistory.Add(new WristbandHistoryEvent("OnWristbandDebugMessage > " + msg, false));
        }

        public void OnWristbandConnected(string pheriperalID, string pheriperalName, WristbandProfile profile) {
            EventHistory.Add(new WristbandHistoryEvent("OnWristbandConnected > name:" + pheriperalName + ", id:" + pheriperalID + ", profile: " + profile.description.ToString(), false));
        }

        public void OnWristbandDisconnected() {
            EventHistory.Add(new WristbandHistoryEvent("OnWristbandDisconnected", false));
        }

        public void OnWristbandProfileFound(string pheriperalID, string pheriperalName) {
            EventHistory.Add(new WristbandHistoryEvent("OnWristbandProfileFound > name: "+ pheriperalName +", id: " + pheriperalID, false));
        }

        public void OnWristbandUnknownProfile(string pheriperalID, string pheriperalName) {
            EventHistory.Add(new WristbandHistoryEvent("OnWristbandUnknownProfile > name: "+ pheriperalName +", id: " + pheriperalID, false));
        }

        public void OnWristbandStepsCollected(StepsData stepsData) {
            EventHistory.Add(new WristbandHistoryEvent("OnWristbandStepsCollected > " + stepsData.totalStepsWalked().ToString(), false));
        }

        public void OnWristbandWriteComplete() {
            EventHistory.Add(new WristbandHistoryEvent("OnWristbandWriteComplete", false));
        }


        /*
        ====================================
        HELPERS:
        ====================================
        */
        private int BoolToInt(bool value) { return (value ? 1 : 0); }
        private bool IntToBool(int value) { return (value == 1); }
        private bool IsKids () { return (wristbandProtocol.ConnectedProfileType == WristbandProfileType.Boosth_Kids); }

    }
}