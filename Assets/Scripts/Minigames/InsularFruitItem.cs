using UnityEngine;

namespace Nehexplorer
{
    public class InsularFruitItem : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image _image;
        [SerializeField] private TMPro.TextMeshProUGUI _label;
        public RectTransform Rect { get; private set; }
        public InsularFruitData Data { get; private set; }
        public bool Collected { get; private set; }

        private void Awake() { Rect = (RectTransform)transform; }

        public void Configure(InsularFruitData data)
        {
            Data = data;
            Collected = false;
            if (Rect == null) Rect = (RectTransform)transform;
            _image.sprite = data.sprite;
            _image.color = data.sprite == null ? data.color : Color.white;
            _image.preserveAspect = true;
            _label.text = data.displayName;
        }

        public void Collect()
        {
            Collected = true;
            gameObject.SetActive(false);
        }
    }
}
