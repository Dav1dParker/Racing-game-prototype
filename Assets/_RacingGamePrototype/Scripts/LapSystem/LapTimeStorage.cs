using System.IO;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.LapSystem
{
    [System.Serializable]
    public sealed class LapTimeData
    {
        public float bestForward = Mathf.Infinity;
        public float bestReverse = Mathf.Infinity;
    }

    public static class LapTimeStorage
    {
        private static readonly string SavePath =
            Path.Combine(Application.dataPath, "BestLapTimes.json");

        public static void Save(LapTimeData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception e)
            {
                //Debug.LogError($"Failed to save lap times: {e.Message}");
            }
        }

        public static LapTimeData Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    var newData = new LapTimeData();
                    Save(newData);
                    //Debug.Log("BestLapTimes.json created with default values");
                    return newData;
                }

                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<LapTimeData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load lap times: {e.Message}");
                var fallback = new LapTimeData();
                Save(fallback);
                return fallback;
            }
        }

    }
}