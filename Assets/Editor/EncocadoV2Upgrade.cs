using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer.EditorTools
{
    // Actualiza la escena ya creada, sin reconstruirla ni tocar otros minijuegos.
    public static class EncocadoV2Upgrade
    {
        private const string RutaEncocado = "Assets/Scenes/Minigame_Pacifica_Encocado.unity";
        private const string RutaMapa = "Assets/Scenes/MapaHub.unity";
        private const string RutaDatos = "Assets/Data/Minigames/Pacifica_EncocadoData.asset";
        private const string RutaPrefab = "Assets/Prefabs/Minigames/Pacifica/Encocado/IngredientCard.prefab";
        private const string RutaCoco = "Assets/Sprites/Minigames/Pacifica/Encocado/coco.png";

        [MenuItem("Nehexplorer/Minijuegos/ACTUALIZAR Encocado v2 (mapa + coco + tarjetas)")]
        public static void Aplicar()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Encocado v2", "Detén Play antes de actualizar.", "OK");
                return;
            }

            foreach (Scene escena in EscenasAbiertas())
            {
                if (escena.isDirty)
                {
                    EditorUtility.DisplayDialog("Encocado v2",
                        "Guarda antes las escenas abiertas (Ctrl+S). No se perderán cambios pendientes.", "OK");
                    return;
                }
            }

            try
            {
                Prevalidar();
                string respaldo = CrearRespaldo();
                ActualizarDatosYEscena();
                ActualizarMapa();
                AgregarABuildSettings();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                // Dejar abierta la escena corregida para comprobarla inmediatamente.
                EditorSceneManager.OpenScene(RutaEncocado, OpenSceneMode.Single);
                Debug.Log("[Encocado v2] Seis tarjetas (4 correctas), regreso de avena, " +
                          "enlace desde MapaHub y Build Settings actualizados. Respaldo: " + respaldo);
                EditorUtility.DisplayDialog("Encocado v2 actualizado",
                    "Listo: coco correcto, seis tarjetas, animación de regreso corregida y " +
                    "acceso desde la Región Pacífica del MapaHub.\n\n" +
                    "La escena quedó abierta. Pulsa Play y después comprueba el acceso desde MapaHub.\n\n" +
                    "Respaldo automático: " + respaldo, "Entendido");
            }
            catch (Exception e)
            {
                Debug.LogError("[Encocado v2] Actualización incompleta: " + e);
                EditorUtility.DisplayDialog("Encocado v2: revisar Console",
                    e.Message + "\n\nSi hubo cambios parciales, restaura el respaldo generado.", "Cerrar");
            }
        }

        private static IEnumerable<Scene> EscenasAbiertas()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                yield return SceneManager.GetSceneAt(i);
        }

        private static void Prevalidar()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RutaEncocado) == null)
                throw new Exception("Primero se debe generar la escena original " + RutaEncocado);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RutaMapa) == null)
                throw new Exception("No existe MapaHub.unity en Assets/Scenes.");
            if (AssetDatabase.LoadAssetAtPath<EncocadoLevelDataSet>(RutaDatos) == null)
                throw new Exception("Falta el archivo de datos del Encocado: " + RutaDatos);
            if (AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab) == null)
                throw new Exception("Falta el prefab IngredientCard: " + RutaPrefab);
            if (AssetImporter.GetAtPath(RutaCoco) == null)
                throw new Exception("No se encontró coco.png. Copia toda la carpeta Assets del parche.");
        }

        private static string CrearRespaldo()
        {
            string raiz = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string carpeta = Path.Combine(raiz, "Respaldo_Encocado_v2_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(carpeta);
            foreach (string origen in new[] { RutaEncocado, RutaMapa, RutaDatos,
                "ProjectSettings/EditorBuildSettings.asset" })
            {
                string archivo = Path.Combine(raiz, origen.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(archivo))
                    File.Copy(archivo, Path.Combine(carpeta, Path.GetFileName(archivo)), true);
            }
            return carpeta;
        }

        private static void ActualizarDatosYEscena()
        {
            Sprite coco = ImportarSprite();
            EncocadoLevelDataSet datos = AssetDatabase.LoadAssetAtPath<EncocadoLevelDataSet>(RutaDatos);
            if (datos.ingredientes == null) datos.ingredientes = new List<EncocadoIngredient>();

            // No perder otros campos del asset: solo actualizar los seis ingredientes.
            string[] ids = { "pescado", "tomate", "chocolate", "avena", "camaron", "coco" };
            string[] nombres = { "Pescado", "Tomate", "Chocolate", "Avena", "Camarón", "Coco" };
            bool[] correctos = { true, true, false, false, true, true };
            for (int i = 0; i < ids.Length; i++)
            {
                EncocadoIngredient dato = datos.ingredientes.Find(x => x != null && x.id == ids[i]);
                if (dato == null)
                {
                    dato = new EncocadoIngredient { id = ids[i] };
                    datos.ingredientes.Add(dato);
                }
                dato.nombre = nombres[i];
                dato.correcto = correctos[i];
                if (ids[i] == "coco") dato.imagen = coco;
            }
            // Los primeros 5 quedan en su orden anterior y coco al final.
            if (datos.ingredientes.Count != 6)
                throw new Exception("El asset contiene ingredientes adicionales o duplicados. " +
                                    "No se reemplazará información sin revisar: " + datos.ingredientes.Count);
            EditorUtility.SetDirty(datos);

            Scene escena = EditorSceneManager.OpenScene(RutaEncocado, OpenSceneMode.Single);
            EncocadoGame juego = BuscarComponente<EncocadoGame>(escena);
            EncocadoResponsiveLayout responsive = BuscarComponente<EncocadoResponsiveLayout>(escena);
            RectTransform canvas = Buscar(escena, "Canvas").GetComponent<RectTransform>();
            if (juego == null || responsive == null || canvas == null)
                throw new Exception("La escena no tiene EncocadoGame, EncocadoResponsiveLayout o Canvas.");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefab);
            var tarjetas = new EncocadoDragItem[6];
            var rects = new RectTransform[6];
            for (int i = 0; i < ids.Length; i++)
            {
                GameObject objeto = BuscarOpcional(escena, "Ingrediente_" + ids[i]);
                if (objeto == null && ids[i] == "coco")
                {
                    objeto = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
                    objeto.name = "Ingrediente_coco";
                    objeto.transform.SetParent(canvas, false);
                    // Mantener las tarjetas juntas, delante del fondo y detrás del bowl.
                    GameObject anterior = Buscar(escena, "Ingrediente_camaron");
                    objeto.transform.SetSiblingIndex(anterior.transform.GetSiblingIndex() + 1);
                }
                if (objeto == null)
                    throw new Exception("Falta la tarjeta: Ingrediente_" + ids[i]);

                RectTransform rect = objeto.GetComponent<RectTransform>();
                EncocadoDragItem tarjeta = objeto.GetComponent<EncocadoDragItem>();
                Image imagen = objeto.transform.Find("IconoIngrediente")?.GetComponent<Image>();
                TextMeshProUGUI nombre = objeto.transform.Find("NombreIngrediente")?.GetComponent<TextMeshProUGUI>();
                if (rect == null || tarjeta == null || imagen == null || nombre == null)
                    throw new Exception("Faltan componentes en la tarjeta: " + objeto.name);
                tarjetas[i] = tarjeta;
                rects[i] = rect;

                var soTarjeta = new SerializedObject(tarjeta);
                Referencia(soTarjeta, "_icono", imagen);
                Referencia(soTarjeta, "_nombre", nombre);
                soTarjeta.ApplyModifiedPropertiesWithoutUndo();

                imagen.sprite = datos.ingredientes[i].imagen;
                imagen.preserveAspect = true;
                imagen.color = Color.white;
                nombre.text = datos.ingredientes[i].nombre.ToUpperInvariant();
                EditorUtility.SetDirty(imagen);
                EditorUtility.SetDirty(nombre);

                // Previsualización de editor; en Play ajusta EncocadoResponsiveLayout.
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                rect.pivot = new Vector2(.5f, .5f);
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
                rect.sizeDelta = new Vector2(235f, 150f);
                rect.anchoredPosition = new Vector2((i - 2.5f) * 253f, 195f);
            }

            var soJuego = new SerializedObject(juego);
            Referencia(soJuego, "_nivel", datos);
            ArrayReferencias(soJuego, "_tarjetas", tarjetas);
            soJuego.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(juego);

            var soLayout = new SerializedObject(responsive);
            Referencia(soLayout, "_root", canvas);
            ArrayReferencias(soLayout, "_tarjetas", rects);
            soLayout.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(responsive);

            GameObject progreso = BuscarOpcional(escena, "Progreso");
            if (progreso != null)
            {
                TextMeshProUGUI texto = progreso.GetComponent<TextMeshProUGUI>();
                if (texto != null) { texto.text = "INGREDIENTES: 0 / 4"; EditorUtility.SetDirty(texto); }
            }

            EditorSceneManager.MarkSceneDirty(escena);
            if (!EditorSceneManager.SaveScene(escena))
                throw new Exception("No se pudo guardar la escena Encocado.");
            AssetDatabase.SaveAssets();
        }

        private static void ActualizarMapa()
        {
            Scene escena = EditorSceneManager.OpenScene(RutaMapa, OpenSceneMode.Single);
            RegionWheelController rueda = BuscarComponente<RegionWheelController>(escena);
            if (rueda == null) throw new Exception("MapaHub no tiene RegionWheelController.");
            SerializedObject serial = new SerializedObject(rueda);
            Button boton = serial.FindProperty("exploreButton").objectReferenceValue as Button;
            if (boton == null) throw new Exception("Falta Explore Button en RegionWheelController.");
            var navegador = boton.GetComponent<EncocadoHubNavigator>();
            if (navegador == null) navegador = boton.gameObject.AddComponent<EncocadoHubNavigator>();
            var serialNav = new SerializedObject(navegador);
            Referencia(serialNav, "_rueda", rueda);
            serialNav.ApplyModifiedPropertiesWithoutUndo();

            // Reemplazar SOLO el listener OnClickExplore del botón; no el script de la rueda.
            for (int i = boton.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                if (boton.onClick.GetPersistentTarget(i) == rueda &&
                    boton.onClick.GetPersistentMethodName(i) == "OnClickExplore")
                    UnityEventTools.RemovePersistentListener(boton.onClick, i);
            }
            bool yaConectado = false;
            for (int i = 0; i < boton.onClick.GetPersistentEventCount(); i++)
            {
                if (boton.onClick.GetPersistentTarget(i) == navegador &&
                    boton.onClick.GetPersistentMethodName(i) == "Explorar")
                    yaConectado = true;
            }
            if (!yaConectado) UnityEventTools.AddPersistentListener(boton.onClick, navegador.Explorar);
            EditorUtility.SetDirty(boton);
            EditorUtility.SetDirty(navegador);

            // Mostrar en el mapa el reto que ahora se abre para la Región Pacífica.
            SerializedProperty regiones = serial.FindProperty("regionDatabase");
            if (regiones != null)
            {
                for (int i = 0; i < regiones.arraySize; i++)
                {
                    SerializedProperty info = regiones.GetArrayElementAtIndex(i);
                    SerializedProperty tipo = info.FindPropertyRelative("type");
                    if (tipo != null && tipo.intValue == (int)RegionType.Pacifica)
                    {
                        SerializedProperty juegos = info.FindPropertyRelative("minigames");
                        if (juegos != null)
                        {
                            juegos.arraySize = 1;
                            juegos.GetArrayElementAtIndex(0).stringValue =
                                "La ballena chef: prepara un Encocado con ingredientes del Pacífico";
                        }
                        break;
                    }
                }
                serial.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(rueda);
            }
            EditorSceneManager.MarkSceneDirty(escena);
            if (!EditorSceneManager.SaveScene(escena))
                throw new Exception("No se pudo guardar el MapaHub.");
        }

        private static void AgregarABuildSettings()
        {
            var escenas = EditorBuildSettings.scenes.ToList();
            int i = escenas.FindIndex(s => s.path == RutaEncocado);
            if (i < 0)
                escenas.Add(new EditorBuildSettingsScene(RutaEncocado, true));
            else
                escenas[i].enabled = true;
            EditorBuildSettings.scenes = escenas.ToArray();
        }

        private static Sprite ImportarSprite()
        {
            TextureImporter importador = AssetImporter.GetAtPath(RutaCoco) as TextureImporter;
            if (importador == null) throw new Exception("No se encontró coco.png: " + RutaCoco);
            if (importador.textureType != TextureImporterType.Sprite ||
                importador.spriteImportMode != SpriteImportMode.Single)
            {
                importador.textureType = TextureImporterType.Sprite;
                importador.spriteImportMode = SpriteImportMode.Single;
                importador.alphaIsTransparency = true;
                importador.SaveAndReimport();
            }
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RutaCoco);
            if (sprite == null) throw new Exception("No se pudo convertir coco.png a Sprite.");
            return sprite;
        }

        private static T BuscarComponente<T>(Scene escena) where T : Component
        {
            foreach (GameObject raiz in escena.GetRootGameObjects())
            {
                T resultado = raiz.GetComponentInChildren<T>(true);
                if (resultado != null) return resultado;
            }
            return null;
        }

        private static GameObject Buscar(Scene escena, string nombre)
        {
            GameObject encontrado = BuscarOpcional(escena, nombre);
            if (encontrado == null) throw new Exception("No existe el GameObject " + nombre);
            return encontrado;
        }

        private static GameObject BuscarOpcional(Scene escena, string nombre)
        {
            foreach (GameObject raiz in escena.GetRootGameObjects())
            {
                Transform encontrado = BuscarHijo(raiz.transform, nombre);
                if (encontrado != null) return encontrado.gameObject;
            }
            return null;
        }

        private static Transform BuscarHijo(Transform raiz, string nombre)
        {
            if (raiz.name == nombre) return raiz;
            foreach (Transform hijo in raiz)
            {
                Transform encontrado = BuscarHijo(hijo, nombre);
                if (encontrado != null) return encontrado;
            }
            return null;
        }

        private static void Referencia(SerializedObject serial, string campo, UnityEngine.Object valor)
        {
            SerializedProperty propiedad = serial.FindProperty(campo);
            if (propiedad == null || valor == null)
                throw new Exception("No se pudo asignar " + campo + " a " + serial.targetObject.name);
            propiedad.objectReferenceValue = valor;
        }

        private static void ArrayReferencias<T>(SerializedObject serial, string campo, T[] valores)
            where T : UnityEngine.Object
        {
            SerializedProperty propiedad = serial.FindProperty(campo);
            if (propiedad == null) throw new Exception("Falta el campo " + campo);
            propiedad.arraySize = valores.Length;
            for (int i = 0; i < valores.Length; i++)
            {
                if (valores[i] == null) throw new Exception("Referencia vacía " + campo + "[" + i + "]");
                propiedad.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
            }
        }
    }
}
