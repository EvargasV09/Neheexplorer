using UnityEngine;

namespace Nehexplorer
{
    // Reubica la UI para computador y celular (vertical u horizontal).
    public class EncocadoResponsiveLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private RectTransform _titulo;
        [SerializeField] private RectTransform _region;
        [SerializeField] private RectTransform _instrucciones;
        [SerializeField] private RectTransform[] _tarjetas;
        [SerializeField] private RectTransform _bowl;
        [SerializeField] private RectTransform _progreso;
        [SerializeField] private RectTransform _mensaje;
        [SerializeField] private RectTransform _ballena;
        [SerializeField] private RectTransform _volver;

        private Vector2 _ultimaDimension;

        private void Start() { Ajustar(); }
        private void LateUpdate()
        {
            if (_root != null && _root.rect.size != _ultimaDimension) Ajustar();
        }

        private static void Poner(RectTransform elemento, Vector2 posicion, Vector2 tamano)
        {
            if (elemento == null) return;
            elemento.anchoredPosition = posicion;
            elemento.sizeDelta = tamano;
        }

        private void Ajustar()
        {
            if (_root == null || _tarjetas == null || _tarjetas.Length != 6) return;
            Vector2 s = _root.rect.size;
            _ultimaDimension = s;
            float w = s.x, h = s.y;
            bool vertical = h > w;

            if (vertical)
            {
                // Dos filas de 3 tarjetas para teléfono vertical.
                float ancho = Mathf.Min(220f, (w - 60f) / 3f);
                Poner(_titulo, new Vector2(0, h / 2f - 125), new Vector2(w - 30, 76));
                Poner(_region, new Vector2(0, h / 2f - 199), new Vector2(w - 40, 52));
                Poner(_instrucciones, new Vector2(0, h / 2f - 282), new Vector2(w - 55, 75));
                for (int i = 0; i < 6; i++)
                {
                    if (_tarjetas[i] == null || _tarjetas[i].parent != _root) continue;
                    float x = (i % 3 - 1) * (ancho + 14);
                    float y = h / 2f - (i < 3 ? 441 : 625);
                    Poner(_tarjetas[i], new Vector2(x, y), new Vector2(ancho, 156));
                }
                Poner(_bowl, new Vector2(0, -62), new Vector2(Mathf.Min(850, w - 80), 315));
                Poner(_progreso, new Vector2(0, -h / 2f + 485), new Vector2(w - 60, 62));
                Poner(_mensaje, new Vector2(0, -h / 2f + 412), new Vector2(w - 60, 85));
                Poner(_ballena, new Vector2(0, -h / 2f + 200), new Vector2(320, 290));
                Poner(_volver, new Vector2(-w / 2f + 170, h / 2f - 45), new Vector2(270, 58));
            }
            else
            {
                // Seis tarjetas centradas, sin solapamiento en computador.
                float ancho = Mathf.Min(235f, (w - 170f) / 6f);
                Poner(_titulo, new Vector2(0, h / 2f - 78), new Vector2(Mathf.Min(960, w - 550), 72));
                Poner(_region, new Vector2(0, h / 2f - 140), new Vector2(1000, 50));
                Poner(_instrucciones, new Vector2(0, h / 2f - 205), new Vector2(Mathf.Min(1200, w - 100), 62));
                for (int i = 0; i < 6; i++)
                {
                    if (_tarjetas[i] == null || _tarjetas[i].parent != _root) continue;
                    Poner(_tarjetas[i], new Vector2((i - 2.5f) * (ancho + 18f), h / 2f - 325), new Vector2(ancho, 150));
                }
                Poner(_bowl, new Vector2(0, -110), new Vector2(Mathf.Min(900, w * 0.55f), 320));
                Poner(_progreso, new Vector2(0, -h / 2f + 183), new Vector2(850, 60));
                Poner(_mensaje, new Vector2(0, -h / 2f + 107), new Vector2(1120, 65));
                Poner(_ballena, new Vector2(w / 2f - 230, -h / 2f + 210), new Vector2(300, 270));
                Poner(_volver, new Vector2(-w / 2f + 220, h / 2f - 70), new Vector2(370, 72));
            }
        }
    }
}
