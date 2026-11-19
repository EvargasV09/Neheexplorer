using System;
using UnityEngine;

namespace Nehexplorer
{
    public enum RegionType
    {
        Caribe,
        Andina,
        Orinoquia,
        Amazonica,
        Espiritual,
        Pacifica,
        Insular
    }

    [System.Serializable]
    public class RegionInfo
    {
        public RegionType type;
        public string displayName;
        public string subtitle;
        public string ecosystem;
        public string climate;
        public string guardianName;
        [TextArea(2, 4)]
        public string description;
        public string[] minigames;
        public Color themeColor = Color.white;
        public Sprite sliceSprite;
        public Vector2 normalOffset;
        public bool isLocked;
    }
}
