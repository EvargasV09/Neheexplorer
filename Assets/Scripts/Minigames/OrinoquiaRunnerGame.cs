using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer
{
    public class OrinoquiaRunnerGame : MinigameBase
    {
        [Header("Datos Educativos")]
        [SerializeField] private OrinoquiaRunnerDataSet dataSet;

        [Header("UI - Encabezado y Progreso")]
        [SerializeField] private TextMeshProUGUI gameTitleText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private TextMeshProUGUI rescuedText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private Image distanceProgressBar;

        [Header("UI - Pista y Sabana")]
        [SerializeField] private RectTransform trackArea;
        [SerializeField] private RectTransform playerRect;
        [SerializeField] private TextMeshProUGUI playerAvatarText;
        [SerializeField] private Transform entitiesContainer;

        [Header("UI - Cápsula de Sabiduría en Vivo")]
        [SerializeField] private GameObject factPopupPanel;
        [SerializeField] private TextMeshProUGUI factPopupTitle;
        [SerializeField] private TextMeshProUGUI factPopupBody;

        [Header("UI - Controles en Pantalla")]
        [SerializeField] private Button laneUpButton;
        [SerializeField] private Button laneDownButton;

        // Carriles Y (Superior, Centro, Inferior)
        private readonly float[] _laneYPositions = new float[] { 130f, 0f, -130f };
        private int _currentLane = 1; // Centro
        private float _currentY = 0f;

        // Estado del juego
        private float _currentDistance = 0f;
        private float _targetDistance = 1200f;
        private float _speed = 65f;
        private int _lives = 3;
        private int _rescuedCount = 0;
        private int _requiredRescued = 5;

        private bool _isInvulnerable = false;
        private float _invulnerableTimer = 0f;
        private float _spawnTimer = 0f;
        private float _spawnInterval = 1.35f;

        // Elementos activos en pantalla
        private class ActiveEntity
        {
            public GameObject gameObject;
            public RectTransform rect;
            public int lane;
            public bool isCollectible;
            public LlanosCollectibleData collectibleData;
            public LlanosObstacleData obstacleData;
        }

        private readonly List<ActiveEntity> _entities = new List<ActiveEntity>();
        private Coroutine _popupCoroutine;

        protected override void Awake()
        {
            base.Awake();
            regionId = "Orinoquia";
            regionDisplayName = "Región Orinoquía";

            if (laneUpButton != null)
            {
                laneUpButton.onClick.AddListener(MoveLaneUp);
            }

            if (laneDownButton != null)
            {
                laneDownButton.onClick.AddListener(MoveLaneDown);
            }
        }

        public override void StartGame()
        {
            base.StartGame();

            _currentDistance = 0f;
            _currentLane = 1;
            _currentY = _laneYPositions[1];
            _rescuedCount = 0;
            _spawnTimer = 0.5f;
            _isInvulnerable = false;

            if (dataSet != null)
            {
                factTitle = dataSet.winFactTitle;
                educationalFact = dataSet.winEducationalFact;
                _targetDistance = dataSet.totalDistance > 0 ? dataSet.totalDistance : 1200f;
                _speed = dataSet.scrollSpeed > 0 ? dataSet.scrollSpeed : 65f;
                _lives = dataSet.startingLives > 0 ? dataSet.startingLives : 3;
                _requiredRescued = dataSet.requiredFaunaCount > 0 ? dataSet.requiredFaunaCount : 5;

                if (gameTitleText != null) gameTitleText.text = dataSet.minigameTitle;
                if (instructionsText != null) instructionsText.text = dataSet.instructions;
            }
            else
            {
                _targetDistance = 1200f;
                _speed = 65f;
                _lives = 3;
                _requiredRescued = 5;
            }

            // Limpiar entidades previas
            ClearAllEntities();

            if (factPopupPanel != null)
            {
                factPopupPanel.SetActive(false);
            }

            UpdateUI();
            UpdatePlayerPosition(instant: true);
        }

        private void Update()
        {
            if (!isGameActive) return;

            HandleInput();
            UpdatePlayerMovement();
            UpdateSpawning();
            UpdateEntitiesMovement();
            UpdateDistanceProgress();

            if (_isInvulnerable)
            {
                _invulnerableTimer -= Time.deltaTime;
                if (_invulnerableTimer <= 0f)
                {
                    _isInvulnerable = false;
                    SetPlayerAlpha(1f);
                }
                else
                {
                    // Parpadeo visual
                    float alpha = Mathf.PingPong(Time.time * 8f, 0.6f) + 0.4f;
                    SetPlayerAlpha(alpha);
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
                    MoveLaneUp();
                }
                else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)
                {
                    MoveLaneDown();
                }
            }
        }

        public void MoveLaneUp()
        {
            if (!isGameActive) return;
            if (_currentLane > 0)
            {
                _currentLane--;
            }
        }

        public void MoveLaneDown()
        {
            if (!isGameActive) return;
            if (_currentLane < _laneYPositions.Length - 1)
            {
                _currentLane++;
            }
        }

        private void UpdatePlayerMovement()
        {
            if (playerRect == null) return;

            float targetY = _laneYPositions[_currentLane];
            _currentY = Mathf.MoveTowards(_currentY, targetY, Time.deltaTime * 650f);
            var pos = playerRect.anchoredPosition;
            pos.y = _currentY;
            playerRect.anchoredPosition = pos;
        }

        private void UpdatePlayerPosition(bool instant)
        {
            if (playerRect == null) return;
            _currentY = _laneYPositions[_currentLane];
            var pos = playerRect.anchoredPosition;
            pos.y = _currentY;
            playerRect.anchoredPosition = pos;
        }

        private void UpdateSpawning()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnRandomEntity();
                _spawnTimer = _spawnInterval;
            }
        }

        private void SpawnRandomEntity()
        {
            if (entitiesContainer == null) return;

            int lane = Random.Range(0, 3);
            bool isCollectible = Random.value < 0.55f;

            var entityGo = new GameObject(isCollectible ? "FaunaItem" : "ObstacleItem", typeof(RectTransform), typeof(Image));
            entityGo.transform.SetParent(entitiesContainer, false);

            var rt = entityGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90f, 90f);
            rt.anchoredPosition = new Vector2(620f, _laneYPositions[lane]);

            var bgImg = entityGo.GetComponent<Image>();

            // Icono / Emoji Text
            var textGo = new GameObject("Icon", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(entityGo.transform, false);
            var tRt = textGo.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 42;
            tmp.raycastTarget = false;

            ActiveEntity entity = new ActiveEntity
            {
                gameObject = entityGo,
                rect = rt,
                lane = lane,
                isCollectible = isCollectible
            };

            if (isCollectible)
            {
                var fauna = GetRandomFauna();
                entity.collectibleData = fauna;
                bgImg.color = fauna.itemColor;
                rt.sizeDelta = new Vector2(130f, 65f);
                tmp.text = string.IsNullOrEmpty(fauna.shortLabel) ? fauna.name : fauna.shortLabel;
                tmp.fontSize = 17;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            }
            else
            {
                var obs = GetRandomObstacle();
                entity.obstacleData = obs;
                bgImg.color = obs.obstacleColor;
                rt.sizeDelta = new Vector2(120f, 65f);
                tmp.text = string.IsNullOrEmpty(obs.shortLabel) ? obs.name : obs.shortLabel;
                tmp.fontSize = 17;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.white;
            }

            Sprite entitySprite = isCollectible ? entity.collectibleData.sprite : entity.obstacleData.sprite;
            if (entitySprite != null)
            {
                bgImg.sprite = entitySprite;
                bgImg.color = Color.white;
                bgImg.preserveAspect = true;
                bgImg.raycastTarget = false;
                rt.sizeDelta = new Vector2(130f, 100f);
                tmp.gameObject.SetActive(false);
            }

            _entities.Add(entity);
        }

        private void UpdateEntitiesMovement()
        {
            float playerX = playerRect != null ? playerRect.anchoredPosition.x : -380f;

            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                var entity = _entities[i];
                if (entity.gameObject == null)
                {
                    _entities.RemoveAt(i);
                    continue;
                }

                // Desplazamiento horizontal
                var pos = entity.rect.anchoredPosition;
                pos.x -= _speed * 5.5f * Time.deltaTime;
                entity.rect.anchoredPosition = pos;

                // Detección de colisión con el jugador
                if (entity.lane == _currentLane && Mathf.Abs(pos.x - playerX) < 65f)
                {
                    if (entity.isCollectible)
                    {
                        CollectEntity(entity);
                    }
                    else
                    {
                        HitObstacle(entity);
                    }

                    Destroy(entity.gameObject);
                    _entities.RemoveAt(i);
                    continue;
                }

                // Destruir si sale de pantalla por la izquierda
                if (pos.x < -680f)
                {
                    Destroy(entity.gameObject);
                    _entities.RemoveAt(i);
                }
            }
        }

        private void CollectEntity(ActiveEntity entity)
        {
            _rescuedCount++;
            UpdateUI();

            if (entity.collectibleData != null)
            {
                ShowFactPopup(
                    $"¡RESCATASTE: {entity.collectibleData.name.ToUpper()}!",
                    entity.collectibleData.educationalNote
                );
            }
        }

        private void HitObstacle(ActiveEntity entity)
        {
            if (_isInvulnerable) return;

            _lives--;
            _isInvulnerable = true;
            _invulnerableTimer = 1.25f;
            UpdateUI();

            if (_lives <= 0)
            {
                EndGame(won: false);
            }
        }

        private void UpdateDistanceProgress()
        {
            _currentDistance += _speed * Time.deltaTime;
            UpdateUI();

            if (_currentDistance >= _targetDistance)
            {
                bool won = _rescuedCount >= _requiredRescued;
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

            if (rescuedText != null)
            {
                rescuedText.text = $"Fauna Rescatada: {_rescuedCount} / {_requiredRescued}";
            }

            if (livesText != null)
            {
                livesText.text = $"Vidas: {_lives} / 3";
            }
        }

        private void ShowFactPopup(string title, string note)
        {
            if (factPopupPanel == null) return;

            if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
            _popupCoroutine = StartCoroutine(PopupRoutine(title, note));
        }

        private IEnumerator PopupRoutine(string title, string note)
        {
            factPopupPanel.SetActive(true);
            if (factPopupTitle != null) factPopupTitle.text = title;
            if (factPopupBody != null) factPopupBody.text = note;

            yield return new WaitForSeconds(3.2f);

            factPopupPanel.SetActive(false);
        }

        private void SetPlayerAlpha(float alpha)
        {
            if (playerRect != null)
            {
                var img = playerRect.GetComponent<Image>();
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
            foreach (var e in _entities)
            {
                if (e != null && e.gameObject != null)
                {
                    Destroy(e.gameObject);
                }
            }
            _entities.Clear();
        }

        public override void EndGame(bool won)
        {
            base.EndGame(won);

            if (resultTitleText != null)
            {
                resultTitleText.text = won
                    ? $"¡LLEGASTE AL HATO LLANERO! ({_rescuedCount} Animales Rescatados)"
                    : $"¡LA SABANA TE DESAFIÓ! ({_rescuedCount}/{_requiredRescued} Rescates)";
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

        private LlanosCollectibleData GetRandomFauna()
        {
            if (dataSet != null && dataSet.faunaList != null && dataSet.faunaList.Count > 0)
            {
                return dataSet.faunaList[Random.Range(0, dataSet.faunaList.Count)];
            }

            return new LlanosCollectibleData
            {
                name = "Chigüiro de los Esteros",
                emoji = "🦫",
                educationalNote = "Es el roedor más grande del mundo y vive en manadas junto a los esteros."
            };
        }

        private LlanosObstacleData GetRandomObstacle()
        {
            if (dataSet != null && dataSet.obstacleList != null && dataSet.obstacleList.Count > 0)
            {
                return dataSet.obstacleList[Random.Range(0, dataSet.obstacleList.Count)];
            }

            return new LlanosObstacleData
            {
                name = "Tronco Caído",
                emoji = "🪵"
            };
        }
    }
}
