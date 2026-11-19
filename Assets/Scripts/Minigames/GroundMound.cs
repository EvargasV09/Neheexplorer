using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class GroundMound : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField, Min(1)] private int damage = 1;

    [Header("Tiempos")]
    [SerializeField, Min(0.05f)] private float warningDuration = 0.4f;
    [SerializeField, Min(0.05f)] private float riseDuration = 0.2f;
    [SerializeField, Min(0.1f)] private float holdDuration = 0.8f;
    [SerializeField, Min(0.05f)] private float sinkDuration = 0.2f;

    private PolygonCollider2D _collider;
    private Vector3 _fullScale;

    private void Awake()
    {
        _collider = GetComponent<PolygonCollider2D>();
        _collider.enabled = false;
        _fullScale = transform.localScale;
        SetHeight(0.06f);
    }

    public void SetFullHeight(float height)
    {
        _fullScale.y = Mathf.Max(0.1f, height) / 1.05f;
        SetHeight(0.06f);
    }

    private IEnumerator Start()
    {
        // Primero marca el suelo sin hacer daño.
        yield return new WaitForSeconds(warningDuration);
        _collider.enabled = true;
        yield return AnimateHeight(0.06f, 1f, riseDuration);
        yield return new WaitForSeconds(holdDuration);
        _collider.enabled = false;
        yield return AnimateHeight(1f, 0.01f, sinkDuration);
        Destroy(gameObject);
    }

    private IEnumerator AnimateHeight(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetHeight(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetHeight(to);
    }

    private void SetHeight(float height)
    {
        transform.localScale = new Vector3(_fullScale.x, _fullScale.y * height, _fullScale.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DamagePlayer(other);
    }

    private void DamagePlayer(Collider2D other)
    {
        if (!_collider.enabled) return;
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage);
    }
}
