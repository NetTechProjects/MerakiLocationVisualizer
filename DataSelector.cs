using System;
using System.Collections.Generic;

namespace MerakiLocationVisualizer
{
    internal static class DataSelector
    {
        static public List<List<int>> currentRecord { get; set; }
        static public int currentAP { get; set; }

        static public bool InitData(int width, int height)
        {
            string[] apMacs = DBConnection.RetrieveAPs();

            if (apMacs[0] == "none")
            {
                VerticeData.NumberOfAPs = 0;
            } 
            else
            {
                VerticeData.NumberOfAPs = apMacs.Length;
            }

            DataSelector.currentAP = 0;

            List<List<string>> observationReports = new List<List<string>>();

            for (int i = 0; i < apMacs.Length; i++)
            {
                observationReports.Add(DBConnection.RetrieveReports(apMacs[i]));
            }

            if (observationReports.Count < 1)
            {
                return false;
            }

            DataSelector.currentRecord = new List<List<int>>();
            for (int x = 0; x < observationReports.Count; x++)
            {
                DataSelector.currentRecord.Add(new List<int> { 0 });
            }

            float SCALE = 2.0f;

            VerticeData.NormalizedReports = DataTransform.NormalizeCoordinates(observationReports, width / SCALE, height / SCALE);

            VerticeData.VerticeSlice = DataSelector.FetchNewerRecords(VerticeData.NormalizedReports, DataSelector.currentAP);

            VerticeData.SquareVertices = DataTransform.GenerateQuads(VerticeData.VerticeSlice, width, height);

            return true;
        }

        static public float[] FetchNewerRecords(List<List<string>> records, int selectedAP)
        {
            List<float> newerRecords = new List<float>();

            int startIndex = DataSelector.currentRecord[selectedAP][0];
            if (startIndex == 0)
            {
                startIndex = records[selectedAP].Count;
            }
            int timeMin = Convert.ToInt32(records[selectedAP][startIndex - 1]);
            int timeMax = timeMin + 60;

            for (int x = startIndex; x > 0; x -= 4)
            {
                if (timeMax > Convert.ToInt32(records[selectedAP][x-1]) &&
                    Convert.ToInt32(records[selectedAP][x-1]) >= timeMin)
                {
                    newerRecords.Add(Convert.ToSingle(records[selectedAP][x - 4]));
                    newerRecords.Add(Convert.ToSingle(records[selectedAP][x - 3]));
                    DataSelector.currentRecord[selectedAP][0] = x - 4;
                }
            }

            if (newerRecords.Count == 0)  // wrap around
            {
                DataSelector.currentRecord[selectedAP][0] = records[selectedAP].Count;
                for (int x = 0; x < (records[selectedAP].Count - 1); x += 4)
                {
                    newerRecords.Add(Convert.ToSingle(records[selectedAP][x]));
                    newerRecords.Add(Convert.ToSingle(records[selectedAP][x + 1]));
                }
            }

            return newerRecords.ToArray();
        }

        static public float[] FetchOlderRecords(List<List<string>> records, int selectedAP)
        {
            List<float> olderRecords = new List<float>();

            int startIndex = DataSelector.currentRecord[selectedAP][0];
            if (startIndex == records[selectedAP].Count)
            {
                startIndex = 0;
            }
            int timeMax = Convert.ToInt32(records[selectedAP][startIndex + 3]);
            int timeMin = timeMax - 60;

            for (int x = startIndex; x < records[selectedAP].Count; x += 4)
            {
                if (timeMax >= Convert.ToInt32(records[selectedAP][x + 3]) &&
                    Convert.ToInt32(records[selectedAP][x + 3]) > timeMin)
                {
                    olderRecords.Add(Convert.ToSingle(records[selectedAP][x]));
                    olderRecords.Add(Convert.ToSingle(records[selectedAP][x + 1]));
                    DataSelector.currentRecord[selectedAP][0] = x + 4;
                }
            }

            if (olderRecords.Count == 0)  // wrap around
            {
                DataSelector.currentRecord[selectedAP][0] = records[selectedAP].Count;
                for (int x = 0; x < (records[selectedAP].Count - 1); x += 4)
                {
                    olderRecords.Add(Convert.ToSingle(records[selectedAP][x]));
                    olderRecords.Add(Convert.ToSingle(records[selectedAP][x + 1]));
                }
            }

            return olderRecords.ToArray();
        }

    }
}
