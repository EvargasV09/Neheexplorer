using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Nehexplorer
{
    [RequireComponent(typeof(Image))]
    public class RegionSliceUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Configuration")]
        public RegionType regionType;
        public Vector2 popDirection = Vector2.up;
        public float popDistance = 16f;
        public float animSpeed = 12f;

        [Header("Visual Feedback")]
        public Color normalColor = Color.white;
        public Color hoverColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        public Color selectedColor = new Color(1.25f, 1.25f, 1.2f, 1f);
        public Color lockedColor = new Color(0.45f, 0.45f, 0.5f, 0.85f);

        private Image _image;
        private RectTransform _rectTransform;
        private Vector2 _originalAnchoredPosition;
        private Vector2 _targetAnchoredPosition;
        private Vector3 _originalScale = Vector3.one;
        private Vector3 _targetScale = Vector3.one;
        private Color _targetColor = Color.white;

        private bool _isHovered;
        private bool _isSelected;
        private bool _isLocked;

        public event Action<RegionSliceUI> OnHoverEntered;
        public event Action<RegionSliceUI> OnHoverExited;
        public event Action<RegionSliceUI> OnSelected;

        public Image TargetImage => _image;
        public bool IsLocked => _isLocked;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _originalAnchoredPosition = _rectTransform.anchoredPosition;
            _targetAnchoredPosition = _originalAnchoredPosition;
            _targetColor = normalColor;

            // Enable pixel-perfect raycasting on the sprite
            _image.alphaHitTestMinimumThreshold = 0.1f;
        }

        public void Initialize(RegionType type, Vector2 offset, Vector2 popDir, bool locked)
        {
            regionType = type;
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.anchoredPosition = offset;
            _originalAnchoredPosition = offset;
            _targetAnchoredPosition = offset;
            popDirection = popDir.normalized;
            _isLocked = locked;
            UpdateVisualTarget();
        }

        public void SetLocked(bool locked)
        {
            _isLocked = locked;
            UpdateVisualTarget();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisualTarget();
        }

        private void UpdateVisualTarget()
        {
            if (_isLocked)
            {
                _targetColor = lockedColor;
                _targetScale = Vector3.one;
                _targetAnchoredPosition = _originalAnchoredPosition;
                return;
            }

            if (_isSelected)
            {
                _targetColor = selectedColor;
                _targetScale = Vector3.one * 1.025f;
                _targetAnchoredPosition = _originalAnchoredPosition + popDirection * popDistance;
            }
            else if (_isHovered)
            {
                _targetColor = hoverColor;
                _targetScale = Vector3.one * 1.02f;
                _targetAnchoredPosition = _originalAnchoredPosition + popDirection * (popDistance * 0.75f);
            }
            else
            {
                _targetColor = normalColor;
                _targetScale = Vector3.one;
                _targetAnchoredPosition = _originalAnchoredPosition;
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime * animSpeed;
            _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, _targetAnchoredPosition, dt);
            _rectTransform.localScale = Vector3.Lerp(_rectTransform.localScale, _targetScale, dt);
            _image.color = Color.Lerp(_image.color, _targetColor, dt);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateVisualTarget();
            OnHoverEntered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateVisualTarget();
            OnHoverExited?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSelected?.Invoke(this);
        }
    }
}
