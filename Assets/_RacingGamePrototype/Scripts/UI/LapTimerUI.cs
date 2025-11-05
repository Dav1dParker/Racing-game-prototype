using UnityEngine;
using TMPro;

namespace _RacingGamePrototype.Scripts.UI
{
    public sealed class LapTimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text currentLapText;
        [SerializeField] private TMP_Text bestLapText;

        private void Update()
        {
            if (!LapSystem.LapManager.Instance) return;
            var manager = LapSystem.LapManager.Instance;

            float current = manager.CurrentLapTime;
            float best = manager.IsReversed
                ? manager.BestLapTimeReverse
                : manager.BestLapTimeForward;

            currentLapText.text = $"Lap: {FormatTime(current)}";
            bestLapText.text = best < Mathf.Infinity
                ? $"Best: {FormatTime(best)}"
                : "Best: --:--.---";
        }

        private static string FormatTime(float t)
        {
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            int ms = Mathf.FloorToInt((t * 1000f) % 1000f);
            return $"{minutes:00}:{seconds:00}.{ms:000}";
        }
    }
}