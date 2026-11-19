using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class BossGroundSlam : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GroundMound moundPrefab;
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private LayerMask groundLayer;

    [Header("Onda de tierra")]
    [SerializeField, Range(1, 12)] private int moundsPerSide = 5;
    [SerializeField, Min(0.1f)] private float moundSpacing = 1.5f;
    [SerializeField, Min(0.05f)] private float waveInterval = 0.15f;
    [SerializeField, Min(0f)] private float startDistance = 0.6f;
    [SerializeField, Min(0.1f)] private float moundHeight = 1.05f;

    private Rigidbody2D _body;
    private BoxCollider2D _collider;
    private bool _jumpStarted;
    private bool _wasAirborne;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        bool grounded = _collider.IsTouchingLayers(groundLayer);
        if (_body.linearVelocity.y > 0.5f) _jumpStarted = true;
        if (_jumpStarted && !grounded) _wasAirborne = true;

        // Solo golpea tras un salto, no al iniciar la escena.
        if (!_wasAirborne || !grounded || _body.linearVelocity.y > 0.5f) return;
        _jumpStarted = false;
        _wasAirborne = false;
        if (moundPrefab != null) StartCoroutine(RaiseWave(_body.position.x, _collider.bounds.extents.x));
    }

    private IEnumerator RaiseWave(float originX, float bossHalfWidth)
    {
        float surfaceY = _collider.bounds.min.y;
        for (int i = 0; i < moundsPerSide; i++)
        {
            float distance = bossHalfWidth + startDistance + i * moundSpacing;
            SpawnMound(originX - distance, surfaceY);
            SpawnMound(originX + distance, surfaceY);
            yield return new WaitForSeconds(waveInterval);
        }
    }

    private void SpawnMound(float x, float surfaceY)
    {
        if (sceneCamera != null)
        {
            float depth = Mathf.Abs(transform.position.z - sceneCamera.transform.position.z);
            float left = sceneCamera.ViewportToWorldPoint(new Vector3(0f, 0f, depth)).x;
            float right = sceneCamera.ViewportToWorldPoint(new Vector3(1f, 0f, depth)).x;
            if (x < left + 0.65f || x > right - 0.65f) return;
        }

        RaycastHit2D ground = Physics2D.Raycast(new Vector2(x, surfaceY + 2f), Vector2.down, 4f, groundLayer);
        if (ground.collider == null) return;
        GroundMound mound = Instantiate(moundPrefab, new Vector3(x, ground.point.y, transform.position.z), Quaternion.identity);
        mound.SetFullHeight(moundHeight);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
