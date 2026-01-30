using UnityEngine;

public class BrushInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform faceRectTransform;
    [SerializeField] private FacePaintSurface paintSurface;

    [Header("Input")]
    [SerializeField] private bool paintWhileHeld = true;

    private bool isPainting;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetUV(Input.mousePosition, out var uv))
            {
                isPainting = true;
                paintSurface.PaintAtUV(uv);
            }
        }

        if (paintWhileHeld && isPainting && Input.GetMouseButton(0))
        {
            if (TryGetUV(Input.mousePosition, out var uv))
            {
                paintSurface.PaintAtUV(uv);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isPainting = false;
        }
    }

    private bool TryGetUV(Vector2 screenPos, out Vector2 uv)
    {
        uv = Vector2.zero;

        if (!RectTransformUtility.RectangleContainsScreenPoint(faceRectTransform, screenPos))
            return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                faceRectTransform, screenPos, null, out Vector2 localPoint))
            return false;

        Rect r = faceRectTransform.rect;
        float u = Mathf.InverseLerp(r.xMin, r.xMax, localPoint.x);
        float v = Mathf.InverseLerp(r.yMin, r.yMax, localPoint.y);

        uv = new Vector2(u, v);
        return true;
    }

    public void SetPaintingEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled) isPainting = false;
    }
}

