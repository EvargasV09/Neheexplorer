using UnityEngine;

namespace Nehexplorer
{
    public class MangroveDropZone : MonoBehaviour, UnityEngine.EventSystems.IDropHandler
    {
        [SerializeField] private string _targetId;
        [SerializeField] private int _stage;
        [SerializeField] private TMPro.TextMeshProUGUI _label;
        [SerializeField] private UnityEngine.UI.Image _image;
        private string _initialText;
        private Color _initialColor;
        private bool _cached;
        public string TargetId => _targetId;
        public int Stage => _stage;
        public void ResetZone()
        {
            if(!_cached) {_initialText=_label.text; _initialColor=_image.color; _cached=true;}
            _label.text=_initialText; _image.color=_initialColor;
        }
        public void MarkPlaced(string label)
        {
            _image.color=new Color(0.55f,0.83f,0.62f);
            if(_targetId!="Waste") _label.text="[OK]\n"+label;
        }
        public void OnDrop(UnityEngine.EventSystems.PointerEventData e)
        {
            if(e.pointerDrag==null) return;
            var item=e.pointerDrag.GetComponent<MangroveDragItem>();
            if(item!=null) item.TryDrop(_targetId);
        }
    }
}
