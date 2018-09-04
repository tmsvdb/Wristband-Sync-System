using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class OrganixGATTServicesManager : SingletonComponent<OrganixGATTServicesManager> {

    public OrganixGATTServices services;


    public string[] ListOfProfileServicesIDs ()
    {
        List<string> output = new List<string>();

        foreach (OrganixGATTProfile profile in services.profiles)
        {
            if (output.IndexOf(profile.id) == -1) output.Add(profile.id);
        }
        return output.ToArray();
    }

    public OrganixGATTProfile GetProfileByName(string peripheralName)
    {
        foreach (OrganixGATTProfile profile in services.profiles)
        {
            if (profile.peripheralName == peripheralName) return profile;
        }
        return null;
    }
    
    public OrganixGATTCharacteristic GetCharacteristicByTag(string peripheralName, OrganixGATTCharacteristicTag characteristicTag)
    {
        OrganixGATTProfile profile = GetProfileByName(peripheralName);

        if (profile == null) return null;

        foreach (OrganixGATTCharacteristic characteristic in profile.characteristics)
        {
            if (characteristic.tag == characteristicTag) return characteristic;
        }
        return null;
    }
}
