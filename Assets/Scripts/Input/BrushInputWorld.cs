using UnityEngine;

public class BrushInputWorld : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Collider2D paintCollider;
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private FacePaintSurfaceWorld paintSurface;

    [Header("Game State")]
    [SerializeField] private GameController gameController;

    [Header("Input")]
    [SerializeField] private bool paintWhileHeld = true;

    private bool isPainting;

    private void Reset()
    {
        cam = Camera.main;
    }

    private void Awake()
    {
        if (gameController == null)
            gameController = FindFirstObjectByType<GameController>();

        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        if (gameController != null && !gameController.CanPaint)
        {
            isPainting = false;
            return;
        }

        if (paintSurface == null || cam == null || paintCollider == null || targetSpriteRenderer == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetUV(out var uv))
            {
                isPainting = true;
                paintSurface.PaintAtUV(uv);
            }
        }
        else if (paintWhileHeld && isPainting && Input.GetMouseButton(0))
        {
            if (TryGetUV(out var uv))
                paintSurface.PaintAtUV(uv);
        }

        if (Input.GetMouseButtonUp(0))
            isPainting = false;
    }

    private bool TryGetUV(out Vector2 uv)
    {
        uv = Vector2.zero;

        float depth = cam.WorldToScreenPoint(targetSpriteRenderer.transform.position).z;

        Vector3 mouse = Input.mousePosition;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, depth));
        Vector2 world2 = new Vector2(world.x, world.y);

        if (!paintCollider.OverlapPoint(world2))
            return false;

        var sprite = targetSpriteRenderer.sprite;
        if (sprite == null)
            return false;

        Vector3 local = targetSpriteRenderer.transform.InverseTransformPoint(world2);
        Bounds b = sprite.bounds;

        float u = Mathf.InverseLerp(b.min.x, b.max.x, local.x);
        float v = Mathf.InverseLerp(b.min.y, b.max.y, local.y);

        if (targetSpriteRenderer.flipX) u = 1f - u;
        if (targetSpriteRenderer.flipY) v = 1f - v;

        uv = new Vector2(u, v);
        return (u >= 0f && u <= 1f && v >= 0f && v <= 1f);
    }

    public void SetPaintingEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled) isPainting = false;
    }
}
