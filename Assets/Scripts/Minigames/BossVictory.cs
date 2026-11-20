using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossHealth), typeof(Rigidbody2D))]
public class BossVictory : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject healthBar;

    [Header("Salto de despedida")]
    [SerializeField, Min(0.1f)] private float exitHorizontalSpeed = 12f;
    [SerializeField, Min(0.1f)] private float exitJumpSpeed = 18f;
    [SerializeField, Min(0.1f)] private float maxExitDuration = 4f;

    private BossHealth _health;
    private Rigidbody2D _body;
    private BossAttack _attack;
    private BossGroundSlam _slam;
    private BossAnimation _animation;
    private Collider2D[] _colliders;
    private SpriteRenderer _sprite;
    private bool _finished;

    private void Awake()
    {
        _health = GetComponent<BossHealth>();
        _body = GetComponent<Rigidbody2D>();
        _attack = GetComponent<BossAttack>();
        _slam = GetComponent<BossGroundSlam>();
        _animation = GetComponent<BossAnimation>();
        _colliders = GetComponentsInChildren<Collider2D>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _health.Defeated += OnDefeated;
    }

    private void OnDisable()
    {
        _health.Defeated -= OnDefeated;
    }

    private void OnDefeated()
    {
        if (_finished) return;
        _finished = true;
        if (_attack != null) _attack.enabled = false;
        if (_slam != null) _slam.enabled = false;
        foreach (Collider2D collider in _colliders) collider.enabled = false;
        if (playerHealth != null) playerHealth.PreventDamage();
        if (healthBar != null) healthBar.SetActive(false);

        // Retira los peligros existentes sin tocar las frutas del jugador.
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            if (root.GetComponent<BossOrb>() != null || root.GetComponent<GroundMound>() != null)
            {
                root.SetActive(false);
                Destroy(root);
            }
        }
        StartCoroutine(ExitAndWin());
    }

    private IEnumerator ExitAndWin()
    {
        float direction = sceneCamera == null || transform.position.x >= sceneCamera.transform.position.x ? 1f : -1f;
        _body.linearVelocity = new Vector2(direction * exitHorizontalSpeed, exitJumpSpeed);
        if (_animation != null) _animation.PlayJump(exitJumpSpeed);
        float elapsed = 0f;
        while (elapsed < maxExitDuration)
        {
            elapsed += Time.deltaTime;
            if (sceneCamera != null)
            {
                float depth = Mathf.Abs(transform.position.z - sceneCamera.transform.position.z);
                Vector3 min = sceneCamera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
                Vector3 max = sceneCamera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
                Bounds bounds = _sprite == null ? new Bounds(transform.position, Vector3.zero) : _sprite.bounds;
                if (bounds.min.x > max.x || bounds.max.x < min.x || bounds.min.y > max.y || bounds.max.y < min.y) break;
            }
            yield return null;
        }
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Destroy(gameObject);
    }
}
