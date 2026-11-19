using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Nehexplorer
{
    public class InsularFruitGame : MinigameBase
    {
        [Header("Datos y Jardín")]
        [SerializeField] private InsularFruitDataSet _dataSet;
        [SerializeField] private RectTransform _garden;
        [SerializeField] private RectTransform _collector;
        [SerializeField] private InsularFruitItem _fruitPrefab;
        [SerializeField] private TMPro.TextMeshProUGUI _progressText;
        [SerializeField] private TMPro.TextMeshProUGUI _albumText;
        [SerializeField] private TMPro.TextMeshProUGUI _catalogNoteText;
        [Header("Ficha")]
        [SerializeField] private GameObject _discoveryPanel;
        [SerializeField] private TMPro.TextMeshProUGUI _discoveryTitle;
        [SerializeField] private TMPro.TextMeshProUGUI _discoveryBody;
        [SerializeField] private UnityEngine.UI.Image _discoveryImage;
        [SerializeField] private UnityEngine.UI.Button _continueButton;

        private readonly List<InsularFruitItem> _items = new List<InsularFruitItem>();
        private readonly HashSet<string> _collectedIds = new HashSet<string>();
        private UnityEngine.InputSystem.InputAction _pointerAction;
        private Canvas _canvas;
        private Vector2 _target;
        private bool _hasTarget;
        private bool _reading;
        private bool _ready;
        public int CollectedCount => _collectedIds.Count;
        public bool IsReading => _reading;
        public bool IsComplete => hasWon;

        protected override void Awake()
        {
            base.Awake();
            regionId = "Insular";
            regionDisplayName = "Región Insular";
            _canvas = _garden != null ? _garden.GetComponentInParent<Canvas>() : null;
            _pointerAction = new UnityEngine.InputSystem.InputAction("InsularPointer", UnityEngine.InputSystem.InputActionType.Value, "<Mouse>/position");
            _pointerAction.performed += OnPointerMoved;
            if (_continueButton != null) _continueButton.onClick.AddListener(ContinueDiscovery);
            if (_discoveryPanel != null) _discoveryPanel.SetActive(false);
        }

        private void OnEnable() { if (_pointerAction != null) _pointerAction.Enable(); }
        private void OnDisable() { if (_pointerAction != null) _pointerAction.Disable(); _hasTarget = false; }
        private void OnDestroy()
        {
            if (_pointerAction != null) { _pointerAction.performed -= OnPointerMoved; _pointerAction.Dispose(); }
            if (_continueButton != null) _continueButton.onClick.RemoveListener(ContinueDiscovery);
        }

        public override void StartGame()
        {
            base.StartGame();
            _ready = false;
            _reading = false;
            _hasTarget = false;
            _collectedIds.Clear();
            foreach (var item in _items) if (item != null) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
            _items.Clear();
            if (_discoveryPanel != null) _discoveryPanel.SetActive(false);
            if (_dataSet == null || _dataSet.fruits == null || _dataSet.fruits.Count == 0 || _garden == null || _collector == null || _fruitPrefab == null || _canvas == null || _discoveryPanel == null || _continueButton == null)
            {
                ConfigurationError("Faltan datos o referencias de la colección."); return;
            }
            var ids = new HashSet<string>();
            foreach (var fruit in _dataSet.fruits)
            {
                if (fruit == null || string.IsNullOrWhiteSpace(fruit.id) || string.IsNullOrWhiteSpace(fruit.displayName) || !ids.Add(fruit.id))
                { ConfigurationError("El catálogo contiene frutas sin identificador/nombre o identificadores repetidos."); return; }
            }
            Canvas.ForceUpdateCanvases();
            _collector.anchoredPosition = Vector2.zero;
            _target = Vector2.zero;
            int count = _dataSet.fruits.Count;
            var bounds = _garden.rect;
            // Reparto circular: todas las frutas están disponibles, lejos del punto de inicio.
            for (int i = 0; i < count; i++)
            {
                float angle = Mathf.PI * 2f * i / count;
                var item = Instantiate(_fruitPrefab, _garden);
                item.Configure(_dataSet.fruits[i]);
                float marginX = item.Rect.rect.width * 0.5f + 24f;
                float marginY = item.Rect.rect.height * 0.5f + 24f;
                if (bounds.width <= marginX * 2f || bounds.height <= marginY * 2f)
                { ConfigurationError("El jardín es demasiado pequeño para las frutas."); return; }
                item.Rect.anchoredPosition = new Vector2(Mathf.Cos(angle) * (bounds.width * 0.5f - marginX), Mathf.Sin(angle) * (bounds.height * 0.5f - marginY));
                _items.Add(item);
            }
            _collector.SetAsLastSibling();
            educationalFact = _dataSet.winFact;
            factTitle = "Diversidad que nos alimenta";
            if (_catalogNoteText != null) _catalogNoteText.text = _dataSet.catalogNote;
            _ready = true;
            RefreshAlbum();
        }

        private void ConfigurationError(string message)
        {
            isGameActive = false;
            Debug.LogError("[InsularFruitGame] " + message, this);
            if (_progressText != null) _progressText.text = "Configuración incompleta: vuelve al mapa";
        }

        private void OnPointerMoved(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!_ready || !isGameActive || _reading || !Application.isFocused) return;
            var screenPoint = context.ReadValue<Vector2>();
            var camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_garden, screenPoint, camera, out local) || !_garden.rect.Contains(local))
            { _hasTarget = false; return; }
            var size = _collector.rect.size * 0.5f;
            var rect = _garden.rect;
            _target = new Vector2(Mathf.Clamp(local.x, rect.xMin + size.x, rect.xMax - size.x), Mathf.Clamp(local.y, rect.yMin + size.y, rect.yMax - size.y));
            _hasTarget = true;
        }

        private void Update()
        {
            if (!_ready || !isGameActive || _reading || !_hasTarget || !Application.isFocused) return;
            Vector2 before = _collector.anchoredPosition;
            Vector2 after = Vector2.MoveTowards(before, _target, _dataSet.moveSpeed * Time.deltaTime);
            _collector.anchoredPosition = after;
            foreach (var item in _items)
            {
                if (item == null || item.Collected) continue;
                Vector2 segment = after - before;
                float t = segment.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(item.Rect.anchoredPosition - before, segment) / segment.sqrMagnitude) : 0f;
                float distance = Vector2.Distance(before + segment * t, item.Rect.anchoredPosition);
                if (distance <= _dataSet.collectionRadius) { Collect(item); break; }
            }
        }

        private void Collect(InsularFruitItem item)
        {
            if (!_collectedIds.Add(item.Data.id)) return;
            item.Collect();
            _reading = true;
            _hasTarget = false;
            RefreshAlbum();
            var data = item.Data;
            _discoveryTitle.text = data.displayName.ToUpperInvariant();
            _discoveryBody.text = "<i>" + data.scientificName + "</i>\n\n<b>DIVERSIDAD</b>\n" + data.diversityFact + "\n\n<b>CONOCE SUS USOS</b>\n" + data.useFact;
            _discoveryImage.sprite = data.sprite;
            _discoveryImage.color = data.sprite == null ? data.color : Color.white;
            _discoveryImage.preserveAspect = true;
            _discoveryPanel.SetActive(true);
        }

        public void ContinueDiscovery()
        {
            if (!_reading) return;
            _reading = false;
            _hasTarget = false;
            _target = _collector.anchoredPosition;
            _discoveryPanel.SetActive(false);
            if (_collectedIds.Count == _dataSet.fruits.Count) EndGame(true);
        }

        private void RefreshAlbum()
        {
            if (_progressText != null) _progressText.text = "Variedades: " + _collectedIds.Count + " / " + _dataSet.fruits.Count;
            var builder = new StringBuilder();
            foreach (var fruit in _dataSet.fruits) builder.Append(_collectedIds.Contains(fruit.id) ? "[OK] " : "[  ] ").Append(fruit.displayName).Append("\n\n");
            if (_albumText != null) _albumText.text = builder.ToString();
        }

        public override void EndGame(bool won)
        {
            _hasTarget = false;
            base.EndGame(won);
            if (won && resultTitleText != null) resultTitleText.text = _dataSet.winTitle;
        }
    }
}
