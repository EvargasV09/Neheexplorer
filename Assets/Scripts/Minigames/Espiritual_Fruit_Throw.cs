
using UnityEngine;
using System.Collections;

public class FruitThrower : MonoBehaviour
{
    [Header("Frutas")]
    [SerializeField] Sprite[] frutas;
    [SerializeField] float throwForce = 12f;
    [SerializeField] float cooldown = 0.4f;

    [Header("Disparo")]
    [SerializeField] Transform firePoint;

    [Header("Animación")]
    Animator animator;
    PlayerController playerController;

    int frutaActual = 0;
    float nextThrowTime;
    bool isAttacking;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) &&
            Time.time >= nextThrowTime &&
            !isAttacking)
        {
            nextThrowTime = Time.time + cooldown;
            Throw();
        }
    }

    void Throw()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0f;

        Vector2 direction = (mouse - firePoint.position).normalized;

        // Crear la fruta usando la imagen seleccionada
        GameObject fruit = new GameObject("Fruta");
        fruit.transform.position = firePoint.position;

        SpriteRenderer spriteRenderer = fruit.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = frutas[frutaActual];

        Rigidbody2D rb = fruit.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = direction * throwForce;

        CircleCollider2D collider = fruit.AddComponent<CircleCollider2D>();

        Collider2D playerCollider = GetComponent<Collider2D>();

        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(collider, playerCollider);
        }

        Espiritual_FruitProjectil projectile =
            fruit.AddComponent<Espiritual_FruitProjectil>();

        // Pasar a la siguiente fruta
        frutaActual = (frutaActual + 1) % frutas.Length;

        if (animator != null)
        {
            StartCoroutine(AttackAnimation());
        }
    }

    IEnumerator AttackAnimation()
    {
        isAttacking = true;

        if (playerController != null)
            playerController.SetAttacking(true);

        animator.ResetTrigger("Shoot");
        animator.SetTrigger("Shoot");

        yield return null;

        while (!animator.GetCurrentAnimatorStateInfo(0)
            .IsName("PlayerDispara"))
        {
            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        if (playerController != null)
            playerController.SetAttacking(false);

        isAttacking = false;
    }
}
