using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nehexplorer
{
    // Todos los datos de ESTE ÚNICO NIVEL se editan desde un ScriptableObject.
    [Serializable]
    public class EncocadoIngredient
    {
        public string id;
        public string nombre;
        public Sprite imagen;
        public bool correcto;
    }

    [CreateAssetMenu(fileName = "Pacifica_EncocadoData", menuName = "Nehexplorer/Minijuegos/Pacífica - Encocado")]
    public class EncocadoLevelDataSet : ScriptableObject
    {
        public string regionId = "Pacifica";
        public string region = "Región Pacífica";
        public string plato = "Encocado";
        public List<EncocadoIngredient> ingredientes = new List<EncocadoIngredient>();

        [TextArea(3, 6)]
        public string explicacion =
            "El encocado es una receta tradicional del Pacífico colombiano. " +
            "Se prepara con pescado o mariscos y una sabrosa salsa de leche de coco. " +
            "¡Nuestra ballena chef celebra los sabores de esta región!";
    }
}
