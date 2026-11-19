using System;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nehexplorer
{
    [System.Serializable]
    public class AmazonSpeciesData
    {
        public string id = "DelfinRosado";
        public string name = "Delfín Rosado (Boto)";
        public string scientificName = "Inia geoffrensis";
        public string habitat = "Río Amazonas y lagos de inundación";
        public string shortLabel = "DELFÍN ROSADO";
        public string emoji = "★";
        public Sprite customSprite;
        [TextArea(2, 3)]
        public string educationalNote = "Es el delfín de río más inteligente y grande del mundo. Su coloración rosada se intensifica con la edad y es protagonista de leyendas ancestrales amazónicas.";
        public Color cardColor = new Color(0.98f, 0.65f, 0.78f, 1f);
    }

    [System.Serializable]
    public class AmazonHazardData
    {
        public string id = "Remolino";
        public string name = "Remolino Fluvial";
        public string shortLabel = "REMOLINO";
        public string emoji = "▲";
        public Sprite customSprite;
        public Color hazardColor = new Color(0.25f, 0.55f, 0.70f, 1f);
    }

    [CreateAssetMenu(fileName = "NuevoSetAmazonica", menuName = "Nehexplorer/Set de Expedición Amazónica")]
    public class AmazonExpeditionDataSet : ScriptableObject
    {
        [Header("Información del Minijuego")]
        public string minigameTitle = "Expedición en Canoa: Río Amazonas y Selva Viva";

        [TextArea(2, 3)]
        public string instructions = "Dirige la canoa con [W / S] o [Arriba / Abajo] y presiona [ESPACIO] o el botón de FOTOGRAFIAR cuando una especie esté en tu encuadre. ¡Fotografía 5 especies y esquiva los obstáculos!";

        [Header("Reglas y Navegación")]
        [Tooltip("Distancia total de la expedición en metros hasta llegar a la aldea de Puerto Nariño.")]
        public float expeditionDistance = 1200f;

        [Tooltip("Velocidad de avance en metros por segundo.")]
        public float scrollSpeed = 55f;

        [Tooltip("Cantidad de oportunidades/resistencia de la canoa.")]
        public int startingHullIntegrity = 3;

        [Tooltip("Mínimo de especies fotográficas requeridas para ganar el fragmento de sabiduría.")]
        public int requiredSpeciesPhotos = 5;

        [Header("Dato Educativo al Ganar")]
        public string winFactTitle = "¿Sabías que...?";

        [TextArea(3, 6)]
        public string winEducationalFact = "La cuenca del río Amazonas representa más de un tercio del territorio de Colombia y produce aproximadamente el 20% del oxígeno del planeta y el 15% del agua dulce que desemboca en los océanos. El Parque Nacional Natural Serranía de Chiribiquete en la Amazonía colombiana es Patrimonio Mixto de la Humanidad por sus tepuyes y más de 75.000 pinturas rupestres ancestrales.";

        [Header("Catálogo de Especies Amazónicas")]
        public List<AmazonSpeciesData> speciesCatalog = new List<AmazonSpeciesData>();

        [Header("Obstáculos del Río")]
        public List<AmazonHazardData> riverHazards = new List<AmazonHazardData>();
    }
}
