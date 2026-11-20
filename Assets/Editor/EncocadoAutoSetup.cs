using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nehexplorer.EditorTools
{
    // Este script es SOLO de Editor y usa el creador que ya copiaron.
    // No modifica ni sobrescribe ninguna escena, script o prefab existente.
    [InitializeOnLoad]
    public static class EncocadoAutoSetup
    {
        private const string Escena = "Assets/Scenes/Minigame_Pacifica_Encocado.unity";
        private const string Datos = "Assets/Data/Minigames/Pacifica_EncocadoData.asset";
        private const string Prefab = "Assets/Prefabs/Minigames/Pacifica/Encocado/IngredientCard.prefab";
        private const string Creador = "Assets/Editor/EncocadoSceneBuilder.cs";
        private static int intentos = 0;

        static EncocadoAutoSetup()
        {
            // Se ejecuta una vez luego de que Unity termine de importar y compilar.
            EditorApplication.delayCall += IntentarAutomaticamente;
        }

        private static void IntentarAutomaticamente()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                if (++intentos < 50) EditorApplication.delayCall += IntentarAutomaticamente;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Escena) != null) return;
            if (AssetDatabase.LoadAssetAtPath<MonoScript>(Creador) == null) return;

            // No crear automáticamente si hay escenas con cambios sin guardar.
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                if (SceneManager.GetSceneAt(i).isDirty)
                {
                    Debug.LogWarning("[Encocado] Archivos copiados. Guarda primero la escena actual (Ctrl+S) y usa el menú Nehexplorer > Minijuegos > Reintentar crear Encocado.");
                    return;
                }
            }
            Generar();
        }

        [MenuItem("Nehexplorer/Minijuegos/Reintentar crear Encocado (sin sobrescribir)")]
        public static void Generar()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Escena) != null)
            {
                Debug.Log("[Encocado] Ya existe la escena: " + Escena);
                return;
            }
            if (AssetDatabase.LoadMainAssetAtPath(Datos) != null ||
                AssetDatabase.LoadMainAssetAtPath(Prefab) != null)
            {
                Debug.LogError("[Encocado] Ya hay datos o un prefab generados, pero falta la escena. Para evitar sobrescribir archivos, no se intentó recrearlos. Comparte el error de Console para solucionarlo con seguridad.");
                return;
            }

            try
            {
                EncocadoSceneBuilder.Construir();
                AssetDatabase.Refresh();
                Debug.Log("[Encocado] ¡Escena creada correctamente! Abre: " + Escena);
            }
            catch (Exception e)
            {
                Debug.LogError("[Encocado] No se pudo generar la escena. Abre Console (Window > General > Console) y copia el error rojo que aparece a continuación.");
                Debug.LogException(e);
            }
        }
    }
}
