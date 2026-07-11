using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BrushCursorUI : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image circleImage;
    [SerializeField] private Image toolImage;
    [SerializeField] private CreamSelectionManager selectionManager;
    [SerializeField] private GameController gameController;

    [Header("Size")]
    [SerializeField] private float baseSize = 48f;

    [Header("Cursor Sprites")]
    [SerializeField] private Sprite acneCursorSprite;
    [SerializeField] private Sprite wrinklesCursorSprite;
    [SerializeField] private Sprite scarsCursorSprite;
    [SerializeField] private Sprite eraserCursorSprite;

    [Header("Cursor Placement")]
    [SerializeField] private Vector2 creamCursorSize = new Vector2(64f, 64f);
    [SerializeField] private Vector2 creamCursorHotspot = new Vector2(0.15f, 0.15f);
    [SerializeField] private Vector2 eraserCursorSize = new Vector2(64f, 64f);
    [SerializeField] private Vector2 eraserCursorHotspot = new Vector2(0.5f, 0.5f);

    [Header("Cursor Tint")]
    [SerializeField] private bool tintCreamCursorWithCreamColor;
    [SerializeField] private Color eraserCursorTint = Color.white;

    [Header("Precision Circle")]
    [SerializeField] private bool showPrecisionCircle = true;
    [SerializeField] private Vector2 precisionCircleSize = new Vector2(20f, 20f);
    [SerializeField, Range(0f, 1f)] private float creamPrecisionCircleAlpha = 0.45f;
    [SerializeField] private Color eraserPrecisionCircleColor = new Color(1f, 1f, 1f, 0.45f);

    private RectTransform rt;
    private RectTransform circleRt;
    private RectTransform toolRt;
    private Camera uiCamera;
    private Vector2 lastGoodPos;
    private Vector2 lastToolSize;
    private Vector2 lastHotspot;
    private Vector2 lastCircleSize;
    private Color lastToolColor;
    private Color lastCircleColor;
    private Sprite lastToolSprite;
    private bool visualsEnabled;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        Image rootImage = GetComponent<Image>();
        if (toolImage == null && rootImage != null && rootImage != circleImage)
            toolImage = rootImage;

        if (circleImage == null)
            circleImage = GetComponentInChildren<Image>(true);

        if (canvas == null) canvas = GetComponentInParent<Canvas>(true);
        EnsureToolLayerRendersAboveCircle();
        if (toolImage != null) toolRt = toolImage.rectTransform;
        if (circleImage != null) circleRt = circleImage.rectTransform;

        if (selectionManager == null) selectionManager = FindFirstObjectByType<CreamSelectionManager>();
        if (gameController == null) gameController = FindFirstObjectByType<GameController>();

        ConfigureAsVisualOnlyCursor();
        ResolveUICamera();

        lastGoodPos = rt.anchoredPosition;
        lastToolSize = rt.sizeDelta;
        lastHotspot = Vector2.one * 0.5f;
        lastCircleSize = circleRt != null ? circleRt.sizeDelta : precisionCircleSize;
        lastToolColor = toolImage != null ? toolImage.color : Color.white;
        lastCircleColor = circleImage != null ? circleImage.color : Color.white;
        lastToolSprite = toolImage != null ? toolImage.sprite : null;
        visualsEnabled = IsAnyVisualEnabled();
    }

    private void OnEnable()
    {
        EnsureToolLayerRendersAboveCircle();
        ConfigureAsVisualOnlyCursor();
        ResolveUICamera();
    }

    private void EnsureToolLayerRendersAboveCircle()
    {
        if (toolImage == null || toolImage.transform != transform)
            return;

        Image rootImage = toolImage;
        GameObject toolLayer = new GameObject("ToolLayer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        toolLayer.transform.SetParent(transform, false);

        var layerRt = toolLayer.GetComponent<RectTransform>();
        layerRt.anchorMin = Vector2.one * 0.5f;
        layerRt.anchorMax = Vector2.one * 0.5f;
        layerRt.pivot = Vector2.one * 0.5f;
        layerRt.anchoredPosition = Vector2.zero;
        layerRt.sizeDelta = rt != null ? rt.sizeDelta : Vector2.zero;

        var layerImage = toolLayer.GetComponent<Image>();
        layerImage.sprite = rootImage.sprite;
        layerImage.color = rootImage.color;
        layerImage.material = rootImage.material;
        layerImage.type = rootImage.type;
        layerImage.preserveAspect = rootImage.preserveAspect;
        layerImage.raycastTarget = false;

        rootImage.enabled = false;
        rootImage.raycastTarget = false;

        toolImage = layerImage;
        toolRt = layerRt;
        toolLayer.transform.SetAsLastSibling();
    }

    private void ConfigureAsVisualOnlyCursor()
    {
        var graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in graphics)
            graphic.raycastTarget = false;

        if (rt != null)
            rt.pivot = Vector2.one * 0.5f;
    }

    private void ResolveUICamera()
    {
        if (canvas == null) { uiCamera = null; return; }
        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : (canvas.worldCamera != null ? canvas.worldCamera : Camera.main);
    }

    private void Update()
    {
        if (canvas == null || rt == null)
            return;

        bool canPaint = (gameController == null) ? true : gameController.CanPaint;
        if (canPaint != visualsEnabled)
        {
            SetVisualsEnabled(canPaint);
            visualsEnabled = canPaint;
        }
        if (!canPaint) return;

        bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            GameplayPointerInput.ScreenPosition,
            uiCamera,
            out var localPos
        );

        Vector2 pointerPos = ok ? localPos : lastGoodPos;
        if (ok)
            lastGoodPos = localPos;

        UpdateCursorVisual();
        PositionCursorAtHotspot(pointerPos);
    }

    private void SetVisualsEnabled(bool enabled)
    {
        if (toolImage != null)
            toolImage.enabled = enabled && GetCurrentSprite() != null;

        if (circleImage != null)
            circleImage.enabled = enabled && showPrecisionCircle;
    }

    private bool IsAnyVisualEnabled()
    {
        bool toolEnabled = toolImage != null && toolImage.enabled;
        bool circleEnabled = circleImage != null && circleImage.enabled;
        return toolEnabled || circleEnabled;
    }

    private void UpdateCursorVisual()
    {
        Sprite sprite = GetCurrentSprite();
        if (toolImage != null)
        {
            if (sprite != lastToolSprite)
            {
                toolImage.sprite = sprite;
                lastToolSprite = sprite;
            }

            bool shouldShowTool = sprite != null;
            if (toolImage.enabled != shouldShowTool)
                toolImage.enabled = shouldShowTool;

            Color toolColor = GetCurrentToolTint();
            if (toolColor != lastToolColor)
            {
                toolImage.color = toolColor;
                lastToolColor = toolColor;
            }
        }

        Vector2 toolSize = GetCurrentToolSize();
        if (toolSize != lastToolSize)
        {
            rt.sizeDelta = toolSize;
            if (toolRt != null)
                toolRt.sizeDelta = toolSize;
            lastToolSize = toolSize;
        }

        Vector2 hotspot = GetCurrentHotspot();
        if (hotspot != lastHotspot)
        {
            lastHotspot = hotspot;
        }

        UpdatePrecisionCircle(toolSize, hotspot);
    }

    private void PositionCursorAtHotspot(Vector2 pointerPos)
    {
        Vector2 toolSize = GetCurrentToolSize();
        Vector2 hotspot = GetCurrentHotspot();
        Vector2 hotspotOffset = new Vector2(
            (hotspot.x - 0.5f) * toolSize.x,
            (hotspot.y - 0.5f) * toolSize.y
        );

        rt.anchoredPosition = pointerPos - hotspotOffset;
    }

    private void UpdatePrecisionCircle(Vector2 toolSize, Vector2 hotspot)
    {
        if (circleImage == null || circleRt == null)
            return;

        if (circleImage.enabled != showPrecisionCircle)
            circleImage.enabled = showPrecisionCircle;

        Color circleColor = GetCurrentPrecisionCircleTint();
        if (circleColor != lastCircleColor)
        {
            circleImage.color = circleColor;
            lastCircleColor = circleColor;
        }

        if (precisionCircleSize != lastCircleSize)
        {
            circleRt.sizeDelta = precisionCircleSize;
            lastCircleSize = precisionCircleSize;
        }

        circleRt.pivot = Vector2.one * 0.5f;
        circleRt.anchoredPosition = new Vector2(
            (hotspot.x - 0.5f) * toolSize.x,
            (hotspot.y - 0.5f) * toolSize.y
        );
    }

    private Sprite GetCurrentSprite()
    {
        if (selectionManager == null)
            return toolImage != null ? toolImage.sprite : null;

        if (selectionManager.IsEraserSelected)
            return eraserCursorSprite != null ? eraserCursorSprite : (toolImage != null ? toolImage.sprite : null);

        return selectionManager.CurrentCream switch
        {
            CreamType.Acne => acneCursorSprite != null ? acneCursorSprite : (toolImage != null ? toolImage.sprite : null),
            CreamType.Wrinkles => wrinklesCursorSprite != null ? wrinklesCursorSprite : (toolImage != null ? toolImage.sprite : null),
            CreamType.Scars => scarsCursorSprite != null ? scarsCursorSprite : (toolImage != null ? toolImage.sprite : null),
            _ => toolImage != null ? toolImage.sprite : null
        };
    }

    private Color GetCurrentToolTint()
    {
        if (selectionManager != null && selectionManager.IsEraserSelected)
            return eraserCursorTint;

        if (selectionManager != null && tintCreamCursorWithCreamColor)
            return selectionManager.CurrentColor;

        return Color.white;
    }

    private Color GetCurrentPrecisionCircleTint()
    {
        if (selectionManager != null && selectionManager.IsEraserSelected)
            return eraserPrecisionCircleColor;

        Color color = selectionManager != null ? selectionManager.CurrentColor : Color.white;
        color.a = creamPrecisionCircleAlpha;
        return color;
    }

    private Vector2 GetCurrentToolSize()
    {
        if (selectionManager != null && selectionManager.IsEraserSelected)
            return eraserCursorSize;

        if (creamCursorSize.x > 0f && creamCursorSize.y > 0f)
            return creamCursorSize;

        return new Vector2(baseSize, baseSize);
    }

    private Vector2 GetCurrentHotspot()
    {
        Vector2 hotspot = selectionManager != null && selectionManager.IsEraserSelected
            ? eraserCursorHotspot
            : creamCursorHotspot;

        hotspot.x = Mathf.Clamp01(hotspot.x);
        hotspot.y = Mathf.Clamp01(hotspot.y);
        return hotspot;
    }
}
