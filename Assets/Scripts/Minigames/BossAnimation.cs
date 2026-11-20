using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class BossAnimation : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform shootPoint;

    [Header("Suelo")]
    [SerializeField] private LayerMask groundLayer;

    private static readonly int IsJumping= Animator.StringToHash("IsJumping");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int JumpPlayback = Animator.StringToHash("JumpPlayback");
    private static readonly int JumpState = Animator.StringToHash("Jump");
    private Animator _animator;
    private Rigidbody2D _body;
    private BoxCollider2D _collider;
    private SpriteRenderer _sprite;
    private Vector3 _shootOffset;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _sprite = GetComponent<SpriteRenderer>();
        if (shootPoint != null) _shootOffset = shootPoint.localPosition;
    }

    private void LateUpdate()
    {
        bool airborne = !_collider.enabled || !_collider.IsTouchingLayers(groundLayer) || _body.linearVelocity.y > 0.5f;
        _animator.SetBool(IsJumping, airborne);
        if (player != null && _sprite != null)
        {
            bool left = !_collider.enabled ? _body.linearVelocity.x < 0f : player.position.x < transform.position.x;
            _sprite.flipX = left;
            if (shootPoint != null)
                shootPoint.localPosition = new Vector3(Mathf.Abs(_shootOffset.x) * (left ? -1f : 1f), _shootOffset.y, _shootOffset.z);
        }
    }

    public void PlayShoot()
    {
        _animator.SetTrigger(Shoot);
    }

    public void PlayJump(float verticalSpeed)
    {
        // El clip de un segundo acompaña la duración real del salto.
        float gravity = Mathf.Max(0.01f, Mathf.Abs(Physics2D.gravity.y * _body.gravityScale));
        float flightTime = Mathf.Max(0.1f, 2f * verticalSpeed / gravity);
        _animator.ResetTrigger(Shoot);
        _animator.SetFloat(JumpPlayback, 1f / flightTime);
        _animator.SetBool(IsJumping, true);
        _animator.Play(JumpState, 0, 0f);
    }
}
