using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer
{
    /// <summary>
    /// Minijuego 1: Memoria del Caribe (4x3 cartas = 6 pares de Especie + Hábitat).
    /// </summary>
    public class CaribbeanMemoryGame : MinigameBase
    {
        [Header("Datos Educativos")]
        [Tooltip("ScriptableObject con los pares y datos educativos. Editable por docentes.")]
        [SerializeField] private MemoryGameDataSet dataSet;

        [Header("Estructura de la Cuadrícula")]
        [SerializeField] private Transform cardsGridContainer;
        [SerializeField] private GameObject cardPrefab;

        [Header("Contadores y Feedback")]
        [SerializeField] private TextMeshProUGUI gameTitleText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private TextMeshProUGUI attemptsText;
        [SerializeField] private TextMeshProUGUI pairsFoundText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [Tooltip("Segundos que permanece visible el nombre al revelar una carta.")]
        [SerializeField] private float cardNameDuration = 1.5f;

        private Coroutine _cardNameRoutine;
        private List<MemoryCardUI> _activeCards = new List<MemoryCardUI>();
        private MemoryCardUI _firstCard;
        private MemoryCardUI _secondCard;
        private bool _isProcessingTurn = false;
        private int _attempts = 0;
        private int _pairsFound = 0;
        private int _totalPairs = 6;

        protected override void Awake()
        {
            base.Awake();
            regionId = "Caribe";
            regionDisplayName = "Región Caribe";
        }

        public override void StartGame()
        {
            base.StartGame();

            _attempts = 0;
            _pairsFound = 0;
            _firstCard = null;
            _secondCard = null;
            _isProcessingTurn = false;

            if (dataSet != null)
            {
                factTitle = dataSet.winFactTitle;
                educationalFact = dataSet.winEducationalFact;

                if (gameTitleText != null) gameTitleText.text = dataSet.minigameTitle;
                if (instructionsText != null) instructionsText.text = dataSet.instructions;
            }

            UpdateUI();
            BuildAndShuffleBoard();
        }

        private void BuildAndShuffleBoard()
        {
            if (cardsGridContainer == null) return;

            // Limpiar cartas previas
            foreach (Transform child in cardsGridContainer)
            {
                Destroy(child.gameObject);
            }
            _activeCards.Clear();

            // Recolectar las cartas de los pares
            List<MemoryCardItem> itemsToPlace = new List<MemoryCardItem>();

            if (dataSet != null && dataSet.pairs != null && dataSet.pairs.Count > 0)
            {
                int count = Mathf.Min(dataSet.pairs.Count, 6);
                _totalPairs = count;

                for (int i = 0; i < count; i++)
                {
                    var pair = dataSet.pairs[i];
                    itemsToPlace.Add(pair.itemA);
                    itemsToPlace.Add(pair.itemB);
                }
            }
            else
            {
                // Fallback automático por si no se asignó el ScriptableObject en el inspector
                itemsToPlace = GetFallbackCaribbeanItems();
                _totalPairs = itemsToPlace.Count / 2;
            }

            // Barajar usando Fisher-Yates
            for (int i = 0; i < itemsToPlace.Count; i++)
            {
                int rnd = Random.Range(i, itemsToPlace.Count);
                var temp = itemsToPlace[i];
                itemsToPlace[i] = itemsToPlace[rnd];
                itemsToPlace[rnd] = temp;
            }

            // Instanciar cartas en la cuadrícula
            foreach (var item in itemsToPlace)
            {
                GameObject cardGo;
                if (cardPrefab != null)
                {
                    cardGo = Instantiate(cardPrefab, cardsGridContainer);
                }
                else
                {
                    cardGo = CreateProceduralCard(cardsGridContainer);
                }

                var cardUI = cardGo.GetComponent<MemoryCardUI>();
                cardUI.Initialize(item, OnCardClicked);
                _activeCards.Add(cardUI);
            }

            if (_cardNameRoutine != null)
            {
                StopCoroutine(_cardNameRoutine);
                _cardNameRoutine = null;
            }
            if (feedbackText != null) feedbackText.text = string.Empty;
        }

        private void ShowCardName(string cardName)
        {
            if (feedbackText == null) return;
            if (_cardNameRoutine != null) StopCoroutine(_cardNameRoutine);
            _cardNameRoutine = StartCoroutine(ShowCardNameRoutine(cardName));
        }

        private IEnumerator ShowCardNameRoutine(string cardName)
        {
            feedbackText.text = cardName;
            feedbackText.color = new Color(0.98f, 0.88f, 0.45f);
            yield return new WaitForSecondsRealtime(cardNameDuration);
            feedbackText.text = string.Empty;
            _cardNameRoutine = null;
        }

        private void OnCardClicked(MemoryCardUI card)
        {
            if (!isGameActive || _isProcessingTurn) return;
            if (!card.IsInteractable) return;

            if (_firstCard == null)
            {
                _firstCard = card;
                _firstCard.FlipUp(() => ShowCardName(card.ItemData.title));
            }
            else if (_secondCard == null && card != _firstCard)
            {
                _secondCard = card;
                _secondCard.FlipUp(() => ShowCardName(card.ItemData.title));
                _attempts++;
                UpdateUI();

                StartCoroutine(EvaluateTurnRoutine());
            }
        }

        private IEnumerator EvaluateTurnRoutine()
        {
            _isProcessingTurn = true;

            // Breve pausa para que el niño pueda leer ambas cartas
            yield return new WaitForSeconds(0.6f);

            bool isMatch = _firstCard.ItemData.pairId == _secondCard.ItemData.pairId;

            if (isMatch)
            {
                _firstCard.SetMatched();
                _secondCard.SetMatched();
                _pairsFound++;
                UpdateUI();

                if (_pairsFound >= _totalPairs)
                {
                    yield return new WaitForSeconds(1.0f);
                    EndGame(true);
                }
            }
            else
            {
                yield return new WaitForSeconds(1.0f);

                _firstCard.FlipDown();
                _secondCard.FlipDown();
            }

            _firstCard = null;
            _secondCard = null;
            _isProcessingTurn = false;
        }

        private void UpdateUI()
        {
            if (attemptsText != null)
            {
                attemptsText.text = $"Intentos: {_attempts}";
            }

            if (pairsFoundText != null)
            {
                pairsFoundText.text = $"Parejas: {_pairsFound} / {_totalPairs}";
            }
        }

        /// <summary>
        /// Genera dinámicamente un GameObject de carta con componentes uGUI si no se asignó prefab.
        /// </summary>
        public static GameObject CreateProceduralCard(Transform parent)
        {
            GameObject cardGo = new GameObject("MemoryCard", typeof(RectTransform), typeof(Image), typeof(Button), typeof(MemoryCardUI));
            cardGo.transform.SetParent(parent, false);

            var rect = cardGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 180f);

            var bgImg = cardGo.GetComponent<Image>();
            bgImg.color = new Color(0.18f, 0.22f, 0.30f, 1f);

            // Dorso / Cover
            GameObject cover = new GameObject("Cover", typeof(RectTransform), typeof(Image));
            cover.transform.SetParent(cardGo.transform, false);
            var coverRect = cover.GetComponent<RectTransform>();
            coverRect.anchorMin = Vector2.zero;
            coverRect.anchorMax = Vector2.one;
            coverRect.offsetMin = new Vector2(6, 6);
            coverRect.offsetMax = new Vector2(-6, -6);
            var coverImg = cover.GetComponent<Image>();
            coverImg.color = new Color(0.24f, 0.32f, 0.45f, 1f);
            coverImg.raycastTarget = false;

            GameObject coverIcon = new GameObject("CoverIcon", typeof(RectTransform), typeof(TextMeshProUGUI));
            coverIcon.transform.SetParent(cover.transform, false);
            var ciRect = coverIcon.GetComponent<RectTransform>();
            ciRect.anchorMin = Vector2.zero;
            ciRect.anchorMax = Vector2.one;
            ciRect.offsetMin = Vector2.zero;
            ciRect.offsetMax = Vector2.zero;
            var ciTmp = coverIcon.GetComponent<TextMeshProUGUI>();
            ciTmp.text = "❓";
            ciTmp.fontSize = 52;
            ciTmp.alignment = TextAlignmentOptions.Center;

            // Cara frontal / Face
            GameObject face = new GameObject("Face", typeof(RectTransform), typeof(Image));
            face.transform.SetParent(cardGo.transform, false);
            var faceRect = face.GetComponent<RectTransform>();
            faceRect.anchorMin = Vector2.zero;
            faceRect.anchorMax = Vector2.one;
            faceRect.offsetMin = new Vector2(6, 6);
            faceRect.offsetMax = new Vector2(-6, -6);
            var faceImg = face.GetComponent<Image>();
            faceImg.color = new Color(0.92f, 0.94f, 0.96f, 1f);
            faceImg.raycastTarget = false;
            face.SetActive(false);

            // Emoji / Icon
            GameObject emojiGo = new GameObject("EmojiText", typeof(RectTransform), typeof(TextMeshProUGUI));
            emojiGo.transform.SetParent(face.transform, false);
            var eRect = emojiGo.GetComponent<RectTransform>();
            eRect.anchorMin = new Vector2(0.5f, 0.65f);
            eRect.anchorMax = new Vector2(0.5f, 0.65f);
            eRect.pivot = new Vector2(0.5f, 0.5f);
            eRect.sizeDelta = new Vector2(100f, 60f);
            var eTmp = emojiGo.GetComponent<TextMeshProUGUI>();
            eTmp.text = "🦩";
            eTmp.fontSize = 44;
            eTmp.alignment = TextAlignmentOptions.Center;

            // Titulo
            GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(face.transform, false);
            var tRect = titleGo.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.20f);
            tRect.anchorMax = new Vector2(1f, 0.42f);
            tRect.offsetMin = new Vector2(8, 0);
            tRect.offsetMax = new Vector2(-8, 0);
            var tTmp = titleGo.GetComponent<TextMeshProUGUI>();
            tTmp.text = "Nombre";
            tTmp.fontSize = 19;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.color = new Color(0.10f, 0.12f, 0.16f);

            // Subtitulo
            GameObject subGo = new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGo.transform.SetParent(face.transform, false);
            var sRect = subGo.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0f, 0.02f);
            sRect.anchorMax = new Vector2(1f, 0.20f);
            sRect.offsetMin = new Vector2(6, 0);
            sRect.offsetMax = new Vector2(-6, 0);
            var sTmp = subGo.GetComponent<TextMeshProUGUI>();
            sTmp.text = "Categoría";
            sTmp.fontSize = 15;
            sTmp.fontStyle = FontStyles.Bold;
            sTmp.alignment = TextAlignmentOptions.Center;
            sTmp.color = new Color(0.22f, 0.30f, 0.38f);

            ciTmp.raycastTarget = false;
            tTmp.raycastTarget = false;
            sTmp.raycastTarget = false;
            eTmp.raycastTarget = false;

            // Wire fields via SerializedObject
            var ui = cardGo.GetComponent<MemoryCardUI>();
            var so = new UnityEditor.SerializedObject(ui);
            so.FindProperty("cardBackground").objectReferenceValue = bgImg;
            so.FindProperty("cardCover").objectReferenceValue = coverImg;
            so.FindProperty("faceContainer").objectReferenceValue = face;
            so.FindProperty("coverContainer").objectReferenceValue = cover;
            so.FindProperty("titleText").objectReferenceValue = tTmp;
            so.FindProperty("subtitleText").objectReferenceValue = sTmp;
            so.FindProperty("emojiText").objectReferenceValue = eTmp;
            so.FindProperty("clickButton").objectReferenceValue = cardGo.GetComponent<Button>();
            so.ApplyModifiedProperties();

            return cardGo;
        }

        private List<MemoryCardItem> GetFallbackCaribbeanItems()
        {
            return new List<MemoryCardItem>
            {
                new MemoryCardItem { pairId = "P1", title = "Flamenco Rosado", subtitle = "Ave representativa", emoji = "🦩", cardColor = new Color(1f, 0.9f, 0.92f) },
                new MemoryCardItem { pairId = "P1", title = "Santuario Los Flamencos", subtitle = "Lagunas de La Guajira", emoji = "🌊", cardColor = new Color(0.9f, 0.95f, 1f) },

                new MemoryCardItem { pairId = "P2", title = "Caimán Aguja", subtitle = "Reptil de gran tamaño", emoji = "🐊", cardColor = new Color(0.9f, 0.97f, 0.92f) },
                new MemoryCardItem { pairId = "P2", title = "Ciénaga Grande", subtitle = "Manglares del Magdalena", emoji = "🌿", cardColor = new Color(0.9f, 0.97f, 0.92f) },

                new MemoryCardItem { pairId = "P3", title = "Tití Cabeciblanco", subtitle = "Primate único de Colombia", emoji = "🐒", cardColor = new Color(1f, 0.96f, 0.9f) },
                new MemoryCardItem { pairId = "P3", title = "Bosque Seco Tropical", subtitle = "Montes de María y Bolívar", emoji = "🌳", cardColor = new Color(1f, 0.96f, 0.9f) },

                new MemoryCardItem { pairId = "P4", title = "Tortuga Carey", subtitle = "Especie marina protegida", emoji = "🐢", cardColor = new Color(0.88f, 0.98f, 0.96f) },
                new MemoryCardItem { pairId = "P4", title = "Arrecifes de Coral", subtitle = "Parque Nacional Tayrona", emoji = "🪸", cardColor = new Color(0.88f, 0.98f, 0.96f) },

                new MemoryCardItem { pairId = "P5", title = "Iguana Verde", subtitle = "Reptil arborícola", emoji = "🦎", cardColor = new Color(0.94f, 0.98f, 0.9f) },
                new MemoryCardItem { pairId = "P5", title = "Sabanas del Sinú", subtitle = "Tierras cálidas de Córdoba", emoji = "🌾", cardColor = new Color(0.94f, 0.98f, 0.9f) },

                new MemoryCardItem { pairId = "P6", title = "Guacamaya Verde", subtitle = "Ave de gran plumaje", emoji = "🦜", cardColor = new Color(0.98f, 0.94f, 0.88f) },
                new MemoryCardItem { pairId = "P6", title = "Sierra Nevada", subtitle = "Montaña costera más alta", emoji = "⛰️", cardColor = new Color(0.98f, 0.94f, 0.88f) }
            };
        }
    }
}
