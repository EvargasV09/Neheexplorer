using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class BossAttack : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private BossOrb orbPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ataques")]
    [SerializeField, Min(0.1f)] private float attackInterval = 2.5f;
    [SerializeField, Min(0.1f)] private float orbSpeed = 7f;
    [SerializeField, Range(0f, 80f)] private float spreadAngle = 18f;

    [Header("Salto")]
    [SerializeField, Min(0.1f)] private float jumpSpeed = 10f;
    [SerializeField, Min(0f)] private float jumpHorizontalSpeed = 7f;

    [Header("Daño por contacto")]
    [SerializeField, Min(1)] private int contactDamage = 1;

    [Header("Animación de disparo")]
    [SerializeField, Min(0f)] private float shootWindup = 0.3f;
    [SerializeField, Min(0f)] private float shootRecovery = 0.4f;

    private Rigidbody2D _body;
    private BoxCollider2D _collider;
    private BossAnimation _animation;
    private bool _isShooting;
    private float _nextAttackTime;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _animation = GetComponent<BossAnimation>();
    }

    private void Start()
    {
        _nextAttackTime = Time.time + attackInterval;
    }

    private void FixedUpdate()
    {
        bool grounded = _collider.IsTouchingLayers(groundLayer);
        if (grounded && _body.linearVelocity.y <= 0.1f)
            _body.linearVelocity = new Vector2(0f, _body.linearVelocity.y);

        if (player == null || _isShooting || Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + attackInterval;

        // Si está en el aire, usa los orbes en lugar de otro salto.
        if (grounded && Random.Range(0, 2) == 0)
            JumpTowardsPlayer();
        else
            ShootFan();
    }

    private void ShootFan()
    {
        if (_isShooting || player == null || orbPrefab == null || shootPoint == null) return;
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        _isShooting = true;
        if (_animation != null) _animation.PlayShoot();
        // El cuarto fotograma muestra la salida del proyectil.
        if (_animation != null) yield return new WaitForSeconds(shootWindup);
        if (player == null) { _isShooting = false; yield break; }
        Vector2 direction = (Vector2)(player.position - shootPoint.position);
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.left;
        direction.Normalize();

        for (int i = -1; i <= 1; i++)
        {
            Vector2 orbDirection = Quaternion.Euler(0f, 0f, i * spreadAngle) * direction;
            BossOrb orb = Instantiate(orbPrefab, shootPoint.position, Quaternion.identity);
            orb.Launch(orbDirection, orbSpeed);
        }
        if (_animation != null) yield return new WaitForSeconds(shootRecovery);
        _isShooting = false;
    }

    private void JumpTowardsPlayer()
    {
        // Calcula una velocidad horizontal para caer cerca del jugador.
        float gravity = Mathf.Max(0.01f, Mathf.Abs(Physics2D.gravity.y * _body.gravityScale));
        float flightTime = 2f * jumpSpeed / gravity;
        float horizontalSpeed = Mathf.Clamp((player.position.x - _body.position.x) / flightTime,
            -jumpHorizontalSpeed, jumpHorizontalSpeed);
        _body.linearVelocity = new Vector2(horizontalSpeed, jumpSpeed);
        if (_animation != null) _animation.PlayJump(jumpSpeed);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _isShooting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        DamagePlayer(collision.collider);
    }

    private void DamagePlayer(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(contactDamage);
    }
}
