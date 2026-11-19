using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nehexplorer
{
    /// <summary>
    /// Administrador global del estado del juego: personaje seleccionado, regiones completadas y persistencia con PlayerPrefs.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public const string PREF_KEY_CHARACTER = "Nehexplorer_SelectedCharacter";
        public const string PREF_KEY_REGION_PREFIX = "Nehexplorer_Region_";

        [Header("Estado del Jugador")]
        [SerializeField] private string selectedCharacter = "Niña";

        [Header("Configuración")]
        [SerializeField] private string hubSceneName = "MapaHub";

        public event Action<string> OnRegionCompleted;
        public event Action<string> OnCharacterChanged;

        public string SelectedCharacter => selectedCharacter;
        public string HubSceneName => hubSceneName;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadState();
        }

        private void LoadState()
        {
            selectedCharacter = PlayerPrefs.GetString(PREF_KEY_CHARACTER, "Niña");
        }

        public void SetSelectedCharacter(string characterName)
        {
            selectedCharacter = characterName;
            PlayerPrefs.SetString(PREF_KEY_CHARACTER, selectedCharacter);
            PlayerPrefs.Save();
            OnCharacterChanged?.Invoke(selectedCharacter);
        }

        public void CompleteRegion(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return;

            string key = PREF_KEY_REGION_PREFIX + regionId;
            if (PlayerPrefs.GetInt(key, 0) == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
                Debug.Log($"[GameManager] ¡Región '{regionId}' completada!");
                OnRegionCompleted?.Invoke(regionId);
            }
        }

        public bool IsCompleted(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return false;
            return PlayerPrefs.GetInt(PREF_KEY_REGION_PREFIX + regionId, 0) == 1;
        }

        public int GetCompletedRegionsCount(string[] allRegionIds)
        {
            if (allRegionIds == null) return 0;
            int count = 0;
            foreach (var id in allRegionIds)
            {
                if (IsCompleted(id)) count++;
            }
            return count;
        }

        public void ResetProgress()
        {
            string[] regions = new string[] { "Caribe", "Andina", "Orinoquia", "Amazonica", "Pacifica", "Insular", "Espiritual" };
            foreach (var r in regions)
            {
                PlayerPrefs.DeleteKey(PREF_KEY_REGION_PREFIX + r);
            }
            PlayerPrefs.DeleteKey(PREF_KEY_CHARACTER);
            PlayerPrefs.Save();
            selectedCharacter = "Niña";
            Debug.Log("[GameManager] Progreso reiniciado.");
        }

        public void LoadHubScene()
        {
            SceneManager.LoadScene(hubSceneName);
        }
    }
}
