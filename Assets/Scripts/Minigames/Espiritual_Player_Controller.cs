
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 6f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float checkRadius = 0.2f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;
    Animator animator;

    float moveInput;
    bool isGrounded;
    bool isAttacking;

    // Dirección original del personaje
    Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        originalScale = transform.localScale;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            groundLayer
        );

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);

        // Voltear personaje según dirección
        if (moveInput > 0)
        {
            transform.localScale = originalScale;
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(
                -originalScale.x,
                originalScale.y,
                originalScale.z
            );
        }

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }
    }

    public void SetAttacking(bool value)
    {
        isAttacking = value;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            moveInput * speed,
            rb.linearVelocity.y
        );
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}
