


namespace Wristband {

    public interface IWristbandServices {
        string[] ListOfProfileServicesIDs ();
        WristbandProfile GetProfileByName(string peripheralName); 
        WristbandCharacteristic GetCharacteristicByTag(string peripheralName, WristbandCharacteristicTag characteristicTag);
    }
}