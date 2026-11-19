using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Nehexplorer.EditorTools
{
    public static class PacificMangroveBuilder
    {
        private static TMPro.TMP_FontAsset _font;
        private static readonly Color Ink=new Color(0.06f,0.20f,0.19f);
        private static readonly Color Teal=new Color(0.04f,0.38f,0.32f);
        public static void Build()
        {
            const string path="Assets/Scenes/Minigame_Pacifica.unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)!=null) throw new System.Exception("La escena Pacífica ya existe. No se sobrescribirá.");
            for(int i=0;i<SceneManager.sceneCount;i++) if(SceneManager.GetSceneAt(i).isDirty) throw new System.Exception("Guarda las escenas abiertas antes de continuar.");
            Folder("Assets/Data/Minigames"); Folder("Assets/Prefabs/Minigames/Pacifica");
            _font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset"); if(_font==null) throw new System.Exception("Fuente TMP no encontrada.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cameraGo=new GameObject("Main Camera",typeof(Camera)); Undo.RegisterCreatedObjectUndo(cameraGo,"Crear Pacífica"); cameraGo.tag="MainCamera"; cameraGo.transform.position=new Vector3(0,0,-10); var cam=cameraGo.GetComponent<Camera>();cam.orthographic=true;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Teal;
            var canvasGo=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster)); Undo.RegisterCreatedObjectUndo(canvasGo,"Crear interfaz Pacífica"); canvasGo.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=0.5f;
            var root=(RectTransform)canvasGo.transform;
            Box("Background",root,Vector2.zero,new Vector2(1920,1080),new Color(0.85f,0.95f,0.90f));
            Text("Title",root,new Vector2(115,460),new Vector2(1000,70),"RESTAURA EL MANGLAR",44,Ink);
            var progress=Text("Progress",root,new Vector2(725,460),new Vector2(430,60),"Restauración: 0 / 9",28,Teal);
            var stageTitle=Text("StageTitle",root,new Vector2(0,355),new Vector2(1780,65),"1. RETIRA LOS RESIDUOS",32,Teal);
            var instructions=Text("Instructions",root,new Vector2(0,295),new Vector2(1740,70),"Arrastra los residuos al punto de recolección.",25,Ink);
            var source=Box("TaskPanel",root,new Vector2(-665,-70),new Vector2(400,600),new Color(1f,0.97f,0.84f));
            Text("TaskHeading",source,new Vector2(0,245),new Vector2(370,55),"TARJETAS",29,Teal);
            var board=Box("MangroveBoard",root,new Vector2(225,-70),new Vector2(1280,600),new Color(0.68f,0.83f,0.70f));
            Text("BoardHeading",board,new Vector2(0,245),new Vector2(1170,60),"DECIDE CÓMO CUIDAR ESTE ECOSISTEMA",26,Ink);
            var feedback=Text("Feedback",root,new Vector2(0,-410),new Vector2(1750,65),"Arrastra una tarjeta y suéltala en su destino.",25,Teal);
            var modelNote=Text("ModelNote",root,new Vector2(0,-485),new Vector2(1770,70),"",20,Ink);
            var dragLayer=Box("DragLayer",root,Vector2.zero,new Vector2(1920,1080),Color.clear);
            var zones=new List<MangroveDropZone>();
            zones.Add(Zone(board,"Waste",0,Vector2.zero,new Vector2(760,320),"PUNTO DE RECOLECCIÓN\n\nResiduos fuera del agua y del suelo\npara su gestión adecuada",new Color(0.78f,0.81f,0.86f)));
            zones.Add(Zone(board,"Red",1,new Vector2(-405,-15),new Vector2(365,360),"BORDE DEL AGUA\n\nMangle rojo\nRaíces en forma de zancos",new Color(0.63f,0.82f,0.86f)));
            zones.Add(Zone(board,"Black",1,new Vector2(0,-15),new Vector2(365,360),"SUELO LODOSO\n\nMangle negro\nRaíces que emergen del suelo",new Color(0.73f,0.66f,0.48f)));
            zones.Add(Zone(board,"White",1,new Vector2(405,-15),new Vector2(365,360),"ZONA MENOS INUNDADA\n\nMangle blanco\nUbicación orientativa",new Color(0.78f,0.87f,0.53f)));
            zones.Add(Zone(board,"Roots",2,new Vector2(-405,-15),new Vector2(365,360),"RAÍCES Y FANGO\n\nRefugios en el suelo\ndel manglar",new Color(0.73f,0.66f,0.48f)));
            zones.Add(Zone(board,"Water",2,new Vector2(0,-15),new Vector2(365,360),"CANALES ENTRE RAÍCES\n\nAgua conectada con\nel estuario",new Color(0.63f,0.82f,0.86f)));
            zones.Add(Zone(board,"Branches",2,new Vector2(405,-15),new Vector2(365,360),"RAMAS DEL MANGLAR\n\nDescanso sobre\nla vegetación",new Color(0.78f,0.87f,0.53f)));
            var data=ScriptableObject.CreateInstance<PacificMangroveDataSet>();
            Add(data,"Bottle","BOTELLA\nResiduo plástico",0,"Waste",new Color(0.74f,0.83f,0.93f),"Retirar los residuos reduce los riesgos de ingestión y enredo para la fauna. Los residuos deben recogerse de forma segura y entregarse para su gestión adecuada.","Los residuos van al punto de recolección.");
            Add(data,"Bag","BOLSA\nResiduo plástico",0,"Waste",new Color(0.93f,0.83f,0.63f),"Una bolsa plástica puede fragmentarse en piezas más pequeñas. Evitar que llegue al manglar y reducir su uso es mejor que tener que retirarla después.","No entierres la bolsa ni la arrojes al agua.");
            Add(data,"Can","LATA\nResiduo metálico",0,"Waste",new Color(0.82f,0.82f,0.85f),"Los residuos metálicos pueden tener bordes cortantes. En una limpieza real se utilizan guantes y se sigue un protocolo de seguridad; no se manipulan objetos peligrosos sin ayuda.","La lata debe salir del ecosistema para una gestión segura.");
            Add(data,"RedPlant","MANGLE ROJO\nPista: borde del agua",1,"Red",new Color(0.83f,0.66f,0.42f),"El mangle rojo tiene raíces de soporte que parecen zancos y ayudan a estabilizarlo en sedimentos blandos. Las raíces crean espacios que pueden servir de refugio a otros organismos.","Busca la zona del borde del agua.");
            Add(data,"BlackPlant","MANGLE NEGRO\nPista: suelo lodoso",1,"Black",new Color(0.73f,0.65f,0.49f),"El mangle negro puede desarrollar neumatóforos: raíces que sobresalen del suelo y facilitan el intercambio de gases en terrenos con poco oxígeno. No deben confundirse con basura ni cortarse.","Busca el suelo lodoso y sus raíces emergentes.");
            Add(data,"WhitePlant","MANGLE BLANCO\nPista: menos inundación",1,"White",new Color(0.82f,0.88f,0.56f),"El mangle blanco suele encontrarse en zonas con inundación menos frecuente, aunque la distribución cambia entre lugares. Este tablero es un modelo: antes de plantar hay que evaluar el agua, el suelo y las especies locales.","Busca la zona menos inundada del modelo.");
            Add(data,"Crab","CANGREJO\nPista: raíces y fango",2,"Roots",new Color(0.98f,0.61f,0.36f),"Muchos cangrejos del manglar usan el suelo y las raíces como refugio. Algunos consumen materia orgánica y contribuyen a su transformación. Restaurar hábitats no significa trasladar animales por cuenta propia.","Busca los refugios del suelo entre raíces.");
            Add(data,"Fish","PECES JUVENILES\nPista: canales con agua",2,"Water",new Color(0.56f,0.80f,0.92f),"Los canales conectados con el estuario y las raíces sumergidas pueden ofrecer alimento y protección a peces juveniles. Mantener la circulación del agua es esencial para recuperar un manglar.","Los peces necesitan los canales de agua.");
            Add(data,"Heron","GARZA\nPista: ramas para descansar",2,"Branches",new Color(0.92f,0.92f,0.85f),"Las garzas pueden usar ramas como lugares de descanso y buscar alimento en zonas someras. Conservar vegetación y evitar molestias ayuda a proteger a las aves. Aquí relacionas refugios, no capturas animales.","Busca las ramas donde puede descansar un ave.");
            AssetDatabase.CreateAsset(data,"Assets/Data/Minigames/Pacifica_MangroveData.asset");
            var items=new List<MangroveDragItem>();
            for(int i=0;i<data.tasks.Count;i++) {
                var rect=Box("Task_"+data.tasks[i].id,source,new Vector2(0,140-(i%3)*165),new Vector2(345,130),data.tasks[i].color); rect.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
                rect.gameObject.AddComponent<CanvasGroup>(); var label=Text("Label",rect,Vector2.zero,new Vector2(325,120),data.tasks[i].label,25,Ink);
                var item=rect.gameObject.AddComponent<MangroveDragItem>(); Set(item,"_image",rect.GetComponent<UnityEngine.UI.Image>()); Set(item,"_label",label); items.Add(item);
                if(i==0) PrefabUtility.SaveAsPrefabAsset(rect.gameObject,"Assets/Prefabs/Minigames/Pacifica/MangroveCard.prefab");
                rect.gameObject.SetActive(i<3);
            }
            foreach(var zone in zones) zone.gameObject.SetActive(zone.Stage==0);
            var discovery=Box("DiscoveryOverlay",root,Vector2.zero,new Vector2(1920,1080),new Color(0.02f,0.13f,0.15f,0.82f)); discovery.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var card=Box("DiscoveryCard",discovery,Vector2.zero,new Vector2(1120,700),new Color(1f,0.98f,0.88f));
            var discoveryTitle=Text("DiscoveryTitle",card,new Vector2(0,255),new Vector2(1040,85),"¡BUENA DECISIÓN!",40,Teal);
            var discoveryBody=Text("DiscoveryBody",card,new Vector2(0,10),new Vector2(980,350),"",30,Ink);
            var next=Button("ContinueButton",card,new Vector2(0,-255),new Vector2(550,85),"CONTINUAR RESTAURANDO"); discovery.gameObject.SetActive(false);
            var final=Box("FinalOverlay",root,Vector2.zero,new Vector2(1920,1080),new Color(0.02f,0.13f,0.15f,0.82f)); final.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var finalCard=Box("FinalCard",final,Vector2.zero,new Vector2(1200,800),new Color(1f,0.98f,0.88f));
            var resultTitle=Text("ResultTitle",finalCard,new Vector2(0,295),new Vector2(1100,100),"¡MANGLAR RESTAURADO!",42,Teal);
            var factTitle=Text("FactTitle",finalCard,new Vector2(0,160),new Vector2(1100,70),"Un ecosistema que protege la costa",32,Teal);
            var factBody=Text("FactBody",finalCard,new Vector2(0,-45),new Vector2(1060,330),data.winFact,29,Ink);
            var returnButton=Button("ReturnButton",finalCard,new Vector2(0,-300),new Vector2(790,90),"RECLAMAR SABIDURÍA Y VOLVER"); final.gameObject.SetActive(false);
            var back=Button("BackToHubButton",root,new Vector2(-690,460),new Vector2(420,70),"← MAPA PRINCIPAL");
            var gameGo=new GameObject("PacificMangroveGame",typeof(PacificMangroveGame)); Undo.RegisterCreatedObjectUndo(gameGo,"Crear restauración del manglar");var game=gameGo.GetComponent<PacificMangroveGame>();
            Set(game,"_dataSet",data); Array(game,"_items",items.ToArray());Array(game,"_zones",zones.ToArray());Set(game,"_dragLayer",dragLayer);Set(game,"_stageTitle",stageTitle);Set(game,"_instructions",instructions);Set(game,"_progress",progress);Set(game,"_feedback",feedback);Set(game,"_modelNote",modelNote);Set(game,"_discoveryPanel",discovery.gameObject);Set(game,"_discoveryTitle",discoveryTitle);Set(game,"_discoveryBody",discoveryBody);Set(game,"_continueButton",next);
            Set(game,"factPanel",final.gameObject);Set(game,"resultTitleText",resultTitle);Set(game,"factTitleText",factTitle);Set(game,"factBodyText",factBody);Set(game,"returnToHubButton",returnButton);Set(game,"returnButtonText",returnButton.GetComponentInChildren<TMPro.TextMeshProUGUI>());Set(game,"backToHubTopButton",back);
            var gameSo=new SerializedObject(game);gameSo.FindProperty("regionId").stringValue="Pacifica";gameSo.FindProperty("regionDisplayName").stringValue="Región Pacífica";gameSo.ApplyModifiedPropertiesWithoutUndo();modelNote.text=data.modelNote;
            var events=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));Undo.RegisterCreatedObjectUndo(events,"Crear eventos Pacífica");events.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,path);
        }
        private static void Add(PacificMangroveDataSet data,string id,string label,int stage,string target,Color color,string fact,string hint) {data.tasks.Add(new MangroveTaskData{id=id,label=label,stage=stage,targetId=target,color=color,fact=fact,hint=hint});}
        private static MangroveDropZone Zone(Transform parent,string id,int stage,Vector2 pos,Vector2 size,string label,Color color) {
            var rect=Box("Zone_"+id,parent,pos,size,color);rect.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;var text=Text("Label",rect,Vector2.zero,size-new Vector2(30,30),label,27,Ink);var zone=rect.gameObject.AddComponent<MangroveDropZone>();var so=new SerializedObject(zone);so.FindProperty("_targetId").stringValue=id;so.FindProperty("_stage").intValue=stage;so.FindProperty("_label").objectReferenceValue=text;so.FindProperty("_image").objectReferenceValue=rect.GetComponent<UnityEngine.UI.Image>();so.ApplyModifiedPropertiesWithoutUndo();return zone;
        }
        private static void Set(Object obj,string name,Object value) {var so=new SerializedObject(obj);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private static void Array<T>(Object obj,string name,T[] values) where T:Object {var so=new SerializedObject(obj);var array=so.FindProperty(name);array.arraySize=values.Length;for(int i=0;i<values.Length;i++) array.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
        private static void Folder(string path) {if(AssetDatabase.IsValidFolder(path))return;int slash=path.LastIndexOf('/');string parent=path.Substring(0,slash);Folder(parent);AssetDatabase.CreateFolder(parent,path.Substring(slash+1));}
        private static RectTransform Box(string name,Transform parent,Vector2 pos,Vector2 size,Color color) {var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));go.transform.SetParent(parent,false);var rect=(RectTransform)go.transform;rect.anchorMin=rect.anchorMax=new Vector2(0.5f,0.5f);rect.sizeDelta=size;rect.anchoredPosition=pos;var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return rect;}
        private static TMPro.TextMeshProUGUI Text(string name,Transform parent,Vector2 pos,Vector2 size,string label,int fontSize,Color color) {var go=new GameObject(name,typeof(RectTransform),typeof(TMPro.TextMeshProUGUI));go.transform.SetParent(parent,false);var rect=(RectTransform)go.transform;rect.anchorMin=rect.anchorMax=new Vector2(0.5f,0.5f);rect.sizeDelta=size;rect.anchoredPosition=pos;var text=go.GetComponent<TMPro.TextMeshProUGUI>();text.font=_font;text.text=label;text.fontSize=fontSize;text.color=color;text.alignment=TMPro.TextAlignmentOptions.Center;text.raycastTarget=false;return text;}
        private static UnityEngine.UI.Button Button(string name,Transform parent,Vector2 pos,Vector2 size,string label) {var rect=Box(name,parent,pos,size,Teal);rect.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;var button=rect.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=rect.GetComponent<UnityEngine.UI.Image>();Text("Label",rect,Vector2.zero,size-new Vector2(20,10),label,26,Color.white);return button;}
    }
}
