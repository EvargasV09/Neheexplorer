using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nehexplorer
{
    [System.Serializable]
    public class LlanosCollectibleData
    {
        public string id = "Chiguiro";
        public string name = "Chigüiro de los Esteros";
        public string shortLabel = "CHIGÜIRO";
        public string emoji = "★";
        [TextArea(1, 2)]
        public string educationalNote = "Es el roedor más grande del mundo; vive en manadas protegiéndose en los ríos y lagunas llaneras.";
        public int points = 100;
        [Tooltip("Sprite del animal que aparece en la pista.")]
        public Sprite sprite;
        public Color itemColor = new Color(0.96f, 0.72f, 0.35f, 1f);
    }

    [System.Serializable]
    public class LlanosObstacleData
    {
        public string id = "Tronco";
        public string name = "Tronco Caído";
        public string shortLabel = "TRONCO";
        public string emoji = "▲";
        [Tooltip("Sprite del obstáculo que aparece en la pista.")]
        public Sprite sprite;
        public Color obstacleColor = new Color(0.65f, 0.38f, 0.22f, 1f);
    }

    [CreateAssetMenu(fileName = "NuevoSetOrinoquia", menuName = "Nehexplorer/Set de Carrera Llanera")]
    public class OrinoquiaRunnerDataSet : ScriptableObject
    {
        [Header("Información del Minijuego")]
        public string minigameTitle = "Carrera Llanera: Travesía por la Sabana";

        [TextArea(2, 3)]
        public string instructions = "Usa las flechas [Arriba / Abajo] o [W / S] o los botones táctiles para cambiar de carril. ¡Rescata la fauna llanera y esquiva los obstáculos antes de llegar al hato!";

        [Header("Reglas y Dificultad")]
        [Tooltip("Distancia total en metros a recorrer para completar la travesía.")]
        public float totalDistance = 1200f;

        [Tooltip("Velocidad de avance en metros por segundo.")]
        public float scrollSpeed = 65f;

        [Tooltip("Cantidad inicial de vidas del jinete.")]
        public int startingLives = 3;

        [Tooltip("Mínimo de animales a rescatar para superar el reto.")]
        public int requiredFaunaCount = 5;

        [Header("Dato Educativo al Ganar")]
        public string winFactTitle = "¿Sabías que...?";

        [TextArea(3, 6)]
        public string winEducationalFact = "Los Llanos Orientales y sus morichales (palmas de moriche que retienen agua subterránea) son vitales para la supervivencia de especies únicas como el Chigüiro, el Oso Palmero y el Delfín de Río (Tonina). La cultura llanera y sus cantos de vaquería han sido declarados Patrimonio Inmaterial de la Humanidad por la UNESCO.";

        [Header("Banco de Fauna a Rescatar")]
        public List<LlanosCollectibleData> faunaList = new List<LlanosCollectibleData>();

        [Header("Obstáculos de la Sabana")]
        public List<LlanosObstacleData> obstacleList = new List<LlanosObstacleData>();
    }
}
