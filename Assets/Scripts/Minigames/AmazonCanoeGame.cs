using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer
{
    /// <summary>
    /// Minijuego 4: Expedición en Canoa por el Río Amazonas.
    /// Navegación fluvial, fotografía de biodiversidad amazónica y evasión de obstáculos.
    /// </summary>
    public class AmazonCanoeGame : MinigameBase
    {
        [Header("Datos Educativos")]
        [SerializeField] private AmazonExpeditionDataSet dataSet;

        [Header("UI - Encabezado y Progreso")]
        [SerializeField] private TextMeshProUGUI gameTitleText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private TextMeshProUGUI photosCountText;
        [SerializeField] private TextMeshProUGUI hullText;
        [SerializeField] private Image distanceProgressBar;

        [Header("UI - Área Fluvial y Canoa")]
        [SerializeField] private RectTransform riverArea;
        [SerializeField] private RectTransform canoeRect;
        [SerializeField] private TextMeshProUGUI canoeLabelText;
        [SerializeField] private Transform riverEntitiesContainer;
        [SerializeField] private RectTransform viewfinderRect;
        [SerializeField] private Image cameraFlashImage;

        [Header("UI - Tarjeta de Descubrimiento Fotográfico")]
        [SerializeField] private GameObject discoveryPopupPanel;
        [SerializeField] private TextMeshProUGUI discoveryTitleText;
        [SerializeField] private TextMeshProUGUI discoveryHabitatText;
        [SerializeField] private TextMeshProUGUI discoveryNoteText;

        [Header("UI - Controles de Navegación y Cámara")]
        [SerializeField] private Button steerUpButton;
        [SerializeField] private Button steerDownButton;
        [SerializeField] private Button capturePhotoButton;

        // Carriles Y en el río: 0 = Ribera Norte (selva alta), 1 = Canal Central (río abierto), 2 = Ribera Sur (lagunas y ribera baja)
        private readonly float[] _laneYPositions = new float[] { 135f, 0f, -135f };
        private int _currentLane = 1;
        private float _currentY = 0f;

        // Parámetros y estado de la partida
        private float _currentDistance = 0f;
        private float _targetDistance = 1200f;
        private float _speed = 55f;
        private int _hullIntegrity = 3;
        private int _photosTaken = 0;
        private int _requiredPhotos = 5;

        private bool _isInvulnerable = false;
        private float _invulnerableTimer = 0f;
        private float _spawnTimer = 0f;
        private float _spawnInterval = 1.4f;

        // Especies ya fotografiadas en esta partida para evitar duplicados inmediatos
        private readonly HashSet<string> _photographedIds = new HashSet<string>();

        // Entidades activas en el río
        private class ActiveRiverEntity
        {
            public GameObject gameObject;
            public RectTransform rect;
            public int lane;
            public bool isSpecies;
            public bool wasPhotographed;
            public AmazonSpeciesData speciesData;
            public AmazonHazardData hazardData;
        }

        private readonly List<ActiveRiverEntity> _activeEntities = new List<ActiveRiverEntity>();
        private Coroutine _popupCoroutine;
        private Coroutine _flashCoroutine;

        protected override void Awake()
        {
            base.Awake();
            regionId = "Amazonica";
            regionDisplayName = "Región Amazónica";

            if (steerUpButton != null)
            {
                steerUpButton.onClick.AddListener(SteerUp);
            }

            if (steerDownButton != null)
            {
                steerDownButton.onClick.AddListener(SteerDown);
            }

            if (capturePhotoButton != null)
            {
                capturePhotoButton.onClick.AddListener(TriggerPhotoCapture);
            }
        }

        public override void StartGame()
        {
            base.StartGame();

            _currentDistance = 0f;
            _currentLane = 1;
            _currentY = _laneYPositions[1];
            _photosTaken = 0;
            _spawnTimer = 0.5f;
            _isInvulnerable = false;
            _photographedIds.Clear();

            if (dataSet != null)
            {
                factTitle = dataSet.winFactTitle;
                educationalFact = dataSet.winEducationalFact;
                _targetDistance = dataSet.expeditionDistance > 0 ? dataSet.expeditionDistance : 1200f;
                _speed = dataSet.scrollSpeed > 0 ? dataSet.scrollSpeed : 55f;
                _hullIntegrity = dataSet.startingHullIntegrity > 0 ? dataSet.startingHullIntegrity : 3;
                _requiredPhotos = dataSet.requiredSpeciesPhotos > 0 ? dataSet.requiredSpeciesPhotos : 5;

                if (gameTitleText != null) gameTitleText.text = dataSet.minigameTitle;
                if (instructionsText != null) instructionsText.text = dataSet.instructions;
            }
            else
            {
                _targetDistance = 1200f;
                _speed = 55f;
                _hullIntegrity = 3;
                _requiredPhotos = 5;
            }

            ClearAllEntities();

            if (discoveryPopupPanel != null)
            {
                discoveryPopupPanel.SetActive(false);
            }

            if (cameraFlashImage != null)
            {
                cameraFlashImage.gameObject.SetActive(false);
            }

            UpdateUI();
            UpdateCanoePosition(instant: true);
        }

        private void Update()
        {
            if (!isGameActive) return;

            HandleInput();
            UpdateCanoeMovement();
            UpdateSpawning();
            UpdateEntitiesMovement();
            UpdateRiverProgress();

            if (_isInvulnerable)
            {
                _invulnerableTimer -= Time.deltaTime;
                if (_invulnerableTimer <= 0f)
                {
                    _isInvulnerable = false;
                    SetCanoeAlpha(1f);
                }
                else
                {
                    float alpha = Mathf.PingPong(Time.time * 8f, 0.6f) + 0.4f;
                    SetCanoeAlpha(alpha);
                }
            }
        }

        private void HandleInput()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
                {
                    SteerUp();
                }
                else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)
                {
                    SteerDown();
                }

                if (kb.spaceKey.wasPressedThisFrame)
                {
                    TriggerPhotoCapture();
                }
            }
        }

        public void SteerUp()
        {
            if (!isGameActive) return;
            if (_currentLane > 0)
            {
                _currentLane--;
            }
        }

        public void SteerDown()
        {
            if (!isGameActive) return;
            if (_currentLane < _laneYPositions.Length - 1)
            {
                _currentLane++;
            }
        }

        public void TriggerPhotoCapture()
        {
            if (!isGameActive) return;

            // Destello de obturador fotográfico
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(CameraFlashRoutine());

            // Buscar si hay alguna especie dentro del rango de captura fotográfica
            float photoCenterX = canoeRect != null ? canoeRect.anchoredPosition.x + 180f : -200f;
            float photoToleranceX = 260f;

            ActiveRiverEntity captured = null;
            float closestDist = float.MaxValue;

            foreach (var entity in _activeEntities)
            {
                if (entity == null || entity.gameObject == null) continue;
                if (!entity.isSpecies || entity.wasPhotographed) continue;

                float dist = Mathf.Abs(entity.rect.anchoredPosition.x - photoCenterX);
                if (dist < photoToleranceX && dist < closestDist)
                {
                    closestDist = dist;
                    captured = entity;
                }
            }

            if (captured != null && captured.speciesData != null)
            {
                captured.wasPhotographed = true;
                _photosTaken++;
                _photographedIds.Add(captured.speciesData.id);

                // Feedback visual en la entidad (tinte verde brillante de éxito fotográfico)
                var bg = captured.gameObject.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = new Color(0.45f, 1f, 0.65f, 1f);
                }
                var lbl = captured.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                if (lbl != null)
                {
                    lbl.text = "¡FOTO!";
                    lbl.color = new Color(0.1f, 0.9f, 0.3f, 1f);
                }

                UpdateUI();

                ShowDiscoveryPopup(
                    $"¡FOTO REGISTRADA: {captured.speciesData.name.ToUpper()}!",
                    $"Hábitat: {captured.speciesData.habitat} ({captured.speciesData.scientificName})",
                    captured.speciesData.educationalNote
                );
            }
        }

        private IEnumerator CameraFlashRoutine()
        {
            if (cameraFlashImage != null)
            {
                cameraFlashImage.gameObject.SetActive(true);
                cameraFlashImage.color = new Color(1f, 1f, 1f, 0.75f);

                float elapsed = 0f;
                float duration = 0.22f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(0.75f, 0f, elapsed / duration);
                    cameraFlashImage.color = new Color(1f, 1f, 1f, alpha);
                    yield return null;
                }

                cameraFlashImage.gameObject.SetActive(false);
            }
        }

        private void UpdateCanoeMovement()
        {
            if (canoeRect == null) return;

            float targetY = _laneYPositions[_currentLane];
            _currentY = Mathf.MoveTowards(_currentY, targetY, Time.deltaTime * 580f);
            var pos = canoeRect.anchoredPosition;
            pos.y = _currentY;
            canoeRect.anchoredPosition = pos;

            if (viewfinderRect != null)
            {
                var vfPos = viewfinderRect.anchoredPosition;
                vfPos.y = _currentY;
                viewfinderRect.anchoredPosition = vfPos;
            }
        }

        private void UpdateCanoePosition(bool instant)
        {
            if (canoeRect == null) return;
            _currentY = _laneYPositions[_currentLane];
            var pos = canoeRect.anchoredPosition;
            pos.y = _currentY;
            canoeRect.anchoredPosition = pos;

            if (viewfinderRect != null)
            {
                var vfPos = viewfinderRect.anchoredPosition;
                vfPos.y = _currentY;
                viewfinderRect.anchoredPosition = vfPos;
            }
        }

        private void UpdateSpawning()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnRiverEntity();
                _spawnTimer = _spawnInterval;
            }
        }

        private void SpawnRiverEntity()
        {
            if (riverEntitiesContainer == null) return;

            int lane = Random.Range(0, 3);
            bool isSpecies = Random.value < 0.60f;

            var entityGo = new GameObject(isSpecies ? "SpeciesItem" : "RiverHazard", typeof(RectTransform), typeof(Image));
            entityGo.transform.SetParent(riverEntitiesContainer, false);

            var rt = entityGo.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(720f, _laneYPositions[lane]);

            var bgImg = entityGo.GetComponent<Image>();
            bgImg.preserveAspect = true;
            bgImg.raycastTarget = false;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(entityGo.transform, false);
            var tRt = textGo.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(8f, 4f); tRt.offsetMax = new Vector2(-8f, -4f);
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = 17;
            tmp.raycastTarget = false;

            ActiveRiverEntity entity = new ActiveRiverEntity
            {
                gameObject = entityGo,
                rect = rt,
                lane = lane,
                isSpecies = isSpecies,
                wasPhotographed = false
            };

            if (isSpecies)
            {
                var species = GetRandomSpecies();
                entity.speciesData = species;
                if (species.customSprite != null)
                {
                    bgImg.sprite = species.customSprite;
                    bgImg.color = Color.white;
                    rt.sizeDelta = new Vector2(110f, 75f);
                    tmp.text = ""; // Sprite limpio visible
                }
                else
                {
                    bgImg.color = species.cardColor;
                    rt.sizeDelta = new Vector2(150f, 65f);
                    tmp.text = species.shortLabel;
                    tmp.color = new Color(0.10f, 0.12f, 0.16f, 1f);
                }
            }
            else
            {
                var hazard = GetRandomHazard();
                entity.hazardData = hazard;
                if (hazard.customSprite != null)
                {
                    bgImg.sprite = hazard.customSprite;
                    bgImg.color = Color.white;
                    rt.sizeDelta = new Vector2(100f, 75f);
                    tmp.text = ""; // Sprite limpio visible
                }
                else
                {
                    bgImg.color = hazard.hazardColor;
                    rt.sizeDelta = new Vector2(140f, 65f);
                    tmp.text = hazard.shortLabel;
                    tmp.color = Color.white;
                }
            }

            _activeEntities.Add(entity);
        }

        private void UpdateEntitiesMovement()
        {
            float canoeX = canoeRect != null ? canoeRect.anchoredPosition.x : -380f;

            for (int i = _activeEntities.Count - 1; i >= 0; i--)
            {
                var entity = _activeEntities[i];
                if (entity == null || entity.gameObject == null)
                {
                    _activeEntities.RemoveAt(i);
                    continue;
                }

                var pos = entity.rect.anchoredPosition;
                pos.x -= _speed * 5.2f * Time.deltaTime;
                entity.rect.anchoredPosition = pos;

                // Colisión física con la canoa (solo los obstáculos dañan la canoa)
                if (entity.lane == _currentLane && Mathf.Abs(pos.x - canoeX) < 70f)
                {
                    if (!entity.isSpecies)
                    {
                        HitRiverHazard(entity);
                        Destroy(entity.gameObject);
                        _activeEntities.RemoveAt(i);
                        continue;
                    }
                }

                // Destruir al salir de pantalla
                if (pos.x < -720f)
                {
                    Destroy(entity.gameObject);
                    _activeEntities.RemoveAt(i);
                }
            }
        }

        private void HitRiverHazard(ActiveRiverEntity hazard)
        {
            if (_isInvulnerable) return;

            _hullIntegrity--;
            _isInvulnerable = true;
            _invulnerableTimer = 1.3f;
            UpdateUI();

            if (_hullIntegrity <= 0)
            {
                EndGame(won: false);
            }
        }

        private void UpdateRiverProgress()
        {
            _currentDistance += _speed * Time.deltaTime;
            UpdateUI();

            if (_currentDistance >= _targetDistance)
            {
                bool won = _photosTaken >= _requiredPhotos;
                EndGame(won);
            }
        }

        private void UpdateUI()
        {
            if (distanceText != null)
            {
                int cur = Mathf.Min(Mathf.RoundToInt(_currentDistance), Mathf.RoundToInt(_targetDistance));
                distanceText.text = $"Distancia: {cur}m / {Mathf.RoundToInt(_targetDistance)}m";
            }

            if (distanceProgressBar != null)
            {
                distanceProgressBar.fillAmount = Mathf.Clamp01(_currentDistance / _targetDistance);
            }

            if (photosCountText != null)
            {
                photosCountText.text = $"Fotos de Especies: {_photosTaken} / {_requiredPhotos}";
            }

            if (hullText != null)
            {
                hullText.text = $"Integridad Canoa: {_hullIntegrity} / 3";
            }
        }

        private void ShowDiscoveryPopup(string title, string habitat, string note)
        {
            if (discoveryPopupPanel == null) return;

            if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
            _popupCoroutine = StartCoroutine(DiscoveryPopupRoutine(title, habitat, note));
        }

        private IEnumerator DiscoveryPopupRoutine(string title, string habitat, string note)
        {
            discoveryPopupPanel.SetActive(true);
            if (discoveryTitleText != null) discoveryTitleText.text = title;
            if (discoveryHabitatText != null) discoveryHabitatText.text = habitat;
            if (discoveryNoteText != null) discoveryNoteText.text = note;

            yield return new WaitForSeconds(3.6f);

            discoveryPopupPanel.SetActive(false);
        }

        private void SetCanoeAlpha(float alpha)
        {
            if (canoeRect != null)
            {
                var img = canoeRect.GetComponent<Image>();
                if (img != null)
                {
                    var c = img.color;
                    c.a = alpha;
                    img.color = c;
                }
            }
        }

        private void ClearAllEntities()
        {
            foreach (var e in _activeEntities)
            {
                if (e != null && e.gameObject != null)
                {
                    Destroy(e.gameObject);
                }
            }
            _activeEntities.Clear();
        }

        public override void EndGame(bool won)
        {
            base.EndGame(won);

            if (resultTitleText != null)
            {
                resultTitleText.text = won
                    ? $"¡EXPEDICIÓN EXITOSA EN PUERTO NARIÑO! ({_photosTaken} Especies Registradas)"
                    : $"¡LA CORRIENTE DETUVO LA CANOA! ({_photosTaken}/{_requiredPhotos} Fotos - Se requerían {_requiredPhotos})";
            }

            if (returnButtonText != null)
            {
                returnButtonText.text = won ? "RECLAMAR SABIDURÍA Y VOLVER" : "INTENTAR DE NUEVO";
            }
        }

        protected override void OnReturnToHubClicked()
        {
            if (hasWon)
            {
                base.OnReturnToHubClicked();
            }
            else
            {
                StartGame();
            }
        }

        private AmazonSpeciesData GetRandomSpecies()
        {
            if (dataSet != null && dataSet.speciesCatalog != null && dataSet.speciesCatalog.Count > 0)
            {
                // Priorizar especies que aún no hayan sido fotografiadas
                var unphotographed = dataSet.speciesCatalog.FindAll(s => !_photographedIds.Contains(s.id));
                if (unphotographed.Count > 0)
                {
                    return unphotographed[Random.Range(0, unphotographed.Count)];
                }
                return dataSet.speciesCatalog[Random.Range(0, dataSet.speciesCatalog.Count)];
            }

            return new AmazonSpeciesData
            {
                name = "Delfín Rosado",
                shortLabel = "DELFÍN ROSADO",
                habitat = "Río Amazonas",
                scientificName = "Inia geoffrensis",
                educationalNote = "Delfín fluvial insignia de la Amazonía."
            };
        }

        private AmazonHazardData GetRandomHazard()
        {
            if (dataSet != null && dataSet.riverHazards != null && dataSet.riverHazards.Count > 0)
            {
                return dataSet.riverHazards[Random.Range(0, dataSet.riverHazards.Count)];
            }

            return new AmazonHazardData
            {
                name = "Remolino Fluvial",
                shortLabel = "REMOLINO"
            };
        }
    }
}
