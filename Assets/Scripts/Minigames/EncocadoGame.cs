using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer
{
    // Sigue el patrón MinigameBase existente. No cambia el minijuego de manglares.
    public class EncocadoGame : MinigameBase
    {
        [Header("Nivel Pacífica: Encocado")]
        [SerializeField] private EncocadoLevelDataSet _nivel;
        [SerializeField] private EncocadoDragItem[] _tarjetas;
        [SerializeField] private RectTransform _capaArrastre;
        [SerializeField] private RectTransform _ingredientesBowl;
        [SerializeField] private RectTransform _bowl;
        [SerializeField] private TextMeshProUGUI _titulo;
        [SerializeField] private TextMeshProUGUI _instrucciones;
        [SerializeField] private TextMeshProUGUI _progreso;
        [SerializeField] private TextMeshProUGUI _mensaje;
        [SerializeField] private Button _jugarDeNuevo;

        private readonly HashSet<string> _acertados = new HashSet<string>();
        private Vector2 _bowlAntesDeSacudir;
        private Coroutine _sacudida;

        protected override void Awake()
        {
            base.Awake();
            if (_jugarDeNuevo != null) _jugarDeNuevo.onClick.AddListener(StartGame);
        }

        private void OnDestroy()
        {
            if (_jugarDeNuevo != null) _jugarDeNuevo.onClick.RemoveListener(StartGame);
        }

        public override void StartGame()
        {
            base.StartGame();
            _acertados.Clear();
            if (_sacudida != null)
            {
                StopCoroutine(_sacudida);
                _sacudida = null;
                if (_bowl != null) _bowl.anchoredPosition = _bowlAntesDeSacudir;
            }

            if (_nivel == null || _tarjetas == null || _tarjetas.Length != 6 ||
                _capaArrastre == null || _ingredientesBowl == null ||
                _bowl == null || _jugarDeNuevo == null)
            {
                Debug.LogError("[Encocado] Faltan referencias del nivel.", this);
                isGameActive = false;
                return;
            }

            regionId = _nivel.regionId;  // Debe ser "Pacifica", como en GameManager.
            regionDisplayName = _nivel.region;
            factTitle = "¡Preparaste " + _nivel.plato + "!";
            educationalFact = _nivel.explicacion;
            _titulo.text = "LA BALLENA CHEF · " + _nivel.region.ToUpperInvariant();
            _instrucciones.text = "¡Prepara un " + _nivel.plato + "! Arrastra los ingredientes al bowl.";
            _mensaje.text = "¡Escoge los ingredientes de nuestra receta!";

            // Pacífica: 6 tarjetas, incluyendo coco, y 4 ingredientes correctos.
            if (_nivel.ingredientes == null || _nivel.ingredientes.Count != 6)
            {
                Debug.LogError("[Encocado] Se esperan 6 ingredientes. Ejecuta el menú ACTUALIZAR Encocado v2.", this);
                isGameActive = false;
                return;
            }
            int correctos = 0;
            var ids = new HashSet<string>();
            foreach (var ingrediente in _nivel.ingredientes)
            {
                if (ingrediente == null || string.IsNullOrEmpty(ingrediente.id) || !ids.Add(ingrediente.id))
                {
                    Debug.LogError("[Encocado] Identificadores repetidos o vacíos.", this);
                    isGameActive = false;
                    return;
                }
                if (ingrediente.correcto) correctos++;
            }
            if (correctos != 4)
            {
                Debug.LogError("[Encocado] Debe haber 4 correctos (pescado, tomate, camarón y coco) y 2 distractores.", this);
                isGameActive = false;
                return;
            }

            for (int i = 0; i < _tarjetas.Length; i++)
            {
                if (_tarjetas[i] == null) { isGameActive = false; return; }
                _tarjetas[i].Configurar(_nivel.ingredientes[i], _capaArrastre,
                    _ingredientesBowl, () => isGameActive, RevisarIngrediente);
            }
            ActualizarProgreso();
        }

        // -1 = incorrecto; 0, 1, 2 o 3 = posición dentro del bowl.
        private int RevisarIngrediente(EncocadoDragItem tarjeta)
        {
            if (!isGameActive || tarjeta == null) return -1;
            EncocadoIngredient ingrediente = tarjeta.Ingrediente;

            if (!ingrediente.correcto)
            {
                _mensaje.text = "¡Ups! " + ingrediente.nombre + " no va en esta receta. ¡Intenta otra vez!";
                if (_sacudida != null)
                {
                    StopCoroutine(_sacudida);
                    _bowl.anchoredPosition = _bowlAntesDeSacudir;
                }
                _sacudida = StartCoroutine(SacudirBowl());
                return -1;
            }

            if (!_acertados.Add(ingrediente.id)) return -1;
            int posicion = _acertados.Count - 1;
            _mensaje.text = "¡Muy bien! Agregaste " + ingrediente.nombre + ".";
            ActualizarProgreso();
            if (_acertados.Count == 4)
            {
                // El item se coloca en el bowl al terminar el evento de arrastre.
                StartCoroutine(MostrarVictoria());
            }
            return posicion;
        }

        private IEnumerator MostrarVictoria()
        {
            yield return null;
            EndGame(true); // Usa el panel final y el guardado de MinigameBase.
        }

        private void ActualizarProgreso()
        {
            _progreso.text = "INGREDIENTES: " + _acertados.Count + " / 4";
        }

        private IEnumerator SacudirBowl()
        {
            Vector2 origen = _bowl.anchoredPosition;
            _bowlAntesDeSacudir = origen;
            for (int i = 0; i < 6; i++)
            {
                float desplazamiento = (i % 2 == 0 ? 1 : -1) * 13f;
                _bowl.anchoredPosition = origen + Vector2.right * desplazamiento;
                yield return new WaitForSeconds(0.045f);
            }
            _bowl.anchoredPosition = origen;
            _sacudida = null;
        }
    }
}
