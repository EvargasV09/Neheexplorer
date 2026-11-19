using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nehexplorer
{
    [System.Serializable]
    public class MemoryCardItem
    {
        [Tooltip("ID único que une la pareja (ej: 'Flamenco'). Ambas cartas deben tener el mismo pairId.")]
        public string pairId;

        [Tooltip("Nombre visible en la carta (ej: 'Flamenco Rosado' o 'Lagunas de La Guajira')")]
        public string title;

        [Tooltip("Tipo o descripción corta (ej: 'Especie nativa' o 'Hábitat natural')")]
        public string subtitle;

        [Tooltip("Emoji representativo placeholder mientras se asigna un sprite real")]
        public string emoji = "🦩";

        [Tooltip("Sprite opcional. Si se deja vacío, el juego muestra el emoji y color plano.")]
        public Sprite customSprite;

        [Tooltip("Color de fondo de la carta cuando se voltea")]
        public Color cardColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    }

    [System.Serializable]
    public class MemoryPair
    {
        public string pairId = "Par_1";
        [Header("Elemento A (Especie)")]
        public MemoryCardItem itemA;
        [Header("Elemento B (Hábitat o Ecosistema)")]
        public MemoryCardItem itemB;
    }

    [CreateAssetMenu(fileName = "NuevoSetMemoria", menuName = "Nehexplorer/Set de Memoria Educativo")]
    public class MemoryGameDataSet : ScriptableObject
    {
        [Header("Información del Minijuego")]
        public string minigameTitle = "Memoria del Caribe: Especies y Hábitats";
        [TextArea(2, 3)]
        public string instructions = "Encuentra las parejas relacionando cada animal o planta representativa del Caribe con su hábitat natural.";

        [Header("Dato Educativo al Ganar")]
        public string winFactTitle = "¿Sabías que...?";
        [TextArea(3, 5)]
        public string winEducationalFact = "El Caribe colombiano no solo tiene playas: alberga el Bosque Seco Tropical, uno de los ecosistemas más amenazados del país, donde habita el mono Tití Cabeciblanco, especie que NO vive en ningún otro lugar del planeta.";

        [Header("Parejas Educativas (6 parejas para cuadrícula 4x3)")]
        public List<MemoryPair> pairs = new List<MemoryPair>();
    }
}
