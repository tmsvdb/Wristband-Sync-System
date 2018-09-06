//using UnityEngine;
using System;
using System.Collections;
using UnityEngine;

namespace Wristband {

    public class Calculations {

        /*
        public static float RoundToNumberOfDigits(float value, int numberOfDigits)
        {
            float mod = (float) Math.Pow(10, numberOfDigits);
            return (float) Math.Round(value * mod) / mod;
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

        */

        public static int CaloriesPerStep(float weight)
        {
            return Mathf.RoundToInt((weight * 693) - 4500);
        }

        public static int BMR (int age, float weight, float height, string gender)
        {
            int maleBMR = Mathf.RoundToInt((float)((10 * weight) + (6.25 * height) - 5 * age - 161) * 100);
            int femaleBMR = Mathf.RoundToInt((float)((10 * weight) + (6.25 * height) - 5 * age + 5) * 100);

            return gender == "m" ? maleBMR : femaleBMR;
        }

        public static float StepsToCalories(int steps, int age, float weight, float height, string gender)
        {
            float hour = System.DateTime.Now.Hour;
            float minute = System.DateTime.Now.Minute;

            float calorie = CaloriesPerStep(weight);
            double bmr = BMR(age, weight, height, gender);

            float bmrCalorie = (float)((bmr * (hour * 60 + minute)) / 18) * 125;
            float totalCalorie = ((calorie * steps) + bmrCalorie) / 1000000;

            return totalCalorie;
        }

        public static int StepsToMeters(int distance_per_step, int steps) {
            return Mathf.RoundToInt((distance_per_step * steps) / 10000f);
        }
    
        public static int GoalSteps (bool isKids, int steps_target)
        {
            return isKids ? Mathf.RoundToInt((float)steps_target * 1.167f) : steps_target;
        }

    }
}