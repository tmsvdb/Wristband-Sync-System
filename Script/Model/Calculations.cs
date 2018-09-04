using UnityEngine;
using System.Collections;

public class Calculations {


    public static float RoundToNumberOfDigits(float value, int numberOfDigits)
    {
        float mod = Mathf.Pow(10, numberOfDigits);
        return Mathf.RoundToInt(value * mod) / mod;
    }

    public static float CalculateBMR(float weightInKG, float heightInCM, int ageInYears, bool isMan)
    {
        return 10 * weightInKG + 6.25f * heightInCM - 5 * ageInYears + (isMan ? 5 : -161);
    }

    public static float CalculateBMRCalories(float bmr, int hours, int minutes)
    {
        return ((bmr * (hours * 60 + minutes)) / 18) * 100;
    }

    public static float CalculateTotalCalories(float calories, int steps, float bmrCalories)
    {
        return ((calories * steps) + bmrCalories) / 1000000;
    }

    public static System.TimeSpan SecondsToTimeSpan (double seconds)
    {
        return System.TimeSpan.FromSeconds(seconds);
    }

}
