using System.Collections;
using System.Collections.Generic;
using System;

namespace Wristband {

    public class StepsData
    {
        private byte[] todayBytes;
        private List<byte[]> historyBytes = new List<byte[]>();

        public StepsData ()
        {
            clearHistory();
        }

        /*
        ------------------------------------
        Set Data
        ------------------------------------
        */
        public byte[] getTodayData()
        {
            return (byte[]) todayBytes.Clone();
        }

        public List<byte[]> getHistoryData()
        {
            List<byte[]> output = new List<byte[]>();

            foreach (byte[] data in historyBytes)
                output.Add((byte[])data.Clone());

            return output;
        }

        public void setTodayData(byte[] data)
        {
            this.todayBytes = data;
        }

        public void setHistoryData(int index, byte[] data)
        {
            historyBytes.Add(data);
        }

        public void Clear()
        {
            todayBytes = null;
            clearHistory();
        }

        /*
        ------------------------------------
        Get Total Data
        ------------------------------------
        */

        public int totalStepsWalked()
        {
            int total = stepsToday();
            for (int i = 0; i < historyBytes.Count; i++)
            {
                int steps = stepsHistory(i);
                total += steps == -1 ? 0 : steps;
            }

            return total;
        }

        public int totalExerciseTime()
        {
            int total = exerciseTimeToday();
            for (int i = 0; i < historyBytes.Count; i++)
            {
                int xt = exerciseTimeHistory(i);
                total += xt == -1 ? 0 : xt;
            }

            return total;
        }

        public StepsDayType[] stepsHistoryList ()
        {
            List<StepsDayType> returnHistoryList = new List<StepsDayType>();

            returnHistoryList.Add(new StepsDayType (stepsToday(), TimeDateStepsTools.ParseToday(), exerciseTimeToday()));

            for (int i = 0; i < 30; i++)
            {
                if (i < historyBytes.Count)
                    returnHistoryList.Add(new StepsDayType(stepsHistory(i), TimeDateStepsTools.ParseDate(i+1), exerciseTimeHistory(i)));
                else
                    returnHistoryList.Add(new StepsDayType(0, TimeDateStepsTools.ParseDate(i + 1), 0));
            }

            return returnHistoryList.ToArray();
        }



        /*
        ------------------------------------
        Get Today Data
        ------------------------------------
        */

        public int stepsToday()
        {
            if (noToday(2)) return 0;

            return (todayBytes[2] << 16) + (todayBytes[1] << 8) + todayBytes[0];
        }

        public int exerciseTimeToday()
        {
            if (noToday(4)) return 0;

            return (todayBytes[4] << 8) + todayBytes[3];
        }

        public int nrOfStepsRecords()
        {
            if (noToday(5)) return 0;

            return todayBytes[5];
        }

        public int nrOfSleepRecords()
        {
            if (noToday(7)) return 0;

            return (todayBytes[6] << 8) + todayBytes[7];
        }

        /*
        ------------------------------------
        Get History Data
        ------------------------------------
        */

        public int daysAgo(int index)
        {
            if (noHistory(index, 0)) return -1;

            return hB(index)[0] & 128;
        }

        public int stepsHistory(int index)
        {
            if (noHistory(index, 3)) return 0;

            return (hB(index)[3] << 16) + (hB(index)[2] << 8) + hB(index)[1];
        }

        public int exerciseTimeHistory(int index)
        {
            if (noHistory(index, 5)) return 0;

            return (hB(index)[5] << 8) + hB(index)[4];
        }

        public int goalStepsHistory(int index)
        {
            if (noHistory(index, 8)) return -1;

            return ((hB(index)[8] & 0x7F) << 16) + (hB(index)[7] << 8) + hB(index)[6];
        }

        public int goalUnitsHistory(int index)
        {
            if (noHistory(index, 8)) return -1;

            return (hB(index)[8] & 0x80);
        }

        public int distanceWalkedHistory(int index)
        {
            if (noHistory(index, 10)) return -1;

            return (hB(index)[10] << 8) + hB(index)[9];
        }

        public int caloriesBurnedPerStepHistory(int index)
        {
            if (noHistory(index, 13)) return -1;

            return ((hB(index)[13] & 0x7F) << 16) + (hB(index)[12] << 8) + hB(index)[11];
        }

        public int distanceUnitsHistory(int index)
        {
            if (noHistory(index, 13)) return -1;

            return (hB(index)[13] & 0x80);
        }

        public int BMRHistory(int index)
        {
            if (noHistory(index, 16)) return -1;

            return (hB(index)[16] << 16) + (hB(index)[15] << 8) + hB(index)[14];
        }

        /*
        ------------------------------------
        Helpers
        ------------------------------------
        */

        private byte[] hB(int index)
        {
            return historyBytes[index];
        }

        private void clearHistory ()
        {
            historyBytes.Clear();
        }

        private bool noToday (int maxBit)
        {
            return (todayBytes == null || todayBytes.Length <= maxBit);
        }

        private bool noHistory (int index, int maxBit)
        {
            return (historyBytes.Count <= index || historyBytes[index] == null || historyBytes[index].Length <= maxBit);
        }

    }
}