//using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
    Profile settings:
    =================

    profiles:
        - peripheralName: PR102
            id: 0000FED0-494C-4F47-4943-544543480000
            description: 0
            characteristics:
                -   tag: 0
                    id: 0000FED1-494C-4F47-4943-544543480000
                -   tag: 1
                    id: 0000FED2-494C-4F47-4943-544543480000
        - peripheralName: B002
            id: 0000FED0-494C-4F47-4943-544543480000
            description: 1
            characteristics:
                -   tag: 0
                    id: 0000FED1-494C-4F47-4943-544543480000
                -   tag: 1
                    id: 0000FED2-494C-4F47-4943-544543480000
*/

namespace Wristband {

    class WristbandServiceException : System.Exception
    {
        public WristbandServiceException(string message)
        {
        
        }

    }

    /*
        Wristband GATT service
    */
    [System.Serializable]
    public class BoosthWristbandServices : IWristbandServices // : ScriptableObject
    {
        private static WristbandProfile KidsProfile = new WristbandProfile(
            "PR102", 
            "0000FED0-494C-4F47-4943-544543480000", 
            WristbandProfileType.Boosth_Kids, 
            new List<WristbandCharacteristic>() {
                new WristbandCharacteristic(WristbandCharacteristicTag.A, "0000FED1-494C-4F47-4943-544543480000"),
                new WristbandCharacteristic(WristbandCharacteristicTag.B, "0000FED2-494C-4F47-4943-544543480000")
            }
        );
        private static WristbandProfile AdultsProfile = new WristbandProfile(
            "B002", 
            "0000FED0-494C-4F47-4943-544543480000", 
            WristbandProfileType.Boosth_Adults, 
            new List<WristbandCharacteristic>() {
                new WristbandCharacteristic(WristbandCharacteristicTag.A, "0000FED1-494C-4F47-4943-544543480000"),
                new WristbandCharacteristic(WristbandCharacteristicTag.B, "0000FED2-494C-4F47-4943-544543480000")
            }
        );

        private List <WristbandProfile> profiles = new List<WristbandProfile>() {
            KidsProfile,
            AdultsProfile
        };

        public string[] ListOfProfileServicesIDs ()
        {
            List<string> output = new List<string>();

            foreach (WristbandProfile profile in profiles)
                if (output.IndexOf(profile.id) == -1) output.Add(profile.id);

            return output.ToArray();
        }

        public WristbandProfile GetProfileByName(string peripheralName)
        {
            foreach (WristbandProfile profile in profiles)
                if (profile.peripheralName == peripheralName) 
                    return profile;

            //Debug.LogError("GetProfileByName ERROR :: profile '"+peripheralName+"' not found!");
            throw new WristbandServiceException ("GetProfileByName ERROR :: profile '"+peripheralName+"' not found!");
            return null;
        }
        
        public WristbandCharacteristic GetCharacteristicByTag(string peripheralName, WristbandCharacteristicTag characteristicTag)
        {
            WristbandProfile profile = GetProfileByName(peripheralName);

            if (profile == null)
                return null;

            foreach (WristbandCharacteristic characteristic in profile.characteristics)
                if (characteristic.tag == characteristicTag)
                    return characteristic;

            //Debug.LogError("GetCharacteristicByTag ERROR :: characteristic tag '"+characteristicTag.ToString()+"' not found!");
            throw new WristbandServiceException ("GetProfileByName ERROR :: profile '"+peripheralName+"' not found!");
            return null;
        }

    }

    /*
        Wristband GATT profile
    */
    [System.Serializable]
    public class WristbandProfile
    {
        public string peripheralName;
        public string id;
        public WristbandProfileType description;

        public List<WristbandCharacteristic> characteristics;

        public WristbandProfile(string peripheralName, string id, WristbandProfileType description, List<WristbandCharacteristic> characteristics)
        {
            this.peripheralName = peripheralName;
            this.id = id;
            this.description = description;
            this.characteristics = characteristics;
        }
    }

    /*
        Wristband GATT Characteristic
    */
    [System.Serializable]
    public class WristbandCharacteristic
    {
        public WristbandCharacteristicTag tag;
        public string id;

        public WristbandCharacteristic(WristbandCharacteristicTag tag, string id)
        {
            this.tag = tag;
            this.id = id;
        }
    }

    /*
        Boosth wristband profile type
    */
    public enum WristbandProfileType { Boosth_Kids, Boosth_Adults }

    /*
        Boosth wristband characteristic tag
    */
    public enum WristbandCharacteristicTag { A = 0, B = 1, C = 2 }

}