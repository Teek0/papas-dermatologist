using System;
using UnityEngine;

public class BrushInputWorld : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Collider2D paintCollider;
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private FacePaintSurfaceWorld paintSurface;
    [SerializeField] private CreamSelectionManager creamSelection;

    [Header("Game State")]
    [SerializeField] private GameController gameController;

    [Header("Input")]
    [SerializeField] private bool paintWhileHeld = true;

    private bool isPainting;
    public static event Action<bool> OnBrushValidStateChanged;

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

        if (creamSelection == null)
            creamSelection = FindFirstObjectByType<CreamSelectionManager>();
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
            Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

            if (paintCollider.OverlapPoint(worldPos) && TryGetUV(worldPos, out Vector2 uv))
            {
                isPainting = true;

                Color col = creamSelection != null ? creamSelection.CurrentColor : Color.white;
                paintSurface.PaintAtUV(uv, col);

                OnBrushValidStateChanged?.Invoke(isPainting);
            }
            else
            {
                isPainting = false;
                OnBrushValidStateChanged?.Invoke(isPainting);
            }
        }

        if (paintWhileHeld && isPainting && Input.GetMouseButton(0))
        {
            Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

            if (paintCollider.OverlapPoint(worldPos) && TryGetUV(worldPos, out Vector2 uv))
            {
                Color col = creamSelection != null ? creamSelection.CurrentColor : Color.white;
                paintSurface.PaintAtUV(uv, col);
            }
        }

        if (Input.GetMouseButtonUp(0))
            isPainting = false;
    }

    private bool TryGetUV(Vector2 worldPos, out Vector2 uv)
    {
        uv = default;

        if (targetSpriteRenderer == null || targetSpriteRenderer.sprite == null)
            return false;

        Vector2 local = targetSpriteRenderer.transform.InverseTransformPoint(worldPos);

        var b = targetSpriteRenderer.sprite.bounds;
        if (!b.Contains(local))
            return false;

        float nx = Mathf.InverseLerp(b.min.x, b.max.x, local.x);
        float ny = Mathf.InverseLerp(b.min.y, b.max.y, local.y);

        uv = new Vector2(nx, ny);
        return true;
    }

    public void SetPaintingEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled) isPainting = false;
    }
}
