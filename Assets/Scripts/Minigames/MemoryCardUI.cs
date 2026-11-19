using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Nehexplorer
{
    public class MemoryCardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public enum CardState
        {
            FaceDown,
            Flipping,
            FaceUp,
            Matched
        }

        [Header("Componentes Visuales")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image cardCover;
        [SerializeField] private GameObject faceContainer;
        [SerializeField] private GameObject coverContainer;

        [Header("Textos y Contenido")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI emojiText;
        [SerializeField] private Image spriteImage;
        [SerializeField] private Button clickButton;

        [Header("Animación y Efectos")]
        [SerializeField] private float flipDuration = 0.22f;

        private MemoryCardItem _itemData;
        private CardState _state = CardState.FaceDown;
        private Action<MemoryCardUI> _onClicked;
        private Coroutine _currentAnim;

        public MemoryCardItem ItemData => _itemData;
        public CardState State => _state;
        public bool IsInteractable => _state == CardState.FaceDown;

        private void Awake()
        {
            if (clickButton != null)
            {
                clickButton.onClick.AddListener(HandleClick);
            }
        }

        public void Initialize(MemoryCardItem item, Action<MemoryCardUI> onClickCallback)
        {
            _itemData = item;
            _onClicked = onClickCallback;
            _state = CardState.FaceDown;

            if (titleText != null)
            {
                titleText.text = item.title;
                titleText.raycastTarget = false;
            }
            if (subtitleText != null)
            {
                subtitleText.text = item.subtitle;
                subtitleText.raycastTarget = false;
            }

            if (item.customSprite != null && spriteImage != null)
            {
                spriteImage.sprite = item.customSprite;
                var aspectFitter = spriteImage.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                if (aspectFitter != null)
                {
                    aspectFitter.aspectRatio = item.customSprite.rect.width / item.customSprite.rect.height;
                }
                spriteImage.gameObject.SetActive(true);
                spriteImage.raycastTarget = false;
                if (emojiText != null) emojiText.gameObject.SetActive(false);
            }
            else
            {
                if (spriteImage != null) spriteImage.gameObject.SetActive(false);
                if (emojiText != null)
                {
                    emojiText.text = item.emoji;
                    emojiText.gameObject.SetActive(true);
                    emojiText.raycastTarget = false;
                }
            }

            if (cardBackground != null)
            {
                cardBackground.color = item.cardColor;
            }

            // Estado inicial: cara abajo
            if (coverContainer != null) coverContainer.SetActive(true);
            if (faceContainer != null) faceContainer.SetActive(false);

            transform.localScale = Vector3.one;
        }

        private void HandleClick()
        {
            if (_state != CardState.FaceDown) return;
            _onClicked?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            HandleClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_state != CardState.FaceDown) return;
            if (_currentAnim == null)
            {
                transform.localScale = new Vector3(1.04f, 1.04f, 1f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_currentAnim == null && _state != CardState.Flipping)
            {
                transform.localScale = Vector3.one;
            }
        }

        public void FlipUp(Action onComplete = null)
        {
            if (_state != CardState.FaceDown) return;
            if (_currentAnim != null) StopCoroutine(_currentAnim);
            _currentAnim = StartCoroutine(FlipRoutine(true, onComplete));
        }

        public void FlipDown(Action onComplete = null)
        {
            if (_state != CardState.FaceUp) return;
            if (_currentAnim != null) StopCoroutine(_currentAnim);
            _currentAnim = StartCoroutine(FlipRoutine(false, onComplete));
        }

        public void SetMatched()
        {
            _state = CardState.Matched;
            if (cardBackground != null)
            {
                cardBackground.color = new Color(0.72f, 0.96f, 0.78f, 1f);
            }
            if (_currentAnim != null) StopCoroutine(_currentAnim);
            _currentAnim = StartCoroutine(PulseRoutine());
        }

        private IEnumerator FlipRoutine(bool showFace, Action onComplete)
        {
            _state = CardState.Flipping;
            float elapsed = 0f;
            float halfDuration = flipDuration * 0.5f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / halfDuration);
                transform.localScale = new Vector3(Mathf.Lerp(1f, 0f, progress), 1f, 1f);
                yield return null;
            }

            if (coverContainer != null) coverContainer.SetActive(!showFace);
            if (faceContainer != null) faceContainer.SetActive(showFace);

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / halfDuration);
                transform.localScale = new Vector3(Mathf.Lerp(0f, 1f, progress), 1f, 1f);
                yield return null;
            }

            transform.localScale = Vector3.one;
            _state = showFace ? CardState.FaceUp : CardState.FaceDown;
            _currentAnim = null;
            onComplete?.Invoke();
        }

        private IEnumerator PulseRoutine()
        {
            Vector3 normalScale = Vector3.one;
            Vector3 popScale = Vector3.one * 1.12f;
            float t = 0f;
            while (t < 0.12f)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(normalScale, popScale, t / 0.12f);
                yield return null;
            }
            t = 0f;
            while (t < 0.12f)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(popScale, normalScale, t / 0.12f);
                yield return null;
            }
            transform.localScale = Vector3.one;
            _currentAnim = null;
        }
    }
}