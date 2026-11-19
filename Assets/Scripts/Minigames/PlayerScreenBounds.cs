using UnityEngine;

public class PlayerScreenBounds : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private Transform player;

    [Header("Bordes")]
    [SerializeField, Min(0f)] private float margin = 0.05f;
    [SerializeField, Min(0.1f)] private float wallThickness = 1f;

    private readonly BoxCollider2D[] _walls = new BoxCollider2D[4];
    private Vector2 _lastMin;
    private Vector2 _lastMax;

    private void Awake()
    {
        if (sceneCamera == null || player == null) { enabled = false; return; }
        string[] names = { "LeftBoundary", "RightBoundary", "BottomBoundary", "TopBoundary" };
        for (int i = 0; i < _walls.Length; i++)
        {
            GameObject wall = new GameObject(names[i]);
            wall.transform.SetParent(transform, false);
            _walls[i] = wall.AddComponent<BoxCollider2D>();
            // No bloquea al jefe, las frutas ni los orbes.
            _walls[i].excludeLayers = ~(1 << player.gameObject.layer);
            _walls[i].layerOverridePriority = 10;
        }
        UpdateBounds(true);
    }

    private void LateUpdate()
    {
        UpdateBounds(false);
    }

    private void UpdateBounds(bool force)
    {
        float depth = Mathf.Abs(player.position.z - sceneCamera.transform.position.z);
        Vector2 min = sceneCamera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
        Vector2 max = sceneCamera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
        min += Vector2.one * margin;
        max -= Vector2.one * margin;
        if (!force && min == _lastMin && max == _lastMax) return;
        _lastMin = min;
        _lastMax = max;
        Vector2 center = (min + max) * 0.5f;
        float width = max.x - min.x;
        float height = max.y - min.y;
        _walls[0].transform.position = new Vector3(min.x - wallThickness * 0.5f, center.y, player.position.z);
        _walls[1].transform.position = new Vector3(max.x + wallThickness * 0.5f, center.y, player.position.z);
        _walls[2].transform.position = new Vector3(center.x, min.y - wallThickness * 0.5f, player.position.z);
        _walls[3].transform.position = new Vector3(center.x, max.y + wallThickness * 0.5f, player.position.z);
        _walls[0].size = _walls[1].size = new Vector2(wallThickness, height + wallThickness * 2f);
        _walls[2].size = _walls[3].size = new Vector2(width + wallThickness * 2f, wallThickness);
    }
}
