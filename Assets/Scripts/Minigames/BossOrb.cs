using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class BossOrb : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField, Min(1)] private int damage = 1;

    [Header("Duración")]
    [SerializeField, Min(0.1f)] private float lifetime = 5f;

    private Rigidbody2D _body;
    private bool _hasHit;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction, float speed)
    {
        _body.linearVelocity = direction.normalized * Mathf.Max(0f, speed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null) return;

        // Solo afecta al jugador, nunca al jefe ni a las frutas.
        _hasHit = true;
        player.TakeDamage(damage);
        Destroy(gameObject);
    }
}
