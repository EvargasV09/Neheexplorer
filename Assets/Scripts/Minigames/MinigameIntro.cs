using System.Collections;
using UnityEngine;

namespace Nehexplorer
{
    [DefaultExecutionOrder(-1000)]
    public class MinigameIntro : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private GameObject introPanel;
        [SerializeField] private MinigameBase minigame;
        [SerializeField] private UnityEngine.EventSystems.EventSystem eventSystem;

        private UnityEngine.InputSystem.InputAction _startAction;
        private float _previousTimeScale;
        private bool _waiting;
        private bool _starting;

        private void Awake()
        {
            if (introPanel == null || minigame == null) return;
            _previousTimeScale = Time.timeScale;
            _waiting = true;
            introPanel.SetActive(true);
            Time.timeScale = 0f;
            if (eventSystem != null) eventSystem.SetSelectedGameObject(null);
            _startAction = new UnityEngine.InputSystem.InputAction("StartMinigame",
                UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/anyKey");
        }

        private void OnEnable()
        {
            if (_startAction == null || !_waiting) return;
            _startAction.performed += OnStartPressed;
            _startAction.Enable();
        }

        private void OnStartPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!_waiting || _starting) return;
            _starting = true;
            _startAction.Disable();
            StartCoroutine(StartAfterInputFrame());
        }

        private IEnumerator StartAfterInputFrame()
        {
            // Consume la tecla inicial antes de habilitar los controles del juego.
            yield return null;
            introPanel.SetActive(false);
            RestoreTime();
            if (eventSystem != null) eventSystem.SetSelectedGameObject(null);
            minigame.StartGame();
        }

        private void RestoreTime()
        {
            if (!_waiting) return;
            _waiting = false;
            Time.timeScale = _previousTimeScale;
        }

        private void OnDisable()
        {
            if (_startAction != null)
            {
                _startAction.performed -= OnStartPressed;
                _startAction.Disable();
            }
            StopAllCoroutines();
            if (introPanel != null) introPanel.SetActive(false);
            RestoreTime();
        }

        private void OnDestroy()
        {
            if (_startAction != null) _startAction.Dispose();
        }
    }
}
