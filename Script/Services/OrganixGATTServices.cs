using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class OrganixGATTServices : ScriptableObject
{
    public List <OrganixGATTProfile> profiles;
}

[System.Serializable]
public class OrganixGATTProfile
{
    public string peripheralName;
    public string id;
    public OrganixGATTProfileType description;

    public List<OrganixGATTCharacteristic> characteristics;
}

[System.Serializable]
public class OrganixGATTCharacteristic
{
    public OrganixGATTCharacteristicTag tag;
    public string id;
}

public enum OrganixGATTProfileType { Boosth_Kids, Boosth_Adults }

public enum OrganixGATTCharacteristicTag { A = 0, B = 1, C = 2 }
