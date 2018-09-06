


namespace Wristband {
    
    public interface IWristbandObserver {
        void OnBluetoothInitialized();
        void OnBluetoothDeInitialized();
        void OnError(WristbandProtocolError error, string info);
        void OnWristbandDebugMessage (string msg);
        void OnWristbandConnected (string pheriperalID, string pheriperalName, WristbandProfile profile);
        void OnWristbandDisconnected ();
        void OnWristbandProfileFound (string pheriperalID, string pheriperalName);
        void OnWristbandUnknownProfile (string pheriperalID, string pheriperalName);
        void OnWristbandStepsCollected (StepsData stepsData);
        void OnWristbandWriteComplete ();
    }
}