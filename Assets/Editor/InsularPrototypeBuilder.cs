using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Nehexplorer.EditorTools
{
    public static class InsularPrototypeBuilder
    {
        private static TMPro.TMP_FontAsset _font;
        private static readonly Color Ink = new Color(0.06f, 0.18f, 0.21f);
        private static readonly Color Teal = new Color(0.04f, 0.40f, 0.45f);

        public static void Build()
        {
            const string scenePath = "Assets/Scenes/Minigame_Insular.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null) throw new System.Exception("La escena Insular ya existe; no se sobrescribirá.");
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new System.Exception("Guarda tus escenas abiertas antes de crear Insular.");
            EnsureFolder("Assets/Prefabs/Minigames/Insular");
            EnsureFolder("Assets/Data/Minigames");
            _font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (_font == null) throw new System.Exception("Fuente TMP del proyecto no encontrada.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraGo = new GameObject("Main Camera", typeof(Camera));
            Undo.RegisterCreatedObjectUndo(cameraGo, "Crear cámara Insular");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.GetComponent<Camera>(); camera.orthographic = true; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Teal; cameraGo.transform.position = new Vector3(0, 0, -10);
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Crear interfaz Insular");
            var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
            var root = (RectTransform)canvasGo.transform;
            var background = Box("Background", root, Vector2.zero, new Vector2(1920,1080), new Color(0.85f,0.96f,0.94f));
            Text("Title", root, new Vector2(120,460), new Vector2(930,70), "FRUTAS DE LAS ISLAS", 44, Ink);
            Text("Instructions", root, new Vector2(0,375), new Vector2(1750,55), "Mueve el mouse por el jardín. Acércate a las frutas para descubrirlas. ¡Sin límite de tiempo!", 25, Ink);
            var progress = Text("Progress", root, new Vector2(695,460), new Vector2(430,65), "Variedades: 0 / 6", 30, Teal);
            var garden = Box("Garden", root, new Vector2(-250,-30), new Vector2(1240,640), new Color(0.74f,0.88f,0.57f));
            Text("GardenHeading", root, new Vector2(-250,325), new Vector2(1200,40), "JARDÍN TROPICAL  ·  SAN ANDRÉS Y PROVIDENCIA", 22, Teal);
            var albumPanel = Box("AlbumPanel", root, new Vector2(635,-30), new Vector2(420,640), new Color(1f,0.98f,0.86f));
            Text("AlbumHeading", albumPanel, new Vector2(0,260), new Vector2(360,60), "MI COLECCIÓN", 30, Teal);
            var album = Text("Album", albumPanel, new Vector2(0,-15), new Vector2(350,460), "", 26, Ink); album.alignment = TMPro.TextAlignmentOptions.TopLeft;
            var note = Text("CatalogNote", root, new Vector2(0,-425), new Vector2(1690,90), "", 21, Ink);

            var fruitRoot = Box("FruitItem", garden, Vector2.zero, new Vector2(150,100), new Color(1f,0.7f,0.2f));
            var fruitLabel = Text("Name", fruitRoot, Vector2.zero, new Vector2(140,90), "FRUTA", 22, Ink);
            var item = fruitRoot.gameObject.AddComponent<InsularFruitItem>();
            Set(item,"_image",fruitRoot.GetComponent<UnityEngine.UI.Image>()); Set(item,"_label",fruitLabel);
            var fruitPrefab = PrefabUtility.SaveAsPrefabAsset(fruitRoot.gameObject,"Assets/Prefabs/Minigames/Insular/FruitItem.prefab").GetComponent<InsularFruitItem>();
            Object.DestroyImmediate(fruitRoot.gameObject);
            var collector = Box("Collector", garden, Vector2.zero, new Vector2(85,85), new Color(0.08f,0.45f,0.73f));
            Text("Letter", collector, Vector2.zero, new Vector2(80,80), "E", 42, Color.white);
            PrefabUtility.SaveAsPrefabAsset(collector.gameObject,"Assets/Prefabs/Minigames/Insular/Collector.prefab");

            var discovery = Box("DiscoveryOverlay", root, Vector2.zero, new Vector2(1920,1080), new Color(0.02f,0.14f,0.20f,0.80f)); discovery.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            var card = Box("DiscoveryCard", discovery, Vector2.zero, new Vector2(1080,790), new Color(1f,0.98f,0.89f));
            var title = Text("FruitTitle",card,new Vector2(0,315),new Vector2(1000,80),"FRUTA DESCUBIERTA",38,Teal);
            var icon = Box("FruitIcon",card,new Vector2(-390,70),new Vector2(180,180),Color.yellow).GetComponent<UnityEngine.UI.Image>();
            var body = Text("FruitFact",card,new Vector2(90,35),new Vector2(720,460),"",27,Ink); body.alignment = TMPro.TextAlignmentOptions.TopLeft;
            var next = Button("ContinueButton",card,new Vector2(0,-310),new Vector2(400,80),"CONTINUAR EXPLORANDO");
            discovery.gameObject.SetActive(false);

            var finalOverlay = Box("FinalOverlay",root,Vector2.zero,new Vector2(1920,1080),new Color(0.02f,0.14f,0.20f,0.80f)); finalOverlay.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            var finalCard = Box("FinalCard",finalOverlay,Vector2.zero,new Vector2(1180,760),new Color(1f,0.98f,0.89f));
            var resultTitle = Text("ResultTitle",finalCard,new Vector2(0,290),new Vector2(1090,110),"¡ÁLBUM COMPLETO!",42,Teal);
            var factTitle = Text("FactTitle",finalCard,new Vector2(0,155),new Vector2(1080,65),"Diversidad que nos alimenta",32,Teal);
            var factBody = Text("FactBody",finalCard,new Vector2(0,-45),new Vector2(1020,290),"",29,Ink);
            var returnButton = Button("ReturnButton",finalCard,new Vector2(0,-285),new Vector2(760,90),"RECLAMAR SABIDURÍA Y VOLVER");
            finalOverlay.gameObject.SetActive(false);
            // Último sibling para que el regreso siga accesible sobre ambos paneles modales.
            var back = Button("BackToHubButton",root,new Vector2(-675,460),new Vector2(450,70),"← MAPA PRINCIPAL");
            var managerGo = new GameObject("InsularFruitGame",typeof(InsularFruitGame)); Undo.RegisterCreatedObjectUndo(managerGo,"Crear minijuego Insular");
            var game = managerGo.GetComponent<InsularFruitGame>();
            var data = ScriptableObject.CreateInstance<InsularFruitDataSet>();
            data.fruits.Add(Fruit("Coco","COCO","Cocos nucifera",new Color(0.78f,0.58f,0.32f),"Aunque suele llamarse nuez, el coco es una drupa. Su cubierta fibrosa protege la semilla y contribuye a su flotación.","La pulpa se consume fresca y se usa para obtener leche de coco. El coco es conocido en la cocina del Caribe insular."));
            data.fruits.Add(Fruit("FrutaPan","FRUTA DE PAN","Artocarpus altilis",new Color(0.60f,0.78f,0.25f),"Es un fruto compuesto formado por muchas flores. Existen cultivares con semillas y otros sin semillas.","Su pulpa rica en almidón suele comerse cocida: hervida, asada o frita. No todas las frutas se consumen dulces."));
            data.fruits.Add(Fruit("Mango","MANGO","Mangifera indica",new Color(1f,0.69f,0.14f),"Es una drupa con una semilla grande. Las variedades cultivadas cambian en color, aroma, tamaño y cantidad de fibra.","Puede comerse fresco o prepararse en jugos. Una piel verde no siempre significa que el mango esté inmaduro."));
            data.fruits.Add(Fruit("Papaya","PAPAYA","Carica papaya",new Color(1f,0.50f,0.23f),"La papaya tiene numerosas semillas en una cavidad central. Su planta tiene un tallo blando, distinto del tronco leñoso de muchos frutales.","La pulpa madura se consume fresca o en jugos. Observar semillas, hojas y frutos ayuda a reconocer distintas especies."));
            data.fruits.Add(Fruit("Tamarindo","TAMARINDO","Tamarindus indica",new Color(0.82f,0.59f,0.38f),"El fruto es una vaina: el tamarindo pertenece a la familia de las leguminosas. Su pulpa rodea varias semillas duras.","La pulpa de sabor ácido se usa en bebidas y dulces. Las frutas pueden tener sabores muy diferentes."));
            data.fruits.Add(Fruit("Guanabana","GUANÁBANA","Annona muricata",new Color(0.54f,0.77f,0.41f),"Su fruto se forma a partir de varios carpelos de una flor. Tiene pulpa blanca, numerosas semillas oscuras y una cáscara con protuberancias.","La pulpa se utiliza en jugos y postres. Conocer una fruta también significa aprender a distinguir su pulpa de sus semillas."));
            AssetDatabase.CreateAsset(data,"Assets/Data/Minigames/Insular_FruitData.asset");
            Set(game,"_dataSet",data); Set(game,"_garden",garden); Set(game,"_collector",collector); Set(game,"_fruitPrefab",fruitPrefab); Set(game,"_progressText",progress); Set(game,"_albumText",album); Set(game,"_catalogNoteText",note);
            Set(game,"_discoveryPanel",discovery.gameObject); Set(game,"_discoveryTitle",title); Set(game,"_discoveryBody",body); Set(game,"_discoveryImage",icon); Set(game,"_continueButton",next);
            Set(game,"factPanel",finalOverlay.gameObject); Set(game,"resultTitleText",resultTitle); Set(game,"factTitleText",factTitle); Set(game,"factBodyText",factBody); Set(game,"returnToHubButton",returnButton); Set(game,"returnButtonText",returnButton.GetComponentInChildren<TMPro.TextMeshProUGUI>()); Set(game,"backToHubTopButton",back);
            var settings = new SerializedObject(game); settings.FindProperty("regionId").stringValue = "Insular"; settings.FindProperty("regionDisplayName").stringValue = "Región Insular"; settings.ApplyModifiedPropertiesWithoutUndo();
            var events = new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule)); Undo.RegisterCreatedObjectUndo(events,"Crear EventSystem Insular");
            events.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene,scenePath);
        }

        private static InsularFruitData Fruit(string id,string name,string scientific,Color color,string diversity,string uses)
        {
            return new InsularFruitData {id=id,displayName=name,scientificName=scientific,color=color,diversityFact=diversity,useFact=uses,sources="Contenido botánico del prototipo pendiente de cotejo bibliográfico docente. Asociación regional del catálogo pendiente de revisión."};
        }
        private static void Set(Object target,string name,Object value) {var so=new SerializedObject(target); so.FindProperty(name).objectReferenceValue=value; so.ApplyModifiedPropertiesWithoutUndo();}
        private static void EnsureFolder(string path) {if(AssetDatabase.IsValidFolder(path)) return; int slash=path.LastIndexOf('/'); string parent=path.Substring(0,slash); EnsureFolder(parent); AssetDatabase.CreateFolder(parent,path.Substring(slash+1));}
        private static RectTransform Box(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image)); go.transform.SetParent(parent,false); var rect=(RectTransform)go.transform; rect.anchorMin=rect.anchorMax=new Vector2(0.5f,0.5f); rect.pivot=new Vector2(0.5f,0.5f); rect.sizeDelta=size; rect.anchoredPosition=pos; var image=go.GetComponent<UnityEngine.UI.Image>(); image.color=color; image.raycastTarget=false; return rect;
        }
        private static TMPro.TextMeshProUGUI Text(string name,Transform parent,Vector2 pos,Vector2 size,string value,int fontSize,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TMPro.TextMeshProUGUI)); go.transform.SetParent(parent,false); var rect=(RectTransform)go.transform; rect.anchorMin=rect.anchorMax=new Vector2(0.5f,0.5f); rect.sizeDelta=size; rect.anchoredPosition=pos; var text=go.GetComponent<TMPro.TextMeshProUGUI>(); text.font=_font; text.text=value; text.fontSize=fontSize; text.color=color; text.alignment=TMPro.TextAlignmentOptions.Center; text.raycastTarget=false; return text;
        }
        private static UnityEngine.UI.Button Button(string name,Transform parent,Vector2 pos,Vector2 size,string label)
        {
            var rect=Box(name,parent,pos,size,Teal); rect.GetComponent<UnityEngine.UI.Image>().raycastTarget=true; var button=rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic=rect.GetComponent<UnityEngine.UI.Image>(); Text("Label",rect,Vector2.zero,size-new Vector2(20,10),label,26,Color.white); return button;
        }
    }
}
