using UnityEngine;
using TMPro;
using _RacingGamePrototype.Scripts.LapSystem;

namespace _RacingGamePrototype.Scripts.UI
{
    public sealed class LapTimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text currentLapText;
        [SerializeField] private TMP_Text bestLapText;

        private void Update()
        {
            if (!LapManager.Instance) return;

            float current = LapManager.Instance.CurrentLapTime;
            float best;
            if (LapManager.Instance.IsReversed)  best = LapManager.Instance.BestLapTimeReverse;
            else best = LapManager.Instance.BestLapTimeForward;
            

            currentLapText.text = $"Lap: {FormatTime(current)}";
            bestLapText.text = best < Mathf.Infinity 
                ? $"Best: {FormatTime(best)}" 
                : "Best: --:--.---";
        }

        private static string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);
            return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
        }
    }
}