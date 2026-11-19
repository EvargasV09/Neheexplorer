using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nehexplorer
{
    [System.Serializable]
    public class TriviaQuestion
    {
        [Tooltip("Texto completo de la pregunta.")]
        [TextArea(2, 4)]
        public string questionText = "¿Cuál es el árbol nacional de Colombia?";

        [Tooltip("Categoría temática (ej: 'Páramos y Agua', 'Pisos Térmicos', 'Fauna Andina').")]
        public string category = "Flora Andina";

        [Tooltip("Emoji o ícono representativo de la categoría.")]
        public string categoryEmoji = "🌲";

        [Tooltip("Ilustración que acompaña esta pregunta.")]
        public Sprite illustration;

        [Tooltip("4 opciones de respuesta.")]
        public string[] options = new string[] { "Opción A", "Opción B", "Opción C", "Opción D" };

        [Tooltip("Índice de la respuesta correcta (0 para A, 1 para B, 2 para C, 3 para D).")]
        [Range(0, 3)]
        public int correctIndex = 0;

        [Tooltip("Explicación educativa y enriquecedora que se muestra tras responder.")]
        [TextArea(2, 4)]
        public string explanation = "Explicación educativa sobre este aspecto de los Andes colombianos.";
    }

    [CreateAssetMenu(fileName = "NuevoSetTriviaAndina", menuName = "Nehexplorer/Set de Trivia Andina")]
    public class AndeanTriviaDataSet : ScriptableObject
    {
        [Header("Información del Minijuego")]
        public string minigameTitle = "Trivia de las Cordilleras: Sabiduría Andina";

        [TextArea(2, 3)]
        public string instructions = "Responde las preguntas sobre la geografía, páramos y biodiversidad de los Andes colombianos antes de que acabe el tiempo.";

        [Header("Configuración del Desafío")]
        [Tooltip("Segundos disponibles por cada pregunta.")]
        public float secondsPerQuestion = 18f;

        [Tooltip("Mínimo de preguntas correctas necesarias para superar el reto y reclamar el fragmento.")]
        public int requiredScore = 3;

        [Header("Dato Educativo al Ganar (Reclamación del Fragmento)")]
        public string winFactTitle = "¿Sabías que...?";

        [TextArea(3, 6)]
        public string winEducationalFact = "Colombia alberga cerca del 50% de los páramos del mundo entero, destacando el Páramo de Sumapaz (el más extenso del planeta) y Chingaza. Los frailejones retienen el agua de la niebla como esponjas gigantes y la liberan lentamente, abasteciendo de agua potable a más del 70% de la población del país.";

        [Header("Banco de Preguntas Educativas")]
        public List<TriviaQuestion> questions = new List<TriviaQuestion>();
    }
}
