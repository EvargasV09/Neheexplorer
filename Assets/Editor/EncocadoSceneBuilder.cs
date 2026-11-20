using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer.EditorTools
{
    // Genera una escena independiente sin alterar ni reemplazar recursos existentes.
    public static class EncocadoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Minigame_Pacifica_Encocado.unity";
        private const string DataPath = "Assets/Data/Minigames/Pacifica_EncocadoData.asset";
        private const string PrefabPath = "Assets/Prefabs/Minigames/Pacifica/Encocado/IngredientCard.prefab";
        private const string Art = "Assets/Sprites/Minigames/Pacifica/Encocado/";
        private static TMP_FontAsset font;

        private static readonly Color Azul = new Color(0.08f, 0.34f, 0.46f);
        private static readonly Color Turquesa = new Color(0.06f, 0.57f, 0.61f);
        private static readonly Color Crema = new Color(1f, 0.97f, 0.84f);

        [MenuItem("Nehexplorer/Minijuegos/Crear Pacífica - Encocado (sin sobrescribir)")]
        private static void ConstruirDesdeMenu()
        {
            try
            {
                Construir();
                EditorUtility.DisplayDialog("Encocado creado",
                    "Escena creada: " + ScenePath +
                    "\n\nAbre la escena y pulsa Play. No se reemplazó ningún archivo existente.",
                    "Entendido");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("No se pudo crear el Encocado", e.Message, "Cerrar");
            }
        }

        public static void Construir()
        {
            // Comprobar todo ANTES de crear algo: nunca sobrescribir escenas, assets o prefabs.
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null ||
                AssetDatabase.LoadAssetAtPath<EncocadoLevelDataSet>(DataPath) != null ||
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
                throw new InvalidOperationException(
                    "Ya existe la escena, el nivel o el prefab Encocado. No se sobrescribe nada.");

            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Guarda antes las escenas abiertas en Unity.");

            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (font == null) throw new InvalidOperationException(
                "Falta LiberationSans SDF de TextMeshPro. Importa TMP Essentials.");

            string[] dibujos = { "ballena.png", "bowl.png", "pescado.png", "tomate.png",
                "chocolate.png", "avena.png", "camaron.png", "coco.png" };
            foreach (string dibujo in dibujos)
                if (AssetImporter.GetAtPath(Art + dibujo) == null)
                    throw new InvalidOperationException("No se encontró el sprite: " + Art + dibujo);

            Carpeta("Assets/Data/Minigames");
            Carpeta("Assets/Prefabs/Minigames/Pacifica/Encocado");
            Sprite ballenaSprite = CargarSprite("ballena.png");
            Sprite bowlSprite = CargarSprite("bowl.png");

            var nivel = ScriptableObject.CreateInstance<EncocadoLevelDataSet>();
            nivel.ingredientes.Add(Dato("pescado", "Pescado", "pescado.png", true));
            nivel.ingredientes.Add(Dato("tomate", "Tomate", "tomate.png", true));
            nivel.ingredientes.Add(Dato("chocolate", "Chocolate", "chocolate.png", false));
            nivel.ingredientes.Add(Dato("avena", "Avena", "avena.png", false));
            nivel.ingredientes.Add(Dato("camaron", "Camarón", "camaron.png", true));
            nivel.ingredientes.Add(Dato("coco", "Coco", "coco.png", true));
            AssetDatabase.CreateAsset(nivel, DataPath);

            var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camara = new GameObject("Main Camera", typeof(Camera));
            camara.tag = "MainCamera";
            camara.transform.position = new Vector3(0, 0, -10);
            var cam = camara.GetComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.83f, 0.96f, 0.99f);

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var escalador = canvasGo.GetComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1920, 1080);
            escalador.matchWidthOrHeight = 0.5f;
            var root = (RectTransform)canvasGo.transform;

            // Fondo, encabezado y espacio para ingredientes.
            var fondo = Caja("FondoAzul", root, Vector2.zero, new Vector2(1920, 1080),
                new Color(0.84f, 0.96f, 0.99f));
            Estirar(fondo);
            var cabecera = Caja("FranjaCabecera", root, new Vector2(0, 430),
                new Vector2(1920, 250), new Color(0.68f, 0.92f, 0.95f));
            EstirarHorizontalArriba(cabecera, 250);
            var volver = Boton("VolverAlMapa", root, new Vector2(-740, 475),
                new Vector2(370, 72), "VOLVER AL MAPA", new Color(0.08f, 0.46f, 0.58f));
            var titulo = Texto("Titulo", root, new Vector2(0, 460), new Vector2(1160, 72),
                "LA BALLENA CHEF", 44, Azul);
            var region = Texto("Region", root, new Vector2(0, 405), new Vector2(950, 50),
                "REGIÓN PACÍFICA", 30, Turquesa);
            var instrucciones = Texto("Instrucciones", root, new Vector2(0, 335),
                new Vector2(1380, 64), "¡Prepara un Encocado! Arrastra los ingredientes al bowl.", 30, Azul);

            // Primero se crea un prefab propio, como otros minijuegos del proyecto.
            RectTransform plantilla = Caja("TarjetaPlantilla", root, Vector2.zero,
                new Vector2(235, 150), Crema);
            plantilla.GetComponent<Image>().raycastTarget = true;
            plantilla.gameObject.AddComponent<CanvasGroup>();
            var icono = Imagen("IconoIngrediente", plantilla, new Vector2(0, 26),
                new Vector2(115, 95), null);
            var nombre = Texto("NombreIngrediente", plantilla, new Vector2(0, -56),
                new Vector2(240, 40), "INGREDIENTE", 27, Azul);
            var scriptTarjeta = plantilla.gameObject.AddComponent<EncocadoDragItem>();
            Asignar(scriptTarjeta, "_icono", icono);
            Asignar(scriptTarjeta, "_nombre", nombre);
            PrefabUtility.SaveAsPrefabAsset(plantilla.gameObject, PrefabPath);
            UnityEngine.Object.DestroyImmediate(plantilla.gameObject);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var tarjetas = new List<EncocadoDragItem>();
            var tarjetasRect = new List<RectTransform>();
            for (int i = 0; i < nivel.ingredientes.Count; i++)
            {
                var objeto = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
                objeto.name = "Ingrediente_" + nivel.ingredientes[i].id;
                objeto.transform.SetParent(root, false);
                var r = (RectTransform)objeto.transform;
                r.sizeDelta = new Vector2(235, 150);
                r.anchoredPosition = new Vector2((i - 2.5f) * 253, 195);
                tarjetas.Add(objeto.GetComponent<EncocadoDragItem>());
                tarjetasRect.Add(r);
            }

            // Bowl: la imagen es el objetivo del drag-and-drop.
            var bowl = Imagen("BowlZonaDeSoltar", root, new Vector2(0, -110),
                new Vector2(900, 320), bowlSprite);
            bowl.raycastTarget = true;
            bowl.gameObject.AddComponent<EncocadoDropZone>();
            var bowlTitulo = Texto("BowlTitulo", bowl.transform, new Vector2(0, 112),
                new Vector2(700, 55), "MI BOWL", 30, Azul);
            var bowlInterior = new GameObject("IngredientesAceptados", typeof(RectTransform));
            bowlInterior.transform.SetParent(bowl.transform, false);
            var zonaIngredientes = (RectTransform)bowlInterior.transform;
            zonaIngredientes.sizeDelta = new Vector2(650, 170);
            zonaIngredientes.anchoredPosition = new Vector2(0, -27);

            var progreso = Texto("Progreso", root, new Vector2(0, -355),
                new Vector2(850, 60), "INGREDIENTES: 0 / 4", 32, Turquesa);
            var mensaje = Texto("Mensaje", root, new Vector2(0, -435),
                new Vector2(1150, 65), "¡Escoge los ingredientes correctos!", 26, Azul);
            var ballena = Imagen("BallenaChefEstatica", root, new Vector2(725, -300),
                new Vector2(300, 270), ballenaSprite);
            ballena.preserveAspect = true;
            ballena.raycastTarget = false;

            // La tarjeta arrastrada se pone sobre la interfaz, pero NO bloquea al bowl.
            var capa = new GameObject("CapaDeArrastre", typeof(RectTransform));
            capa.transform.SetParent(root, false);
            var capaRect = (RectTransform)capa.transform;
            Estirar(capaRect);

            // Botones y panel de victoria siguiendo las referencias de MinigameBase.
            var panel = Caja("PanelFinal", root, Vector2.zero, new Vector2(1920, 1080),
                new Color(0.04f, 0.19f, 0.24f, 0.82f));
            Estirar(panel);
            panel.GetComponent<Image>().raycastTarget = true;
            var tarjetaFinal = Caja("TarjetaVictoria", panel, Vector2.zero,
                new Vector2(1060, 680), new Color(1f, 0.98f, 0.89f));
            var victoria = Texto("TituloVictoria", tarjetaFinal, new Vector2(0, 242),
                new Vector2(1000, 95), "¡FELICITACIONES, EXPLORADOR!", 42, Turquesa);
            var plato = Texto("NombrePlato", tarjetaFinal, new Vector2(0, 130),
                new Vector2(1000, 75), "¡Preparaste Encocado!", 37, Azul);
            var explicacion = Texto("Explicacion", tarjetaFinal, new Vector2(0, -32),
                new Vector2(970, 260), nivel.explicacion, 31, Azul);
            var rejugar = Boton("JugarDeNuevo", tarjetaFinal, new Vector2(-236, -254),
                new Vector2(410, 82), "JUGAR DE NUEVO", new Color(0.98f, 0.65f, 0.20f));
            var volverResultado = Boton("VolverAlMapaDesdeVictoria", tarjetaFinal,
                new Vector2(240, -254), new Vector2(440, 82),
                "VOLVER AL MAPA", Turquesa);
            panel.gameObject.SetActive(false);

            var juegoObjeto = new GameObject("EncocadoGame", typeof(EncocadoGame));
            var juego = juegoObjeto.GetComponent<EncocadoGame>();
            Asignar(juego, "_nivel", nivel);
            AsignarLista(juego, "_tarjetas", tarjetas.ToArray());
            Asignar(juego, "_capaArrastre", capaRect);
            Asignar(juego, "_ingredientesBowl", zonaIngredientes);
            Asignar(juego, "_bowl", bowl.GetComponent<RectTransform>());
            Asignar(juego, "_titulo", titulo);
            Asignar(juego, "_instrucciones", instrucciones);
            Asignar(juego, "_progreso", progreso);
            Asignar(juego, "_mensaje", mensaje);
            Asignar(juego, "_jugarDeNuevo", rejugar);
            Asignar(juego, "factPanel", panel.gameObject);
            Asignar(juego, "resultTitleText", victoria);
            Asignar(juego, "factTitleText", plato);
            Asignar(juego, "factBodyText", explicacion);
            Asignar(juego, "returnToHubButton", volverResultado);
            Asignar(juego, "returnButtonText", volverResultado.GetComponentInChildren<TextMeshProUGUI>(true));
            // Botón superior: una sola llamada, sin duplicar listeners del código base.
            UnityEventTools.AddPersistentListener(volver.onClick, juego.ReturnToHubImmediately);
            var serialJuego = new SerializedObject(juego);
            serialJuego.FindProperty("regionId").stringValue = "Pacifica";
            serialJuego.FindProperty("regionDisplayName").stringValue = "Región Pacífica";
            serialJuego.ApplyModifiedPropertiesWithoutUndo();

            var adaptable = canvasGo.AddComponent<EncocadoResponsiveLayout>();
            Asignar(adaptable, "_root", root);
            Asignar(adaptable, "_titulo", titulo.rectTransform);
            Asignar(adaptable, "_region", region.rectTransform);
            Asignar(adaptable, "_instrucciones", instrucciones.rectTransform);
            AsignarLista(adaptable, "_tarjetas", tarjetasRect.ToArray());
            Asignar(adaptable, "_bowl", bowl.rectTransform);
            Asignar(adaptable, "_progreso", progreso.rectTransform);
            Asignar(adaptable, "_mensaje", mensaje.rectTransform);
            Asignar(adaptable, "_ballena", ballena.rectTransform);
            Asignar(adaptable, "_volver", volver.GetComponent<RectTransform>());

            var eventos = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            eventos.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();

            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(escena, ScenePath))
                throw new InvalidOperationException("Unity no pudo guardar la escena.");
            Debug.Log("[Encocado] Escena Pacífica creada sin modificar minijuegos existentes.");
        }

        private static EncocadoIngredient Dato(string id, string nombre, string png, bool correcto)
        {
            return new EncocadoIngredient { id = id, nombre = nombre,
                imagen = CargarSprite(png), correcto = correcto };
        }

        private static Sprite CargarSprite(string nombre)
        {
            string ruta = Art + nombre;
            var importador = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (importador == null) throw new InvalidOperationException("Imagen ausente: " + ruta);
            if (importador.textureType != TextureImporterType.Sprite ||
                importador.spriteImportMode != SpriteImportMode.Single)
            {
                importador.textureType = TextureImporterType.Sprite;
                importador.spriteImportMode = SpriteImportMode.Single;
                importador.alphaIsTransparency = true;
                importador.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (sprite == null) throw new InvalidOperationException("No se pudo importar el sprite " + ruta);
            return sprite;
        }

        private static void Carpeta(string ruta)
        {
            if (AssetDatabase.IsValidFolder(ruta)) return;
            int n = ruta.LastIndexOf('/');
            Carpeta(ruta.Substring(0, n));
            AssetDatabase.CreateFolder(ruta.Substring(0, n), ruta.Substring(n + 1));
        }

        private static RectTransform Caja(string nombre, Transform parent, Vector2 posicion,
            Vector2 tamano, Color color)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = r.anchorMax = new Vector2(.5f, .5f);
            r.sizeDelta = tamano;
            r.anchoredPosition = posicion;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return r;
        }

        private static Image Imagen(string nombre, Transform parent, Vector2 posicion,
            Vector2 tamano, Sprite sprite)
        {
            var r = Caja(nombre, parent, posicion, tamano, Color.white);
            var image = r.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            return image;
        }

        private static TextMeshProUGUI Texto(string nombre, Transform parent, Vector2 posicion,
            Vector2 tamano, string contenido, int tamanoLetra, Color color)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = r.anchorMax = new Vector2(.5f, .5f);
            r.sizeDelta = tamano;
            r.anchoredPosition = posicion;
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = contenido;
            label.fontSize = tamanoLetra;
            label.enableAutoSizing = true;
            label.fontSizeMin = 18;
            label.fontSizeMax = tamanoLetra;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private static Button Boton(string nombre, Transform parent, Vector2 posicion,
            Vector2 tamano, string contenido, Color fondo)
        {
            var r = Caja(nombre, parent, posicion, tamano, fondo);
            var img = r.GetComponent<Image>();
            img.raycastTarget = true;
            var boton = r.gameObject.AddComponent<Button>();
            boton.targetGraphic = img;
            Texto("Texto", r, Vector2.zero, tamano - new Vector2(18, 10),
                contenido, 27, Color.white);
            return boton;
        }

        private static void Estirar(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        private static void EstirarHorizontalArriba(RectTransform r, float altura)
        {
            r.anchorMin = new Vector2(0, 1);
            r.anchorMax = new Vector2(1, 1);
            r.pivot = new Vector2(.5f, 1);
            r.offsetMin = new Vector2(0, -altura);
            r.offsetMax = Vector2.zero;
        }

        private static void Asignar(UnityEngine.Object destino, string nombre, UnityEngine.Object valor)
        {
            var serial = new SerializedObject(destino);
            var propiedad = serial.FindProperty(nombre);
            if (propiedad == null) throw new InvalidOperationException("Campo no existe: " + nombre);
            propiedad.objectReferenceValue = valor;
            serial.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AsignarLista<T>(UnityEngine.Object destino, string nombre, T[] elementos)
            where T : UnityEngine.Object
        {
            var serial = new SerializedObject(destino);
            var propiedad = serial.FindProperty(nombre);
            propiedad.arraySize = elementos.Length;
            for (int i = 0; i < elementos.Length; i++)
                propiedad.GetArrayElementAtIndex(i).objectReferenceValue = elementos[i];
            serial.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
