using UnityEngine;
using UnityEngine.EventSystems;

public class RegionHoverZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RegionZoomController zoomController;
    private float angleDegrees;

    void Start()
    {
        // Calcula el ángulo real según la posición del botón respecto al centro del padre
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 pos = rt.anchoredPosition;
        angleDegrees = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        zoomController.ZoomToAngle(angleDegrees);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        zoomController.ResetZoom();
    }
}