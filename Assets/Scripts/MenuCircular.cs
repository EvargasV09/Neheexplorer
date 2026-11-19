using UnityEngine;
using UnityEngine.UI;

public class RadialMenuLayout : MonoBehaviour
{
    public RectTransform[] items;

    void Start()
    {
        float angleStep = 360f / items.Length;

        for (int i = 0; i < items.Length; i++)
        {
            RectTransform item = items[i];

            item.anchoredPosition = Vector2.zero;
            item.localRotation = Quaternion.Euler(0, 0, -angleStep * i);

            Image img = item.GetComponentInChildren<Image>();
            if (img != null)
            {
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Radial360;
                img.fillOrigin = (int)Image.Origin360.Top;
                img.fillClockwise = true;
                img.fillAmount = 1f / items.Length;
            }
        }
    }
}