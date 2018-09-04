using UnityEngine;
using System.Collections;
using System;

public class TimeDateStepsTools : MonoBehaviour {

    public static string ParseToday()
    {
        DateTime newDate = DateTime.Today;
        return DateToString(newDate);
    }

    public static string ParseDate(int daysBack)
    {
        DateTime newDate = DateTime.Now.AddDays(-daysBack);
        return DateToString(newDate);
    }

    

    public static StepsDayType GetStepsByDate (string stepsDate, StepsDayType [] stepsHistoryList)
    {
        foreach (StepsDayType stepsDay in stepsHistoryList)
        {
            if (stepsDay.day == stepsDate)
                return stepsDay;
        }

        return null;
    }

    public static int GetIndexFromStepDay(StepsDayType stepsdaytype, StepsDayType[] stepsHistoryList)
    {
        for (int i = 0; i < stepsHistoryList.Length; i++)
        {
            if (stepsdaytype.day == stepsHistoryList[i].day)
                return i;
        }

        return -1;
    }

    /*
        TIMESTAMP TOOLS
    */

    public static string TodayTimeStamp()
    {
        DateTime newDate = DateTime.Today;
        return DateToTimeStamp(newDate);
    }

    /*
        LOCAL TOOLS
    */

    public static string AddZeros(string number, int lenght)
    {
        while (number.Length < lenght) { number = String.Concat("0", number); }
        return number;
    }

    private static string DateToString(DateTime newDate)
    {
        string day = newDate.Day.ToString();
        string month = newDate.Month.ToString();
        string year = newDate.Year.ToString();

        return year + "-" + AddZeros(month, 2) + "-" + AddZeros(day, 2);
    }

    private static string DateToTimeStamp(DateTime newDate)
    {
        string year = newDate.Year.ToString();   

        string day = AddZeros(newDate.Day.ToString(), 2);
        string month = AddZeros(newDate.Month.ToString(), 2);   
        string hour = AddZeros(newDate.Hour.ToString(), 2);
        string min = AddZeros(newDate.Minute.ToString(), 2);
        string sec = AddZeros(newDate.Second.ToString(), 2);

        string mili = AddZeros(newDate.Millisecond.ToString(), 3);

        // EXAMPLE: "2017-05-02T13:21:47.037Z";
        return string.Format("{0}-{1}-{2}T{3}:{4}:{5}.{6}Z", year, month, day, hour, min, sec, mili);
    }
}
