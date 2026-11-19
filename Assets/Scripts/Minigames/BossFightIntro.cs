using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class BossFightIntro : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private Behaviour[] playerControls;

    private UnityEngine.InputSystem.InputAction _startAction;
    private bool[] _previousEnabled;
    private float _previousTimeScale;
    private bool _waiting;
    private bool _starting;

    private void Awake()
    {
        if (introPanel == null) return;
        _previousTimeScale = Time.timeScale;
        _previousEnabled = new bool[playerControls.Length];
        for (int i = 0; i < playerControls.Length; i++)
        {
            if (playerControls[i] == null) continue;
            _previousEnabled[i] = playerControls[i].enabled;
            playerControls[i].enabled = false;
        }
        _waiting = true;
        introPanel.SetActive(true);
        Time.timeScale = 0f;
        _startAction = new UnityEngine.InputSystem.InputAction("StartBossFight",
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
        // La tecla de inicio no dispara también un salto o una fruta.
        yield return null;
        introPanel.SetActive(false);
        ResumeFight();
    }

    private void ResumeFight()
    {
        if (!_waiting) return;
        _waiting = false;
        Time.timeScale = _previousTimeScale;
        for (int i = 0; i < playerControls.Length; i++)
            if (playerControls[i] != null) playerControls[i].enabled = _previousEnabled[i];
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
        ResumeFight();
    }

    private void OnDestroy()
    {
        if (_startAction != null) _startAction.Dispose();
    }
}
