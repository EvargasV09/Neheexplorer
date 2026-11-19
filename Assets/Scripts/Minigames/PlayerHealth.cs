using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vidas")]
    [SerializeField, Min(1)] private int maxLives = 3;
    [SerializeField] private TMPro.TextMeshProUGUI livesText;

    [Header("Invulnerabilidad")]
    [SerializeField, Min(0.01f)] private float invulnerabilityDuration = 1f;
    [SerializeField, Min(0.01f)] private float blinkInterval = 0.1f;

    private int _currentLives;
    private float _invulnerableUntil;
    private bool _isDead;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null) _originalColor = _spriteRenderer.color;
        _currentLives = maxLives;
        UpdateLivesText();
    }

    public void TakeDamage(int amount)
    {
        if (_isDead || amount <= 0 || Time.time < _invulnerableUntil) return;

        _currentLives = Mathf.Max(0, _currentLives - amount);
        UpdateLivesText();
        if (_currentLives == 0)
        {
            _isDead = true;
            SceneManager.LoadScene(gameObject.scene.name);
            return;
        }

        // Bloquea golpes repetidos mientras parpadea.
        _invulnerableUntil = Time.time + invulnerabilityDuration;
        StartCoroutine(Blink());
    }

    public void PreventDamage()
    {
        StopAllCoroutines();
        _invulnerableUntil = float.PositiveInfinity;
        if (_spriteRenderer != null) _spriteRenderer.color = _originalColor;
    }

    private void UpdateLivesText()
    {
        if (livesText != null) livesText.SetText("Vidas: {0}", _currentLives);
    }

    private IEnumerator Blink()
    {
        bool faded = false;
        while (Time.time < _invulnerableUntil)
        {
            faded = !faded;
            if (_spriteRenderer != null)
            {
                Color color = _originalColor;
                if (faded) color.a *= 0.2f;
                _spriteRenderer.color = color;
            }
            yield return new WaitForSeconds(Mathf.Min(blinkInterval, Mathf.Max(0f, _invulnerableUntil - Time.time)));
        }
        if (_spriteRenderer != null) _spriteRenderer.color = _originalColor;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (_spriteRenderer != null) _spriteRenderer.color = _originalColor;
    }
}
