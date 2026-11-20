using UnityEngine;
using UnityEngine.EventSystems;

namespace Nehexplorer
{
    // Bowl: detecta que una tarjeta fue soltada sobre él.
    public class EncocadoDropZone : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData evento)
        {
            if (evento.pointerDrag == null) return;
            EncocadoDragItem tarjeta = evento.pointerDrag.GetComponent<EncocadoDragItem>();
            if (tarjeta != null) tarjeta.IntentarSoltar();
        }
    }
}
