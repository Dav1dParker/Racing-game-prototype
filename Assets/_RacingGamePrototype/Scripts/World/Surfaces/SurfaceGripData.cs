using System.Collections.Generic;
using UnityEngine;

namespace _RacingGamePrototype.Scripts.World.Surfaces
{
    [CreateAssetMenu(menuName = "Racing/Surface Grip Data", fileName = "SurfaceGripData")]
    public sealed class SurfaceGripData : ScriptableObject
    {
        [System.Serializable]
        public struct SurfaceEntry
        {
            public SurfaceType type;
            [Range(0f, 2f)] public float gripMultiplier;
            public string tag;
        }

        [SerializeField] private SurfaceEntry[] entries;

        private readonly Dictionary<string, float> _gripMap = new();

        public float GetGripForTag(string tag)
        {
            if (_gripMap.Count == 0)
            {
                foreach (var e in entries)
                    _gripMap[e.tag] = e.gripMultiplier;
            }

            return _gripMap.GetValueOrDefault(tag, 1f);
        }
    }
}