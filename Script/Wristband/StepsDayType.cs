using System.Collections;
using System.Collections.Generic;

namespace Wristband {

    [System.Serializable]
    public class StepsDayType
    {
        public int steps;           // (int) number of steps walked during this day
        public string day;          // (string) "yyyy-mm-dd"
        public int exercise_time;   // (int) time in minutes

        public int distance;            // v2.5 implementation: distance walked in meters
        public float calories_burned;   // v2.5 implementation: calories burned

        public StepsDayType(int steps = 0, string day = "", int exercise_time = 0, int distance = 0, float calories_burned = 0)
        {
            this.steps = steps;
            this.day = day;
            this.exercise_time = exercise_time;

            this.distance = distance;
            this.calories_burned = calories_burned;
        }

        public string StringFormat () { return "[D" + day + " S" + steps + " T" + exercise_time + " A" + distance + " C" + calories_burned + "]"; }

        public StepsDayType SubtractStepsDay (StepsDayType subtract_stepsDay_from_this)
        {
            steps -= subtract_stepsDay_from_this.steps;
            exercise_time -= subtract_stepsDay_from_this.exercise_time;
            distance -= subtract_stepsDay_from_this.distance;
            calories_burned -= subtract_stepsDay_from_this.calories_burned;
            return this;
        }
    }
}