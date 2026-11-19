using System;
using UnityEngine;

namespace Nehexplorer
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MangroveDragItem : MonoBehaviour, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler
    {
        [SerializeField] private UnityEngine.UI.Image _image;
        [SerializeField] private TMPro.TextMeshProUGUI _label;
        private RectTransform _rect;
        private CanvasGroup _group;
        private RectTransform _dragLayer;
        private Transform _home;
        private Vector2 _homePosition;
        private int _homeIndex;
        private Vector2 _offset;
        private bool _dragging;
        private Func<bool> _canDrag;
        private Func<MangroveDragItem,string,bool> _drop;
        public MangroveTaskData Data { get; private set; }
        public bool Completed { get; private set; }

        private void Awake() { Cache(); }
        private void Cache() { _rect=(RectTransform)transform; _group=GetComponent<CanvasGroup>(); }
        public void Bind(MangroveTaskData data, RectTransform dragLayer, Func<bool> canDrag, Func<MangroveDragItem,string,bool> drop)
        {
            Cache(); Data=data; _dragLayer=dragLayer; _canDrag=canDrag; _drop=drop; Completed=false;
            _image.sprite=data.sprite; _image.color=data.sprite==null?data.color:Color.white; _image.preserveAspect=true; _label.text=data.label;
        }
        public void ResetItem() { CancelDrag(); Completed=false; }
        public void OnBeginDrag(UnityEngine.EventSystems.PointerEventData e)
        {
            if(e.button!=UnityEngine.EventSystems.PointerEventData.InputButton.Left||Completed||_canDrag==null||!_canDrag()) return;
            _home=transform.parent; _homePosition=_rect.anchoredPosition; _homeIndex=transform.GetSiblingIndex();
            transform.SetParent(_dragLayer,true); transform.SetAsLastSibling();
            Vector2 point; RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragLayer,e.position,e.pressEventCamera,out point);
            _offset=_rect.anchoredPosition-point; _group.blocksRaycasts=false; _group.alpha=0.85f; _dragging=true;
        }
        public void OnDrag(UnityEngine.EventSystems.PointerEventData e)
        {
            if(!_dragging) return;
            if(!_canDrag()) {CancelDrag(); return;}
            Vector2 point; if(RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragLayer,e.position,e.pressEventCamera,out point)) {
                var bounds=_dragLayer.rect; var margin=_rect.rect.size*0.5f;
                _rect.anchoredPosition=new Vector2(Mathf.Clamp(point.x+_offset.x,bounds.xMin+margin.x,bounds.xMax-margin.x),Mathf.Clamp(point.y+_offset.y,bounds.yMin+margin.y,bounds.yMax-margin.y));
            }
        }
        public void OnEndDrag(UnityEngine.EventSystems.PointerEventData e) { CancelDrag(); }
        public bool TryDrop(string targetId)
        {
            if(Completed||_drop==null||_canDrag==null||!_canDrag()) return false;
            if(!_drop(this,targetId)) return false;
            Completed=true; CancelDrag(); gameObject.SetActive(false); return true;
        }
        private void CancelDrag()
        {
            if(!_dragging) return;
            _dragging=false; transform.SetParent(_home,false); transform.SetSiblingIndex(_homeIndex); _rect.anchoredPosition=_homePosition; _group.blocksRaycasts=true; _group.alpha=1f;
        }
        private void OnDisable() {CancelDrag();}
        private void OnApplicationFocus(bool focus) {if(!focus) CancelDrag();}
    }
}
