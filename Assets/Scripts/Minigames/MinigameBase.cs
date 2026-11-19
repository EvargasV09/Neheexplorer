using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Nehexplorer
{
    /// <summary>
    /// Clase base abstracta para todos los minijuegos de Nehexplorer.
    /// Maneja el ciclo de vida, el panel educativo final "¿Sabías que...?" y el regreso al MapaHub.
    /// </summary>
    public abstract class MinigameBase : MonoBehaviour
    {
        [Header("Datos de la Región")]
        [Tooltip("Identificador exacto de la región para GameManager (ej: Caribe, Andina, Amazonica)")]
        [SerializeField] protected string regionId = "Caribe";
        [SerializeField] protected string regionDisplayName = "Región Caribe";

        [Header("Contenido Educativo Final (Editable por Docentes)")]
        [SerializeField] protected string factTitle = "¿Sabías que...?";
        [TextArea(3, 6)]
        [SerializeField] protected string educationalFact = "En La Guajira colombiana se encuentra el Santuario de Fauna y Flora Los Flamencos, donde miles de flamencos rosados se alimentan de pequeños crustáceos en lagunas costeras protegidas.";

        [Header("Navegación al Mapa")]
        [SerializeField] protected UnityEngine.UI.Button backToHubTopButton;

        [Header("Referencias UI del Panel Final")]
        [SerializeField] protected GameObject factPanel;
        [SerializeField] protected TextMeshProUGUI resultTitleText;
        [SerializeField] protected TextMeshProUGUI factTitleText;
        [SerializeField] protected TextMeshProUGUI factBodyText;
        [SerializeField] protected Button returnToHubButton;
        [SerializeField] protected TextMeshProUGUI returnButtonText;

        protected bool isGameActive = false;
        protected bool hasWon = false;

        protected virtual void Awake()
        {
            if (factPanel != null)
            {
                factPanel.SetActive(false);
            }

            if (backToHubTopButton != null)
            {
                backToHubTopButton.onClick.AddListener(ReturnToHub);
            }

            if (returnToHubButton != null)
            {
                returnToHubButton.onClick.AddListener(OnReturnToHubClicked);
            }

            if (backToHubTopButton != null)
            {
                backToHubTopButton.onClick.AddListener(ReturnToHubImmediately);
        }
        }

        /// <summary>
        /// Sale inmediatamente hacia el mapa principal (MapaHub), sin importar el estado de la partida.
        /// </summary>
        public virtual void ReturnToHubImmediately()
        {
            if (hasWon)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CompleteRegion(regionId);
                }
                else
                {
                    PlayerPrefs.SetInt(GameManager.PREF_KEY_REGION_PREFIX + regionId, 1);
                    PlayerPrefs.Save();
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadHubScene();
            }
            else
            {
                SceneManager.LoadScene("MapaHub");
            }
        }

        protected virtual void Start()
        {
            StartGame();
        }

        /// <summary>
        /// Inicia o reinicia la lógica del minijuego.
        /// </summary>
        public virtual void StartGame()
        {
            isGameActive = true;
            hasWon = false;
            if (factPanel != null)
            {
                factPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Finaliza la partida. Muestra el panel informativo y desbloquea la región si se ganó.
        /// </summary>
        public virtual void EndGame(bool won)
        {
            isGameActive = false;
            hasWon = won;

            if (won)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CompleteRegion(regionId);
                }
                else
                {
                    PlayerPrefs.SetInt(GameManager.PREF_KEY_REGION_PREFIX + regionId, 1);
                    PlayerPrefs.Save();
                }
            }

            if (factPanel != null)
            {
                factPanel.SetActive(true);

                if (resultTitleText != null)
                {
                    resultTitleText.text = won
                        ? "¡FELICITACIONES, EXPLORADOR!"
                        : "¡BUEN INTENTO!";
                    resultTitleText.color = won
                        ? new Color(0.98f, 0.82f, 0.35f)
                        : new Color(0.9f, 0.6f, 0.4f);
                }

                if (factTitleText != null)
                {
                    factTitleText.text = factTitle;
                }

                if (factBodyText != null)
                {
                    factBodyText.text = educationalFact;
                }

                if (returnButtonText != null)
                {
                    returnButtonText.text = won ? "RECLAMAR SABIDURÍA Y VOLVER" : "VOLVER AL MAPA";
                }
            }
        }

        /// <summary>
        /// Regresa al mapa incluso durante una partida o después de perder.
        /// El botón superior no debe invocar callbacks de reintento sobrescritos.
        /// </summary>
        public void ReturnToHub()
        {
            isGameActive = false;
            StopAllCoroutines();

            if (hasWon)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CompleteRegion(regionId);
                }
                else
                {
                    // Fallback si se probó la escena directamente sin pasar por GameManager
                    PlayerPrefs.SetInt(GameManager.PREF_KEY_REGION_PREFIX + regionId, 1);
                    PlayerPrefs.Save();
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadHubScene();
            }
            else
            {
                SceneManager.LoadScene("MapaHub");
            }
        }

        /// <summary>
        /// Callback del botón para guardar progreso y regresar al mapa general.
        /// </summary>
        protected virtual void OnReturnToHubClicked()
        {
            ReturnToHub();
        }
    }
}
