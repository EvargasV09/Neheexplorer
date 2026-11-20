using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer
{
    // Pointer de Unity: admite ratón y pantalla táctil, sin HTML ni JavaScript.
    [RequireComponent(typeof(CanvasGroup))]
    public class EncocadoDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _icono;
        [SerializeField] private TextMeshProUGUI _nombre;

        private RectTransform _rect;
        private RectTransform _capaArrastre;
        private RectTransform _ingredientesBowl;
        private CanvasGroup _grupo;
        private Transform _origen;
        private bool _origenCapturado;
        private Vector2 _posicionInicial;
        private Vector2 _tamanoInicial;
        private Vector2 _desplazamiento;
        private Vector2 _anclaMinInicial;
        private Vector2 _anclaMaxInicial;
        private Vector2 _pivoteInicial;
        private Vector3 _escalaInicial;
        private Quaternion _rotacionInicial;
        private int _indiceInicial;
        private bool _arrastrando;
        private bool _completado;
        private Func<bool> _sePuedeArrastrar;
        private Func<EncocadoDragItem, int> _alSoltar;

        public EncocadoIngredient Ingrediente { get; private set; }

        public void Configurar(EncocadoIngredient dato, RectTransform capa,
            RectTransform ingredientesBowl, Func<bool> puedeArrastrar,
            Func<EncocadoDragItem, int> alSoltar)
        {
            _rect = (RectTransform)transform;
            _grupo = GetComponent<CanvasGroup>();
            Ingrediente = dato;
            _capaArrastre = capa;
            _ingredientesBowl = ingredientesBowl;
            _sePuedeArrastrar = puedeArrastrar;
            _alSoltar = alSoltar;
            if (!_origenCapturado)
            {
                _origen = transform.parent;
                _indiceInicial = transform.GetSiblingIndex();
                _posicionInicial = _rect.anchoredPosition;
                _tamanoInicial = _rect.sizeDelta;
                _anclaMinInicial = _rect.anchorMin;
                _anclaMaxInicial = _rect.anchorMax;
                _pivoteInicial = _rect.pivot;
                _escalaInicial = _rect.localScale;
                _rotacionInicial = _rect.localRotation;
                _origenCapturado = true;
            }
            _icono.sprite = dato.imagen;
            _icono.preserveAspect = true;
            _nombre.text = dato.nombre.ToUpperInvariant();
            Reiniciar();
        }

        public void Reiniciar()
        {
            _arrastrando = false;
            _completado = false;
            if (_origen != null)
            {
                if (transform.parent != _origen)
                    RestaurarTransform(); // Estaba dentro del bowl.
                else
                {
                    // Si nunca se movió, conservar el layout actual (móvil/PC).
                    _posicionInicial = _rect.anchoredPosition;
                    _tamanoInicial = _rect.sizeDelta;
                    _escalaInicial = _rect.localScale;
                    _rotacionInicial = _rect.localRotation;
                    _anclaMinInicial = _rect.anchorMin;
                    _anclaMaxInicial = _rect.anchorMax;
                    _pivoteInicial = _rect.pivot;
                }
            }
            if (_grupo != null)
            {
                _grupo.blocksRaycasts = true;
                _grupo.alpha = 1f;
            }
            gameObject.SetActive(true);
        }

        public void OnBeginDrag(PointerEventData evento)
        {
            if (evento.button != PointerEventData.InputButton.Left || _completado ||
                _sePuedeArrastrar == null || !_sePuedeArrastrar()) return;

            // Capturar su posición real (puede haber cambiado por pantalla vertical).
            _posicionInicial = _rect.anchoredPosition;
            _tamanoInicial = _rect.sizeDelta;
            _escalaInicial = _rect.localScale;
            _rotacionInicial = _rect.localRotation;
            _anclaMinInicial = _rect.anchorMin;
            _anclaMaxInicial = _rect.anchorMax;
            _pivoteInicial = _rect.pivot;
            transform.SetParent(_capaArrastre, true);
            transform.SetAsLastSibling();
            Vector2 punto;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _capaArrastre, evento.position, evento.pressEventCamera, out punto);
            _desplazamiento = _rect.anchoredPosition - punto;
            _grupo.blocksRaycasts = false; // Permite que el bowl reciba OnDrop.
            _grupo.alpha = 0.88f;
            _arrastrando = true;
        }

        public void OnDrag(PointerEventData evento)
        {
            if (!_arrastrando) return;
            Vector2 punto;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _capaArrastre, evento.position, evento.pressEventCamera, out punto))
            {
                _rect.anchoredPosition = punto + _desplazamiento;
            }
        }

        public void OnEndDrag(PointerEventData evento)
        {
            if (!_completado) VolverAlInicio();
        }

        // Devuelve true solamente cuando el ingrediente se acepta en el bowl.
        public bool IntentarSoltar()
        {
            if (!_arrastrando || _completado || _alSoltar == null) return false;
            int posicionEnBowl = _alSoltar(this);
            if (posicionEnBowl < 0) return false;

            _completado = true;
            _arrastrando = false;
            transform.SetParent(_ingredientesBowl, false);
            // Cuatro ingredientes distribuidos simétricamente dentro del bowl.
            _rect.localScale = Vector3.one;
            _rect.sizeDelta = new Vector2(140, 118);
            _rect.anchoredPosition = new Vector2((posicionEnBowl - 1.5f) * 155f, 0);
            _grupo.alpha = 1f;
            _grupo.blocksRaycasts = false;
            return true;
        }

        private void VolverAlInicio()
        {
            if (!_arrastrando) return;
            _arrastrando = false;
            RestaurarTransform();
            _grupo.alpha = 1f;
            _grupo.blocksRaycasts = true;
        }

        // Restaura también escala, giro, anclas y pivote; soluciona
        // desplazamientos/estiramientos al devolver distractores como la avena.
        private void RestaurarTransform()
        {
            if (_origen == null || _rect == null) return;
            transform.SetParent(_origen, false);
            transform.SetSiblingIndex(_indiceInicial);
            _rect.anchorMin = _anclaMinInicial;
            _rect.anchorMax = _anclaMaxInicial;
            _rect.pivot = _pivoteInicial;
            _rect.localScale = _escalaInicial;
            _rect.localRotation = _rotacionInicial;
            _rect.sizeDelta = _tamanoInicial;
            _rect.anchoredPosition = _posicionInicial;
        }

        private void OnApplicationFocus(bool tieneFoco)
        {
            if (!tieneFoco) VolverAlInicio();
        }
    }
}
