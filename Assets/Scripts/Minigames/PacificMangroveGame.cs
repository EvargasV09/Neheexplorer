using System.Collections.Generic;
using UnityEngine;

namespace Nehexplorer
{
    public class PacificMangroveGame : MinigameBase
    {
        [SerializeField] private PacificMangroveDataSet _dataSet;
        [SerializeField] private MangroveDragItem[] _items;
        [SerializeField] private MangroveDropZone[] _zones;
        [SerializeField] private RectTransform _dragLayer;
        [SerializeField] private TMPro.TextMeshProUGUI _stageTitle;
        [SerializeField] private TMPro.TextMeshProUGUI _instructions;
        [SerializeField] private TMPro.TextMeshProUGUI _progress;
        [SerializeField] private TMPro.TextMeshProUGUI _feedback;
        [SerializeField] private TMPro.TextMeshProUGUI _modelNote;
        [SerializeField] private GameObject _discoveryPanel;
        [SerializeField] private TMPro.TextMeshProUGUI _discoveryTitle;
        [SerializeField] private TMPro.TextMeshProUGUI _discoveryBody;
        [SerializeField] private UnityEngine.UI.Button _continueButton;
        private readonly HashSet<string> _completed = new HashSet<string>();
        private int _stage;
        private bool _reading;
        private bool _ready;
        public int CompletedCount => _completed.Count;
        public int CurrentStage => _stage;
        public bool IsReading => _reading;
        public bool IsComplete => hasWon;
        public bool CanInteract => _ready && isGameActive && !_reading;

        protected override void Awake()
        {
            base.Awake(); regionId="Pacifica"; regionDisplayName="Región Pacífica";
            if(_continueButton!=null) _continueButton.onClick.AddListener(ContinueRestoration);
        }
        private void OnDestroy() {if(_continueButton!=null) _continueButton.onClick.RemoveListener(ContinueRestoration);}
        public override void StartGame()
        {
            base.StartGame(); _ready=false; _stage=0; _reading=false; _completed.Clear();
            if(_dataSet==null||_items==null||_zones==null||_dragLayer==null||_discoveryPanel==null||_continueButton==null||_dataSet.tasks.Count!=_items.Length||_items.Length==0||_dataSet.stageTitles.Length!=3||_dataSet.stageInstructions.Length!=3)
            {FailConfiguration("Referencias o catálogo incompletos."); return;}
            var ids=new HashSet<string>(); int[] perStage=new int[3];
            for(int i=0;i<_items.Length;i++) {
                var task=_dataSet.tasks[i];
                if(_items[i]==null||task==null||string.IsNullOrWhiteSpace(task.id)||!ids.Add(task.id)||task.stage<0||task.stage>2||!HasTarget(task.targetId,task.stage)) {FailConfiguration("Tarea o destino inválido.");return;}
                perStage[task.stage]++;
                _items[i].Bind(task,_dragLayer,()=>CanInteract,TryPlace);
                _items[i].ResetItem();
            }
            for(int i=0;i<3;i++) if(perStage[i]==0) {FailConfiguration("Falta una etapa.");return;}
            foreach(var zone in _zones) zone.ResetZone();
            _discoveryPanel.SetActive(false); educationalFact=_dataSet.winFact; factTitle="Un ecosistema que protege la costa";
            if(_modelNote!=null) _modelNote.text=_dataSet.modelNote;
            _ready=true; UpdateStage();
        }
        private bool HasTarget(string id,int stage) {foreach(var zone in _zones) if(zone!=null&&zone.TargetId==id&&zone.Stage==stage) return true; return false;}
        private void FailConfiguration(string message) {isGameActive=false; Debug.LogError("[PacificMangroveGame] "+message,this); if(_feedback!=null) _feedback.text="Configuración incompleta. Vuelve al mapa.";}
        public bool TryPlace(MangroveDragItem item,string targetId)
        {
            if(!CanInteract||item==null||item.Data==null||item.Data.stage!=_stage||_completed.Contains(item.Data.id)) return false;
            bool known=false; foreach(var candidate in _items) if(candidate==item) {known=true;break;}
            if(!known) return false;
            if(targetId!=item.Data.targetId||!HasTarget(targetId,_stage)) {
                _feedback.text="Prueba otra zona. "+item.Data.hint; return false;
            }
            _completed.Add(item.Data.id);
            foreach(var zone in _zones) if(zone.TargetId==targetId&&zone.Stage==_stage) zone.MarkPlaced(item.Data.label);
            _reading=true; _progress.text="Restauración: "+_completed.Count+" / "+_items.Length;
            _discoveryTitle.text="¡BUENA DECISIÓN!";
            _discoveryBody.text=item.Data.fact;
            _discoveryPanel.SetActive(true);
            _feedback.text="Lee el dato y pulsa Continuar.";
            return true;
        }
        public void ContinueRestoration()
        {
            if(!_reading||!isGameActive) return;
            _reading=false; _discoveryPanel.SetActive(false);
            if(_completed.Count==_items.Length) {EndGame(true); return;}
            bool stageDone=true; foreach(var item in _items) if(item.Data.stage==_stage&&!_completed.Contains(item.Data.id)) stageDone=false;
            if(stageDone) _stage++;
            UpdateStage();
        }
        private void UpdateStage()
        {
            _stageTitle.text=_dataSet.stageTitles[_stage]; _instructions.text=_dataSet.stageInstructions[_stage];
            _progress.text="Restauración: "+_completed.Count+" / "+_items.Length; _feedback.text="Arrastra una tarjeta y suéltala en su destino. Si no coincide, vuelve a intentarlo.";
            foreach(var item in _items) item.gameObject.SetActive(item.Data.stage==_stage&&!_completed.Contains(item.Data.id));
            foreach(var zone in _zones) zone.gameObject.SetActive(zone.Stage==_stage);
        }
        public override void EndGame(bool won)
        {
            base.EndGame(won); if(won&&resultTitleText!=null) resultTitleText.text="¡MANGLAR RESTAURADO!";
        }
    }
}
