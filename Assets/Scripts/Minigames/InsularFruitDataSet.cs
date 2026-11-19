using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nehexplorer
{
    [Serializable]
    public class InsularFruitData
    {
        public string id;
        public string displayName;
        public string scientificName;
        public Sprite sprite;
        public Color color = Color.yellow;
        [TextArea(3, 6)] public string diversityFact;
        [TextArea(2, 4)] public string useFact;
        [TextArea(2, 4)] public string sources;
    }

    [CreateAssetMenu(fileName = "Insular_FruitData", menuName = "Nehexplorer/Frutas Insulares")]
    public class InsularFruitDataSet : ScriptableObject
    {
        [Header("Colección")]
        public List<InsularFruitData> fruits = new List<InsularFruitData>();
        [Min(1f)] public float moveSpeed = 600f;
        [Min(1f)] public float collectionRadius = 64f;
        [Header("Panel Final")]
        public string winTitle = "¡ÁLBUM DE FRUTAS COMPLETO!";
        [TextArea(3, 6)] public string winFact = "La diversidad de frutas incluye diferentes especies, formas, colores y semillas. Conocerlas ayuda a valorar las plantas que nos alimentan. No todas las frutas cultivadas en una isla son nativas ni exclusivas de ella.";
        [TextArea(2, 4)] public string catalogNote = "Colección tropical de demostración. Selección regional pendiente de revisión docente; no implica que todas las frutas sean nativas de las islas.";
    }
}
