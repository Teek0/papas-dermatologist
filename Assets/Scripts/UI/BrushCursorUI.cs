using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BrushCursorUI : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image circleImage;
    [SerializeField] private CreamSelectionManager selectionManager;
    [SerializeField] private GameController gameController;

    [Header("Size")]
    [SerializeField] private float baseSize = 48f;

    private RectTransform rt;
    private Camera uiCamera;
    private Vector2 lastGoodPos;
    private Vector2 lastSize;
    private Color lastColor;
    private bool lastEnabled;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        if (circleImage == null) circleImage = GetComponent<Image>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>(true);

        if (selectionManager == null) selectionManager = FindFirstObjectByType<CreamSelectionManager>();
        if (gameController == null) gameController = FindFirstObjectByType<GameController>();

        ResolveUICamera();
        lastGoodPos = rt.anchoredPosition;
        lastSize = rt.sizeDelta;
        lastColor = circleImage != null ? circleImage.color : Color.white;
        lastEnabled = circleImage != null && circleImage.enabled;
    }

    private void OnEnable()
    {
        ResolveUICamera();
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
        if (canvas == null || rt == null || circleImage == null)
            return;

        bool canPaint = (gameController == null) ? true : gameController.CanPaint;
        if (canPaint != lastEnabled)
        {
            circleImage.enabled = canPaint;
            lastEnabled = canPaint;
        }
        if (!canPaint) return;

        bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            uiCamera,
            out var localPos
        );

        if (ok)
        {
            rt.anchoredPosition = localPos;
            lastGoodPos = localPos;
        }
        else
        {
            rt.anchoredPosition = lastGoodPos;
        }

        if (selectionManager != null)
        {
            Color c = selectionManager.CurrentColor;
            if (c != lastColor)
            {
                circleImage.color = c;
                lastColor = c;
            }
        }

        Vector2 size = new Vector2(baseSize, baseSize);
        if (size != lastSize)
        {
            rt.sizeDelta = size;
            lastSize = size;
        }
    }
}
