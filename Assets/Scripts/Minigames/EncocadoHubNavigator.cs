using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nehexplorer
{
    // Conecta SOLO Pacífica con Encocado; los demás botones mantienen su lógica original.
    // Lo agrega el menú de actualización al botón Explorar del MapaHub.
    public class EncocadoHubNavigator : MonoBehaviour
    {
        [SerializeField] private RegionWheelController _rueda;
        private RegionType _seleccionada = RegionType.Caribe;

        private void OnEnable()
        {
            if (_rueda == null) _rueda = FindFirstObjectByType<RegionWheelController>();
            if (_rueda != null) _rueda.OnRegionSelected += CambiarSeleccion;
        }

        private void OnDisable()
        {
            if (_rueda != null) _rueda.OnRegionSelected -= CambiarSeleccion;
        }

        private void CambiarSeleccion(RegionType region)
        {
            _seleccionada = region;
            if (region == RegionType.Pacifica && _rueda != null)
            {
                // También funciona si el mapa genera su base de datos al iniciar.
                RegionInfo datos = _rueda.GetRegionData(RegionType.Pacifica);
                if (datos != null)
                {
                    datos.minigames = new[] {
                        "La ballena chef: prepara un Encocado con ingredientes del Pacífico"
                    };
                    _rueda.DisplayRegionData(RegionType.Pacifica);
                }
            }
        }

        public void Explorar()
        {
            if (_rueda == null)
            {
                Debug.LogError("[Encocado] No se encontró RegionWheelController en MapaHub.", this);
                return;
            }
            if (_seleccionada == RegionType.Pacifica)
            {
                SceneManager.LoadScene("Minigame_Pacifica_Encocado");
            }
            else
            {
                // Conserva la navegación anterior de las otras regiones.
                _rueda.OnClickExplore();
            }
        }
    }
}
