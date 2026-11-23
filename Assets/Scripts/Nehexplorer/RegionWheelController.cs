using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nehexplorer
{
    public class RegionWheelController : MonoBehaviour
    {
        [Header("Wheel Configuration")]
        [SerializeField] private RectTransform wheelContainer;
        [SerializeField] private List<RegionSliceUI> slices = new List<RegionSliceUI>();

        [Header("Information Panel UI")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI ecosystemText;
        [SerializeField] private TextMeshProUGUI climateText;
        [SerializeField] private TextMeshProUGUI guardianText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI minigamesText;
        [SerializeField] private Button exploreButton;
        [SerializeField] private TextMeshProUGUI exploreButtonText;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private TextMeshProUGUI lockedReasonText;

        [Header("Status & Progress")]
        [SerializeField] private TextMeshProUGUI fragmentsCountText;
        [SerializeField] private int completedRegionsCount = 0;

        [Header("Data")]
        [SerializeField] private List<RegionInfo> regionDatabase = new List<RegionInfo>();

        private RegionType _currentSelectedRegion = RegionType.Caribe;
        private RegionType _currentHoveredRegion = RegionType.Caribe;

        public event Action<RegionType> OnRegionSelected;
        public event Action<RegionType> OnExploreRequested;

        private void Start()
        {
            if (regionDatabase == null || regionDatabase.Count == 0)
            {
                InitializeDefaultDatabase();
            }

            SyncCompletedRegionsFromSave();
            SetupSlices();
            UpdateFragmentsProgressUI();
            SelectRegion(RegionType.Caribe);
        }

        public void SyncCompletedRegionsFromSave()
        {
            string[] regions = new string[] { "Caribe", "Andina", "Orinoquia", "Amazonica", "Pacifica", "Insular" };
            int count = 0;
            foreach (var r in regions)
            {
                bool isDone = false;
                if (GameManager.Instance != null)
                {
                    isDone = GameManager.Instance.IsCompleted(r);
                }
                else
                {
                    isDone = PlayerPrefs.GetInt(GameManager.PREF_KEY_REGION_PREFIX + r, 0) == 1;
                }
                if (isDone) count++;
            }
            completedRegionsCount = count;
        }

        public void SetupSlices()
        {
            foreach (var slice in slices)
            {
                if (slice == null) continue;
                slice.OnHoverEntered -= HandleSliceHover;
                slice.OnHoverExited -= HandleSliceExit;
                slice.OnSelected -= HandleSliceSelected;

                slice.OnHoverEntered += HandleSliceHover;
                slice.OnHoverExited += HandleSliceExit;
                slice.OnSelected += HandleSliceSelected;

                // Check lock condition (Espiritual locked if not all 6 completed)
                if (slice.regionType == RegionType.Espiritual)
                {
                    slice.SetLocked(completedRegionsCount < 6);
                }
            }
        }

        private void HandleSliceHover(RegionSliceUI slice)
        {
            _currentHoveredRegion = slice.regionType;
            DisplayRegionData(slice.regionType);
        }

        private void HandleSliceExit(RegionSliceUI slice)
        {
            // Return to currently selected region display
            DisplayRegionData(_currentSelectedRegion);
        }

        private void HandleSliceSelected(RegionSliceUI slice)
        {
            SelectRegion(slice.regionType);
        }

        public void SelectRegion(RegionType type)
        {
            _currentSelectedRegion = type;

            foreach (var slice in slices)
            {
                if (slice == null) continue;
                slice.SetSelected(slice.regionType == type);
            }

            DisplayRegionData(type);
            OnRegionSelected?.Invoke(type);
        }

        public void DisplayRegionData(RegionType type)
        {
            var data = GetRegionData(type);
            if (data == null) return;

            if (titleText != null)
            {
                titleText.text = data.displayName.ToUpper();
                titleText.color = data.themeColor;
            }

            if (subtitleText != null)
            {
                subtitleText.text = data.subtitle;
            }

            if (ecosystemText != null)
            {
                ecosystemText.text = $"<b>Ecosistema:</b> {data.ecosystem}";
            }

            if (climateText != null)
            {
                climateText.text = $"<b>Clima:</b> {data.climate}";
            }

            if (guardianText != null)
            {
                guardianText.text = $"<b>Guardián:</b> {data.guardianName}";
            }

            if (descriptionText != null)
            {
                descriptionText.text = data.description;
            }

            if (minigamesText != null)
            {
                if (data.minigames != null && data.minigames.Length > 0)
                {
                    minigamesText.text = "<b>Minijuegos y Retos:</b>\n• " + string.Join("\n• ", data.minigames);
                }
                else
                {
                    minigamesText.text = "";
                }
            }

            bool isLocked = (type == RegionType.Espiritual && completedRegionsCount < 6);

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(isLocked);
            }

            if (lockedReasonText != null)
            {
                lockedReasonText.text = isLocked
                    ? $"Completa los 6 minijuegos de Caribe, Andina, Orinoquía, Amazonía, Pacífica e Insular para desbloquear el séptimo reto ({completedRegionsCount}/6)."
                    : "";
            }

            if (exploreButton != null)
            {
                exploreButton.interactable = !isLocked;
            }

            if (exploreButtonText != null)
            {
                if (isLocked)
                    exploreButtonText.text = "BLOQUEADO";
                else if (type == RegionType.Espiritual)
                    exploreButtonText.text = "¡INICIAR RITUAL FINAL!";
                else
                    exploreButtonText.text = "¡EXPLORAR REGIÓN!";
            }
        }

        public RegionInfo GetRegionData(RegionType type)
        {
            return regionDatabase.Find(r => r.type == type);
        }

        public void SetCompletedRegions(int count)
        {
            // La cifra enviada por la UI no puede sustituir las victorias guardadas.
            SyncCompletedRegionsFromSave();
            UpdateFragmentsProgressUI();

            var spiritualSlice = slices.Find(s => s != null && s.regionType == RegionType.Espiritual);
            if (spiritualSlice != null)
            {
                spiritualSlice.SetLocked(completedRegionsCount < 6);
            }

            DisplayRegionData(_currentSelectedRegion);
        }

        private void UpdateFragmentsProgressUI()
        {
            if (fragmentsCountText != null)
            {
                fragmentsCountText.text = $"Fragmentos de Sabiduría: {completedRegionsCount} / 6";
            }
        }

        public void OnClickExplore()
        {
            SetCompletedRegions(completedRegionsCount);
            if (_currentSelectedRegion == RegionType.Espiritual && completedRegionsCount < 6)
            {
                return;
            }

            Debug.Log($"[Nehexplorer] Explorando {_currentSelectedRegion}...");
            OnExploreRequested?.Invoke(_currentSelectedRegion);

            if (_currentSelectedRegion == RegionType.Caribe)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Minigame_Caribe");
            }
            else if (_currentSelectedRegion == RegionType.Andina)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Minigame_Andina");
            }
            else if (_currentSelectedRegion == RegionType.Orinoquia)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Minigame_Orinoquia");
            }
            else if (_currentSelectedRegion == RegionType.Amazonica)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Minigame_Amazonica");
            }
            else if (_currentSelectedRegion == RegionType.Insular)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Minigame_Insular");
            }
            else if (_currentSelectedRegion == RegionType.Pacifica)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Minigame_Pacifica_Encocado");
            }
            else if (_currentSelectedRegion == RegionType.Espiritual)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Minigame_Espiritual");
            }
        }

        public void InitializeDefaultDatabase()
        {
            regionDatabase = new List<RegionInfo>
            {
                new RegionInfo
                {
                    type = RegionType.Caribe,
                    displayName = "Región Caribe",
                    subtitle = "Tierra de brisa marina, sabanas y murallas históricas",
                    ecosystem = "Costas, manglares, arrecifes y desierto de La Guajira",
                    climate = "Cálido tropical, seco y semiárido en el norte",
                    guardianName = "Guardián de la Brisa Dorada",
                    description = "Abarca las cálidas llanuras costeras del norte, desde las históricas fortalezas de Cartagena hasta los paisajes áridos de La Guajira.",
                    minigames = new string[] {
                        "Gestión de agua en La Guajira (Conservación hídrica)",
                        "Memoria: Flora y fauna costera representativa"
                    },
                    themeColor = new Color(0.98f, 0.76f, 0.28f)
                },
                new RegionInfo
                {
                    type = RegionType.Andina,
                    displayName = "Región Andina",
                    subtitle = "El corazón de las tres cordilleras y los páramos",
                    ecosystem = "Bosques de niebla, páramos, valles y nieves perpetuas",
                    climate = "Variedad de pisos térmicos (de cálido a gélido)",
                    guardianName = "Guardián de los Nevados",
                    description = "Es la zona más montañosa y poblada del país. Cruza las cordilleras Occidental, Central y Oriental albergando los páramos más ricos en agua del mundo.",
                    minigames = new string[] {
                        "Escalada y ascenso respondiendo sobre pisos térmicos",
                        "Simulación de cultivo sostenible en ladera cafetera"
                    },
                    themeColor = new Color(0.42f, 0.78f, 0.98f)
                },
                new RegionInfo
                {
                    type = RegionType.Orinoquia,
                    displayName = "Región Orinoquía",
                    subtitle = "Los infinitos Llanos Orientales y atardeceres de fuego",
                    ecosystem = "Sabana tropical inundable, esteros y morichales",
                    climate = "Tropical de sabana (estación seca y lluviosa)",
                    guardianName = "Guardián del Sol Llanero",
                    description = "Inmensas llanuras bañadas por afluentes del río Orinoco, donde conviven chigüiros, garzas y una arraigada cultura ganadera y musical.",
                    minigames = new string[] {
                        "Carrera llanera esquivando obstáculos y conociendo fauna",
                        "Puzzle del ciclo de inundación y sequía"
                    },
                    themeColor = new Color(0.96f, 0.58f, 0.22f)
                },
                new RegionInfo
                {
                    type = RegionType.Amazonica,
                    displayName = "Región Amazónica",
                    subtitle = "El pulmón verde del mundo y hogar ancestral",
                    ecosystem = "Selva húmeda tropical densa, ríos caudalosos e igapós",
                    climate = "Cálido y muy húmedo (ecuatorial)",
                    guardianName = "Guardián del Yagé y los Grandes Ríos",
                    description = "La mayor reserva biológica del planeta, poblada por delfines rosados, guacamayas, jaguares y pueblos indígenas guardianes de la selva.",
                    minigames = new string[] {
                        "Navegación en canoa registrando especies y frenando deforestación",
                        "Quiz visual de biodiversidad amazónica"
                    },
                    themeColor = new Color(0.35f, 0.85f, 0.45f)
                },
                new RegionInfo
                {
                    type = RegionType.Espiritual,
                    displayName = "Región Espiritual",
                    subtitle = "El santuario celestial de los guardianes",
                    ecosystem = "Reino ancestral místico flotante",
                    climate = "Energía armónica pura",
                    guardianName = "Gran Espíritu Protector de Colombia",
                    description = "Un espacio sagrado que une el conocimiento de todas las regiones. Se revela a los exploradores que han demostrado respeto y comprensión por la naturaleza colombiana.",
                    minigames = new string[] {
                        "Desafío del Altar: Integración de los 6 fragmentos de sabiduría",
                        "Enigma de la Biodiversidad Nacional"
                    },
                    themeColor = new Color(0.45f, 0.95f, 0.85f)
                },
                new RegionInfo
                {
                    type = RegionType.Pacifica,
                    displayName = "Región Pacífica",
                    subtitle = "Cuna de biodiversidad, selva pluvial y ballenas jorobadas",
                    ecosystem = "Selvas muy húmedas, estuarios, manglares y litoral",
                    climate = "Extremadamente lluvioso y cálido húmedo",
                    guardianName = "Guardián de la Lluvia y las Mareas",
                    description = "Una de las zonas más lluviosas del planeta. Sus aguas son santuario anual de ballenas y sus ríos arterias de vida y riqueza cultural afrodescendiente e indígena.",
                    minigames = new string[] {
                         "La ballena chef: prepara un Encocado con ingredientes del Pac\xEDfico"
                    },
                    themeColor = new Color(0.28f, 0.72f, 0.62f)
                },
                new RegionInfo
                {
                    type = RegionType.Insular,
                    displayName = "Región Insular",
                    subtitle = "Paraísos oceánicos del Caribe y del Pacífico",
                    ecosystem = "Arrecifes de coral, playas, cayos y ecosistemas pelágicos",
                    climate = "Tropical marítimo cálido",
                    guardianName = "Guardián de los Arrecifes",
                    description = "Compuesta por San Andrés, Providencia y Santa Catalina en el Caribe, y Malpelo y Gorgona en el Pacífico. Es cuna de la Reserva de Biósfera Seaflower.",
                    minigames = new string[] {
                        "Frutas de las Islas: Recoge frutas siguiendo el mouse",
                        "Descubre datos sobre la diversidad de frutas tropicales"
                    },
                    themeColor = new Color(0.32f, 0.82f, 0.92f)
                }
            };
        }
    }
}
