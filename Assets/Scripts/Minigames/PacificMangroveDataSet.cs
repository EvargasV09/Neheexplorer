using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nehexplorer
{
    [Serializable]
    public class MangroveTaskData
    {
        public string id;
        public string label;
        [Range(0,2)] public int stage;
        public string targetId;
        public Sprite sprite;
        public Color color = Color.yellow;
        [TextArea(2,5)] public string fact;
        [TextArea(1,3)] public string hint;
    }

    [CreateAssetMenu(fileName="Pacifica_MangroveData",menuName="Nehexplorer/Restauración del Manglar")]
    public class PacificMangroveDataSet : ScriptableObject
    {
        public List<MangroveTaskData> tasks = new List<MangroveTaskData>();
        public string[] stageTitles = { "1. RETIRA LOS RESIDUOS", "2. UBICA LAS PLANTAS", "3. RECUPERA LOS REFUGIOS" };
        [TextArea(2,4)] public string[] stageInstructions = {
            "Arrastra los residuos al punto de recolección. No los entierres ni los arrojes al agua.",
            "Lee cada tarjeta y ubícala en la zona que indica su pista.",
            "Relaciona cada animal con su refugio. En la naturaleza no se trasladan animales para completar una restauración."
        };
        [TextArea(3,6)] public string winFact = "Los manglares del Pacífico colombiano ofrecen refugio y alimento a numerosos seres vivos. Sus raíces ayudan a retener sedimentos y sus suelos pueden almacenar carbono. Restaurarlos requiere recuperar el flujo del agua, reducir las presiones humanas y contar con las comunidades locales; no basta con plantar árboles.";
        [TextArea(2,4)] public string modelNote = "Modelo educativo simplificado: la distribución de los mangles depende del agua, el suelo y la salinidad. Una restauración real requiere evaluación técnica y participación comunitaria.";
    }
}
