using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class OrganixServiceEditor : EditorWindow
{
    public OrganixGATTServices services;

    [MenuItem("Goal043/Edit Organix GATT Services Data")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(OrganixServiceEditor));
    }

    void OnEnable()
    {
        services = AssetDatabase.LoadAssetAtPath("Assets/Data/OrganixGATTServices.asset", typeof(OrganixGATTServices)) as OrganixGATTServices;

        if (services == null)
            CreateNewProfileList();
    }

    void OnGUI()
    {
        if (services != null)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh data link to manager"))
            {
                UpdateServiceManager();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Profile", GUILayout.ExpandWidth(true)))
            {
                AddProfile();
            }
            GUILayout.EndHorizontal();
            
            if (services.profiles == null)
                Debug.Log("wtf");

            if (services.profiles.Count > 0)
            {
                List<int> removeProfiles = new List<int>();

                for (int ip = 0; ip < services.profiles.Count; ip++)
                {
                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("description", GUILayout.MaxWidth(145));
                    string[] profileTypes = { "Boosth-Kids", "Boosth-Adults" };
                    services.profiles[ip].description = (OrganixGATTProfileType)EditorGUILayout.Popup((int)services.profiles[ip].description, profileTypes);
                    GUILayout.EndHorizontal();  

                    GUILayout.BeginHorizontal();
                    services.profiles[ip].peripheralName = EditorGUILayout.TextField("Profile peripheralName", services.profiles[ip].peripheralName as string);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    services.profiles[ip].id = EditorGUILayout.TextField("Profile id", services.profiles[ip].id as string);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Remove Profile", GUILayout.ExpandWidth(false)))
                    {
                        removeProfiles.Add(ip);
                    }
                    if (GUILayout.Button("Add Characteristic", GUILayout.ExpandWidth(false)))
                    {
                        services.profiles[ip].characteristics.Add(new OrganixGATTCharacteristic());
                        //UpdateServiceManager();
                    }
                    GUILayout.EndHorizontal();

                    List<int> removeCharacteristics = new List<int>();

                    for (int ic = 0; ic < services.profiles[ip].characteristics.Count; ic++)
                    {
                        OrganixGATTCharacteristic characteristic = services.profiles[ip].characteristics[ic];

                        GUILayout.Space(10);

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("  ", "", GUILayout.MaxWidth(30));
                        characteristic.tag = (OrganixGATTCharacteristicTag)EditorGUILayout.Popup((int)characteristic.tag, new string[] { "user settings & history data", "get activity & set alarm", "phone finder service" });
                            //EditorGUILayout.TextField("Characteristic description", characteristic.tag as string);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("  ", "", GUILayout.MaxWidth(30));
                        characteristic.id = EditorGUILayout.TextField("Characteristic id", characteristic.id as string);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("  ", "", GUILayout.MaxWidth(30));
                        if (GUILayout.Button("remove", GUILayout.ExpandWidth(false)))
                        {
                            removeCharacteristics.Add(ic);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.Space(10);

                    // remove characteristics
                    foreach (int index in removeCharacteristics)
                    {
                        services.profiles[ip].characteristics.RemoveAt(index);
                    }
                }

                // remove profiles
                foreach (int index in removeProfiles)
                {
                    DeleteProfile(index);
                }
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.Label("This Profile List is Empty.");
            }
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(services);
        }
    }

    void CreateNewProfileList()
    {
        services = OrganixGATTServicesCreator.Create();
        services.profiles = new List<OrganixGATTProfile>();

        UpdateServiceManager();
    }


    void AddProfile()
    {
        OrganixGATTProfile newProfile = new OrganixGATTProfile();
        newProfile.characteristics = new List<OrganixGATTCharacteristic>();

        services.profiles.Add(newProfile);
    }

    void DeleteProfile(int index)
    {
        services.profiles.RemoveAt(index);
    }

    void UpdateServiceManager ()
    {
        OrganixGATTServicesManager.Instance.services = services;
    }
}