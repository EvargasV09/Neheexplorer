using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer.EditorTools
{
    // Repara SOLO la nueva escena de Encocado y su asset de datos.
    // No toca ninguna escena ni scripts de los otros minijuegos.
    public static class EncocadoRepairReferences
    {
        private const string EscenaRuta = "Assets/Scenes/Minigame_Pacifica_Encocado.unity";
        private const string DatosRuta = "Assets/Data/Minigames/Pacifica_EncocadoData.asset";
        private const string ArteRuta = "Assets/Sprites/Minigames/Pacifica/Encocado/";

        [MenuItem("Nehexplorer/Minijuegos/REPARAR imágenes y referencias del Encocado")]
        public static void Reparar()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Encocado", "Detén Play antes de reparar.", "Entendido");
                return;
            }
            if (SceneManager.GetActiveScene().isDirty)
            {
                EditorUtility.DisplayDialog("Encocado", "Guarda la escena actual (Ctrl+S) y vuelve a ejecutar el menú.", "Entendido");
                return;
            }

            try
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(EscenaRuta) == null)
                    throw new Exception("No se encontró la escena: " + EscenaRuta);

                var escena = EditorSceneManager.OpenScene(EscenaRuta, OpenSceneMode.Single);
                var nivel = AssetDatabase.LoadAssetAtPath<EncocadoLevelDataSet>(DatosRuta);
                if (nivel == null)
                    throw new Exception("No se encontró " + DatosRuta + ". El menú de creación debió generar este archivo.");

                RepararDatos(nivel);

                var objetoJuego = Buscar(escena, "EncocadoGame");
                if (objetoJuego == null) throw new Exception("Falta el GameObject EncocadoGame en la escena.");
                var juego = Requerir<EncocadoGame>(objetoJuego);

                string[] ids = { "pescado", "tomate", "chocolate", "avena", "camaron", "coco" };
                var tarjetas = new EncocadoDragItem[ids.Length];
                var rects = new RectTransform[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    var tarjetaObjeto = Buscar(escena, "Ingrediente_" + ids[i]);
                    if (tarjetaObjeto == null)
                        throw new Exception("No se encontró Ingrediente_" + ids[i] + ".");

                    tarjetas[i] = Requerir<EncocadoDragItem>(tarjetaObjeto);
                    rects[i] = Requerir<RectTransform>(tarjetaObjeto);
                    var icono = Requerir<Image>(BuscarHijo(tarjetaObjeto, "IconoIngrediente"));
                    var nombre = Requerir<TextMeshProUGUI>(BuscarHijo(tarjetaObjeto, "NombreIngrediente"));
                    var soTarjeta = new SerializedObject(tarjetas[i]);
                    Establecer(soTarjeta, "_icono", icono);
                    Establecer(soTarjeta, "_nombre", nombre);
                    soTarjeta.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(tarjetas[i]);
                    // Preview de editor: no hace falta iniciar Play para ver ingredientes.
                    icono.sprite = nivel.ingredientes[i].imagen;
                    icono.preserveAspect = true;
                    icono.color = Color.white;
                    nombre.text = nivel.ingredientes[i].nombre.ToUpperInvariant();
                    EditorUtility.SetDirty(icono);
                    EditorUtility.SetDirty(nombre);
                }

                var canvasObjeto = Buscar(escena, "Canvas");
                var canvas = Requerir<RectTransform>(canvasObjeto);
                var capa = Requerir<RectTransform>(Buscar(escena, "CapaDeArrastre"));
                var ingredientesDentro = Requerir<RectTransform>(Buscar(escena, "IngredientesAceptados"));
                var bowl = Requerir<RectTransform>(Buscar(escena, "BowlZonaDeSoltar"));
                var titulo = Requerir<TextMeshProUGUI>(Buscar(escena, "Titulo"));
                var region = Requerir<RectTransform>(Buscar(escena, "Region"));
                var instrucciones = Requerir<TextMeshProUGUI>(Buscar(escena, "Instrucciones"));
                var progreso = Requerir<TextMeshProUGUI>(Buscar(escena, "Progreso"));
                var mensaje = Requerir<TextMeshProUGUI>(Buscar(escena, "Mensaje"));
                var rejugar = Requerir<Button>(Buscar(escena, "JugarDeNuevo"));
                var panel = Buscar(escena, "PanelFinal").gameObject;
                var textoVictoria = Requerir<TextMeshProUGUI>(Buscar(escena, "TituloVictoria"));
                var textoPlato = Requerir<TextMeshProUGUI>(Buscar(escena, "NombrePlato"));
                var textoExp = Requerir<TextMeshProUGUI>(Buscar(escena, "Explicacion"));
                var volver = Requerir<Button>(Buscar(escena, "VolverAlMapaDesdeVictoria"));
                var volverSuperior = Requerir<Button>(Buscar(escena, "VolverAlMapa"));
                var ballena = Requerir<Image>(Buscar(escena, "BallenaChefEstatica"));

                var serial = new SerializedObject(juego);
                Establecer(serial, "_nivel", nivel);
                EstablecerArray(serial, "_tarjetas", tarjetas);
                Establecer(serial, "_capaArrastre", capa);
                Establecer(serial, "_ingredientesBowl", ingredientesDentro);
                Establecer(serial, "_bowl", bowl);
                Establecer(serial, "_titulo", titulo);
                Establecer(serial, "_instrucciones", instrucciones);
                Establecer(serial, "_progreso", progreso);
                Establecer(serial, "_mensaje", mensaje);
                Establecer(serial, "_jugarDeNuevo", rejugar);
                Establecer(serial, "factPanel", panel);
                Establecer(serial, "resultTitleText", textoVictoria);
                Establecer(serial, "factTitleText", textoPlato);
                Establecer(serial, "factBodyText", textoExp);
                Establecer(serial, "returnToHubButton", volver);
                Establecer(serial, "returnButtonText", volver.GetComponentInChildren<TextMeshProUGUI>(true));
                // backToHubTopButton se deja como está porque ya tiene su listener.
                serial.FindProperty("regionId").stringValue = "Pacifica";
                serial.FindProperty("regionDisplayName").stringValue = "Región Pacífica";
                serial.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(juego);

                var adap = canvas.GetComponent<EncocadoResponsiveLayout>();
                if (adap != null)
                {
                    var layout = new SerializedObject(adap);
                    Establecer(layout, "_root", canvas);
                    Establecer(layout, "_titulo", titulo.rectTransform);
                    Establecer(layout, "_region", region);
                    Establecer(layout, "_instrucciones", instrucciones.rectTransform);
                    EstablecerArray(layout, "_tarjetas", rects);
                    Establecer(layout, "_bowl", bowl);
                    Establecer(layout, "_progreso", progreso.rectTransform);
                    Establecer(layout, "_mensaje", mensaje.rectTransform);
                    Establecer(layout, "_ballena", ballena.rectTransform);
                    Establecer(layout, "_volver", volverSuperior.GetComponent<RectTransform>());
                    layout.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(adap);
                }

                ReponerSprite(Buscar(escena, "BowlZonaDeSoltar"), "bowl.png");
                ReponerSprite(Buscar(escena, "BallenaChefEstatica"), "ballena.png");
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(escena);
                if (!EditorSceneManager.SaveScene(escena))
                    throw new Exception("Unity no pudo guardar la escena reparada.");

                Debug.Log("[Encocado] Reparación terminada: nivel, 6 tarjetas, imágenes, UI y referencias guardados.");
                EditorUtility.DisplayDialog("Encocado reparado",
                    "Reparadas las referencias y las 6 tarjetas. Abre la escena y pulsa Play.\n\n" +
                    "Solo se actualizó Minigame_Pacifica_Encocado.unity y, si era necesario, su archivo de datos.", "Entendido");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Encocado] La reparación no se completó: " + ex);
                EditorUtility.DisplayDialog("Error al reparar Encocado", ex.Message + "\nRevisa Console.", "Cerrar");
            }
        }

        private static void RepararDatos(EncocadoLevelDataSet nivel)
        {
            string[] ids = { "pescado", "tomate", "chocolate", "avena", "camaron", "coco" };
            string[] nombres = { "Pescado", "Tomate", "Chocolate", "Avena", "Camarón", "Coco" };
            bool[] correctos = { true, true, false, false, true, true };
            bool cambio = false;
            if (nivel.ingredientes == null || nivel.ingredientes.Count != ids.Length)
            {
                nivel.ingredientes = new List<EncocadoIngredient>();
                for (int i = 0; i < ids.Length; i++) nivel.ingredientes.Add(new EncocadoIngredient());
                cambio = true;
            }
            for (int i = 0; i < ids.Length; i++)
            {
                if (nivel.ingredientes[i] == null)
                {
                    nivel.ingredientes[i] = new EncocadoIngredient();
                    cambio = true;
                }
                var ingrediente = nivel.ingredientes[i];
                Sprite sprite = CargarSprite(ids[i] + ".png");
                if (ingrediente.id != ids[i] || ingrediente.nombre != nombres[i] ||
                    ingrediente.correcto != correctos[i] || ingrediente.imagen != sprite)
                {
                    ingrediente.id = ids[i];
                    ingrediente.nombre = nombres[i];
                    ingrediente.correcto = correctos[i];
                    ingrediente.imagen = sprite;
                    cambio = true;
                }
            }
            if (cambio) EditorUtility.SetDirty(nivel);
        }

        private static Sprite CargarSprite(string nombre)
        {
            var ruta = ArteRuta + nombre;
            var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) throw new Exception("No existe el PNG " + ruta);
            if (imp.textureType != TextureImporterType.Sprite || imp.spriteImportMode != SpriteImportMode.Single)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.alphaIsTransparency = true;
                imp.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (sprite == null) throw new Exception("No se pudo cargar el sprite " + ruta);
            return sprite;
        }

        private static void ReponerSprite(Transform t, string nombre)
        {
            var imagen = Requerir<Image>(t);
            imagen.sprite = CargarSprite(nombre);
            imagen.preserveAspect = true;
            EditorUtility.SetDirty(imagen);
        }

        private static Transform Buscar(Scene escena, string nombre)
        {
            foreach (var raiz in escena.GetRootGameObjects())
            {
                var encontrado = BuscarHijo(raiz.transform, nombre);
                if (encontrado != null) return encontrado;
            }
            throw new Exception("Falta el GameObject: " + nombre);
        }

        private static Transform BuscarHijo(Transform raiz, string nombre)
        {
            if (raiz.name == nombre) return raiz;
            for (int i = 0; i < raiz.childCount; i++)
            {
                var r = BuscarHijo(raiz.GetChild(i), nombre);
                if (r != null) return r;
            }
            return null;
        }

        private static T Requerir<T>(Transform t) where T : Component
        {
            if (t == null) throw new Exception("Falta un objeto de la escena para " + typeof(T).Name);
            var componente = t.GetComponent<T>();
            if (componente == null) throw new Exception(t.name + " no tiene " + typeof(T).Name);
            return componente;
        }

        private static void Establecer(SerializedObject so, string campo, UnityEngine.Object valor)
        {
            if (valor == null) throw new Exception("La referencia " + campo + " es nula.");
            var p = so.FindProperty(campo);
            if (p == null) throw new Exception("No existe el campo " + campo + " en " + so.targetObject.name);
            p.objectReferenceValue = valor;
        }

        private static void EstablecerArray<T>(SerializedObject so, string campo, T[] valores) where T : UnityEngine.Object
        {
            var p = so.FindProperty(campo);
            if (p == null) throw new Exception("No existe el campo " + campo + " en " + so.targetObject.name);
            p.arraySize = valores.Length;
            for (int i = 0; i < valores.Length; i++)
            {
                if (valores[i] == null) throw new Exception("Referencia nula en " + campo + " [" + i + "]");
                p.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
            }
        }
    }
}
