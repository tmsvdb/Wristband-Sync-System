using UnityEngine;
using UnityEditor;
using System.Collections;

public class OrganixGATTServicesCreator
{
    //[MenuItem("Goal043/Create Organix GATT Services Data")]
    public static OrganixGATTServices Create()
    {
        OrganixGATTServices asset = ScriptableObject.CreateInstance<OrganixGATTServices>();

        AssetDatabase.CreateAsset(asset, "Assets/Data/OrganixGATTServices.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}