using UnityEngine;

public class RegionZoomController : MonoBehaviour
{
    public RectTransform wheelImage; // arrastra aquí el mismo objeto (RegionsWheel)
    public float zoomScale = 1.15f;
    public float panDistance = 40f;
    public float speed = 8f;

    private Vector3 targetScale = Vector3.one;
    private Vector2 targetPos = Vector2.zero;

    void Update()
    {
        wheelImage.localScale = Vector3.Lerp(wheelImage.localScale, targetScale, Time.deltaTime * speed);
        wheelImage.anchoredPosition = Vector2.Lerp(wheelImage.anchoredPosition, targetPos, Time.deltaTime * speed);
    }

    public void ZoomToAngle(float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        targetScale = Vector3.one * zoomScale;
        targetPos = -dir * panDistance; // desplaza la zona hacia el centro
    }

    public void ResetZoom()
    {
        targetScale = Vector3.one;
        targetPos = Vector2.zero;
    }
}