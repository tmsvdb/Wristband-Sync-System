
using System;

namespace Wristband {

    public class ByteConverter {
        /*
        ====================================
        SET WRISTBAND DATA:
        ====================================
        */
        public static byte[] CreateNewWristbandSettings(bool is_kids, int steps_target, int distance_per_step, float weight, float height, string gender, string date_of_birth)
        {
            return ConvertSettingsToByteArray(
                0x80, 
                DateTime.Now.Hour, 
                DateTime.Now.Minute, 
                DateTime.Now.Second, 
                Calculations.GoalSteps(is_kids, steps_target), 
                0x00, 
                distance_per_step, 
                0x80, 
                Calculations.CaloriesPerStep(weight), 
                Calculations.BMR(Age(date_of_birth), weight, height, gender),
                0x80
            );
        }

        private static byte[] ConvertSettingsToByteArray (
            int hour24, 
            int hour, 
            int minute, 
            int second, 
            int goal, 
            int goalUnit, 
            int distance, 
            int distanceUnit, 
            int calorie, 
            int bmr, 
            int over100goal
            )
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

        public static int Age (string date_of_birth)
        {
            System.DateTime bday = System.DateTime.ParseExact(date_of_birth, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            System.DateTime today = System.DateTime.Today;

            int age = today.Year - bday.Year;
            if (bday > today.AddYears(-age))
                age--;

            return age;
        }
    }
}