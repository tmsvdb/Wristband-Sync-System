using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Wristband {

    public class WristbandManager : SingletonComponent<WristbandManager>, IWristbandObserver {

        // wristband properties
        // ---------------------------------
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
            wristbandProtocol.SubscibeObserver(this);
        }

        public string GenerateShortID() {
            return UnityEngine.Random.Range(0, 16777215).ToString("X6");
        }

        /*
        ====================================
        CREATE WRISTBAND DATA:
        ====================================
        */
        public byte[] CreateNewWristbandSettings() {
            return ByteConverter.CreateNewWristbandSettings (
                IsKids(), 
                userData.StepsTarget(), 
                userData.DistancePerStep(), 
                userData.Weight(), 
                userData.Height(),
                userData.Gender(), 
                userData.DateOfBirth());
        }

        /*
        ====================================
        WRISTBAND COMMANDS:
        ====================================
        */
        public bool WristbandIsConnected () {
            return wristbandProtocol.IsConnected();
        }

        public void InitializeWristband() {
            wristbandProtocol.InitialyzeCentral();
        }

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

        public StepsData GetStepsData {
            get { return wristbandProtocol.GetSteps; }
        }

        /*
        ====================================
        WRISTBAND PROTOCOL EVENT HANDLERS:
        ====================================
        */
        public void OnBluetoothInitialized() {
            throw new NotImplementedException();
        }

        public void OnBluetoothDeInitialized() {
            throw new NotImplementedException();
        }

        public void OnError(WristbandProtocolError error, string info) {
            throw new NotImplementedException();
        }

        public void OnWristbandDebugMessage(string msg) {
            throw new NotImplementedException();
        }

        public void OnWristbandConnected(string pheriperalID, string pheriperalName, WristbandProfile profile) {
            throw new NotImplementedException();
        }

        public void OnWristbandDisconnected() {
            throw new NotImplementedException();
        }

        public void OnWristbandProfileFound(string pheriperalID, string pheriperalName) {
            throw new NotImplementedException();
        }

        public void OnWristbandUnknownProfile(string pheriperalID, string pheriperalName) {
            throw new NotImplementedException();
        }

        public void OnWristbandStepsCollected(StepsData stepsData) {
            throw new NotImplementedException();
        }

        public void OnWristbandWriteComplete() {
            throw new NotImplementedException();
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