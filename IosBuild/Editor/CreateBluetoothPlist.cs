using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
//using UnityEditor.iOS.Xcode;
using UnityEngine;

public class CreateBluetoothPlist  {
	
#if UNITY_IOS
    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget target, string pathToBuiltProject)
    {
    //public static void ChangeXcodePlist(string pathToBuiltProject)
    //{
        // Get plist
        string plistPath = pathToBuiltProject + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // Get root
        PlistElementDict rootDict = plist.root;

        // background location useage key (new in iOS 8)
        rootDict.SetString("NSBluetoothPeripheralUsageDescription", "Bluetooth connection is required to connect to the Boosth Activity Tracker.");

        // background modes
        /*  PlistElementArray bgModes = rootDict.CreateArray("UIBackgroundModes");
          bgModes.AddString("location");
          bgModes.AddString("fetch");*/

        // Write to file
        File.WriteAllText(plistPath, plist.WriteToString());
        
    }
#endif
}
