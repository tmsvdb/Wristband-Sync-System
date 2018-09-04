﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : SingletonComponent<GameManager> {

    // wristband properties
    // ---------------------------------
    private OrganixGATTprotocol wristbandProtocol;

    // runtime data
    // ---------------------------------
    private OrganixGATTProfile _profile;
    private StepsData _steps = new StepsData();
    public string connectedDeviceID;
    public string connectedDeviceName;
    public OrganixGATTProfileType noTrackerStartupProfile = OrganixGATTProfileType.Boosth_Kids;
    public bool noTracker = false;
    public StepsDayType[] LocalStepsHistory;

    /*
    ====================================
    STARTUP SETTINGS:
    ====================================
    */
    void Start() {
        wristbandProtocol = new OrganixGATTprotocol();
    }

    public void Wait (float numSeconds, Action onComplete) {
        StartCoroutine(WaitASec(numSeconds, onComplete));
    }

    private IEnumerator WaitASec (float numSeconds, Action onComplete)
    {
        yield return new WaitForSeconds(numSeconds);
        onComplete();
    }

    public string GenerateShortID()
    {
        return UnityEngine.Random.Range(0, 16777215).ToString("X6");
    }

    /*
    ====================================
    SET WRISTBAND DATA:
    ====================================
    */
    public byte[] CreateNewWristbandSettings()
    {
        bool isKids = (wristbandProfile.description == OrganixGATTProfileType.Boosth_Kids);
        return ConvertSettingsToByteArray(0x80, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, GoalSteps(isKids), 0x00, DistancePerStep(), 0x80, CaloriesPerStep(), BMR(), 0x80);
    }

    private byte[] ConvertSettingsToByteArray (int hour24, int hour, int minute, int second, int goal, int goalUnit, int distance, int distanceUnit, int calorie, int bmr, int over100goal)
    {
        byte[] data = new byte[16];

        data[0] = System.Convert.ToByte(hour | hour24);
        data[1] = System.Convert.ToByte(minute);
        data[2] = System.Convert.ToByte(second);

        data[3] = System.Convert.ToByte(goal & 0xFF);
        data[4] = System.Convert.ToByte((goal >> 8) & 0xFF);
        data[5] = System.Convert.ToByte(((goal >> 16) & 0x7F) | goalUnit);

        data[6] = System.Convert.ToByte(distance & 0xFF);
        data[7] = System.Convert.ToByte((distance >> 8) & 0xFF);

        data[8] = System.Convert.ToByte(calorie & 0xFF);
        data[9] = System.Convert.ToByte((calorie >> 8) & 0xFF);
        data[10] = System.Convert.ToByte((calorie >> 16) & 0xFF);
        data[11] = System.Convert.ToByte(((calorie >> 24) & 0x7F) | distanceUnit);

        data[12] = System.Convert.ToByte(bmr & 0xFF);
        data[13] = System.Convert.ToByte((bmr >> 8) & 0xFF);
        data[14] = System.Convert.ToByte((bmr >> 16) & 0xFF);
        data[15] = System.Convert.ToByte(over100goal);

        return data;
    }

    public int Age ()
    {
        System.DateTime bday = System.DateTime.ParseExact(DateOfBirth(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        System.DateTime today = System.DateTime.Today;

        int age = today.Year - bday.Year;
        if (bday > today.AddYears(-age))
            age--;

        return age;
    }


    /*
    ====================================
    CALCULATIONS:
    ====================================
    */
    public int CaloriesPerStep()
    {
        return Mathf.RoundToInt((Weight() * 693) - 4500);
    }

    public int BMR ()
    {
        int age = Age();
        int maleBMR = Mathf.RoundToInt((float)((10 * Weight ()) + (6.25 * Height ()) - 5 * age - 161) * 100);
        int femaleBMR = Mathf.RoundToInt((float)((10 * Weight ()) + (6.25 * Height ()) - 5 * age + 5) * 100);

        return Gender() == "m" ? maleBMR : femaleBMR;
    }

    public float StepsToCalories(int steps)
    {
        float hour = System.DateTime.Now.Hour;
        float minute = System.DateTime.Now.Minute;

        float calorie = CaloriesPerStep();
        double bmr = BMR();

        float bmrCalorie = (float)((bmr * (hour * 60 + minute)) / 18) * 125;
        float totalCalorie = ((calorie * steps) + bmrCalorie) / 1000000;

        return totalCalorie;
    }

    public int StepsToMeters(int steps) {
        return Mathf.RoundToInt((DistancePerStep() * steps) / 10000f);
    }
  
    public int GoalSteps (bool isKids)
    {
        return isKids ? Mathf.RoundToInt((float)StepsTarget() * 1.167f) : StepsTarget();
    }


    /*
    ====================================
    PLAYER DATA REFERENCES:
    ====================================
    */
    public string DateOfBirth()
    {
        throw new NotImplementedException();
        //return AtlasServicesManager.Instance.users.userData.date_of_birth.Split('T')[0];
    }

    public string Gender () { 
        throw new NotImplementedException();
        //return AtlasServicesManager.Instance.users.userData.gender; 
    }

    public float Weight () { 
        throw new NotImplementedException();
        //return AtlasServicesManager.Instance.boosth.bodyData.weight; 
    }

    public float Height () { 
        throw new NotImplementedException();
        //return AtlasServicesManager.Instance.boosth.bodyData.height; 
    }

    public int DistancePerStep() {
        throw new NotImplementedException();
        //return AtlasServicesManager.Instance.boosth.bodyData.distance_per_step;
    }

    public int StepsTarget() { 
        throw new NotImplementedException();
        //return AtlasServicesManager.Instance.boosth.bodyData.steps_target; 
    }

  
    /*
    ====================================
    WRISTBAND COMMANDS:
    ====================================
    */
    public bool WristbandIsConnected ()
    {
        return wristbandProtocol.IsConnected();
    }

    public void InitializeWristband(Action initComplete, Action<string> onError)
    {
        wristbandProtocol.InitialyzeCentral(initComplete, onError);
    }

    public void RebootBluetoothCentral(Action rebootComplete)
    {
        wristbandProtocol.RebootCentral(rebootComplete);
    }

    public void ConnectToWristband(Action<string, string, OrganixGATTProfile> connectComplete, Action<string, string> onMatchingProfile, Action<string, string> unknownProfile)
    {
        wristbandProtocol.ScanForDeviceAndAutoConnect(connectComplete, onMatchingProfile, unknownProfile);
    }

    public void DisconnectFromWristband(Action disconnectComplete)
    {
        wristbandProtocol.Disconnect(disconnectComplete);
    }

    public void AbortWristband(Action abortCompleteCallback = null)
    {
        wristbandProtocol.Abort(abortCompleteCallback);
    }

    public void StopScan ()
    {
        wristbandProtocol.StopScan();
    }

    public void GetStepsFromWristband(Action<StepsData> getStepComplete)
    {
        wristbandProtocol.ReadStepsAndExercizeTime(getStepComplete);
    }

    public void RemoveHistoryFormWristband(Action removeHistoryComplete)
    {
        wristbandProtocol.ClearHistory(removeHistoryComplete);
    }

    public void WriteWristbandSettings(byte[] data, Action writeComplete, Action<string> errorOverrideCallback = null)
    {
        wristbandProtocol.SetClockAndUserSettings(data, writeComplete, errorOverrideCallback);
    }

    /*
    ====================================
    GAME DATA:
    ====================================
    */
    public OrganixGATTProfile wristbandProfile {
        get { return _profile; }
        set { _profile = value; }
    }

    public StepsData stepsCollected {
        get { return _steps;  }
        set { _steps = value; }
    }

    /*
    ====================================
    HELPERS:
    ====================================
    */
    private int BoolToInt(bool value) { return (value ? 1 : 0); }
    private bool IntToBool(int value) { return (value == 1); }

}
