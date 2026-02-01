using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FacePaintSurfaceWorld : MonoBehaviour
{
    [Header("Startup")]
    [Tooltip("Si está activo, el overlay se oculta y solo se muestra al primer trazo.")]
    [SerializeField] private bool hideRendererUntilFirstPaint = true;

    [Header("Paint Texture (Runtime)")]
    [SerializeField] private int textureSize = 256;
    [SerializeField] private float pixelsPerUnit = 100f;

    [Header("Paint Area Mask (Where painting is allowed)")]
    [SerializeField] private Texture2D paintMask;
    [SerializeField, Range(0f, 1f)] private float maskThreshold = 0.1f;
    [SerializeField] private bool invertMaskV = false;

    [Header("Mask Look (Visual Appearance)")]
    [SerializeField] private Texture2D maskLookTexture;
    [SerializeField, Min(0.1f)] private float lookTiling = 4f;
    [SerializeField] private Color maskTint = new Color(1f, 0.47f, 0.7f, 1f);

    [Header("Brush Settings")]
    [SerializeField, Min(1)] private int brushRadius = 10;
    [SerializeField, Range(0.01f, 1f)] private float brushStrength = 0.25f;

    private SpriteRenderer sr;

    private Texture2D paintTexture;
    private Color32[] paintPixels;

    private Color32[] maskPixels;
    private int maskW, maskH;

    private Color32[] lookPixels;
    private int lookW, lookH;

    private int currentTextureSize;
    private bool dirty;
    private bool hasPainted;

    public Texture2D PaintTexture => paintTexture;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        RecreatePaintTextureAndSprite();
        CacheMaskPixels();
        CacheLookPixels();

        ClearTexture(apply: true);

        if (hideRendererUntilFirstPaint)
            sr.enabled = false;
    }

    private void LateUpdate()
    {
        if (dirty)
        {
            paintTexture.SetPixels32(paintPixels);
            paintTexture.Apply(false);
            dirty = false;
        }
    }

    private void OnValidate()
    {
        textureSize = Mathf.Max(16, textureSize);
        brushRadius = Mathf.Max(1, brushRadius);
        lookTiling = Mathf.Max(0.1f, lookTiling);
        pixelsPerUnit = Mathf.Max(1f, pixelsPerUnit);

        if (!Application.isPlaying) return;

        if (paintTexture != null && textureSize != currentTextureSize)
        {
            RecreatePaintTextureAndSprite();
            CacheMaskPixels();
            CacheLookPixels();
            ClearTexture(apply: true);
        }
        else
        {
            CacheMaskPixels();
            CacheLookPixels();
        }
    }

    private void RecreatePaintTextureAndSprite()
    {
        if (paintTexture != null)
        {
            Destroy(paintTexture);
            paintTexture = null;
        }

        if (sr != null && sr.sprite != null)
        {
            Destroy(sr.sprite);
        }

        currentTextureSize = textureSize;

        paintTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        paintTexture.filterMode = FilterMode.Point;
        paintTexture.wrapMode = TextureWrapMode.Clamp;

        paintPixels = new Color32[textureSize * textureSize];

        var newSprite = Sprite.Create(
            paintTexture,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit
        );

        sr.sprite = newSprite;
        dirty = true;
    }

    public void ClearTexture(bool apply = true)
    {
        var clear = new Color32(0, 0, 0, 0);
        for (int i = 0; i < paintPixels.Length; i++)
            paintPixels[i] = clear;

        hasPainted = false;

        if (hideRendererUntilFirstPaint)
            sr.enabled = false;

        if (apply)
            dirty = true;
    }

    private void CacheMaskPixels()
    {
        maskPixels = null;
        maskW = maskH = 0;

        if (paintMask == null) return;

        if (!paintMask.isReadable)
        {
            Debug.LogWarning($"[FacePaintSurfaceWorld] paintMask '{paintMask.name}' no es legible. Activa Read/Write en Import Settings, o la máscara será ignorada.");
            return;
        }

        maskPixels = paintMask.GetPixels32();
        maskW = paintMask.width;
        maskH = paintMask.height;
    }

    private void CacheLookPixels()
    {
        lookPixels = null;
        lookW = lookH = 0;

        if (maskLookTexture == null) return;

        if (!maskLookTexture.isReadable)
        {
            Debug.LogWarning($"[FacePaintSurfaceWorld] maskLookTexture '{maskLookTexture.name}' no es legible. Activa Read/Write si quieres usar esta textura de look.");
            return;
        }

        lookPixels = maskLookTexture.GetPixels32();
        lookW = maskLookTexture.width;
        lookH = maskLookTexture.height;
    }

    public void PaintAtUV(Vector2 uv, Color color)
    {
        if (paintTexture == null || paintPixels == null || paintPixels.Length != textureSize * textureSize)
            RecreatePaintTextureAndSprite();

        if (hideRendererUntilFirstPaint && !hasPainted)
            sr.enabled = true;

        hasPainted = true;

        int cx = Mathf.RoundToInt(Mathf.Clamp01(uv.x) * (textureSize - 1));
        int cy = Mathf.RoundToInt(Mathf.Clamp01(uv.y) * (textureSize - 1));

        int r = brushRadius;
        int r2 = r * r;

        Color32 target = (Color32)color;

        for (int dy = -r; dy <= r; dy++)
        {
            int py = cy + dy;
            if (py < 0 || py >= textureSize) continue;

            for (int dx = -r; dx <= r; dx++)
            {
                int px = cx + dx;
                if (px < 0 || px >= textureSize) continue;

                if (dx * dx + dy * dy > r2) continue;

                float u = (float)px / (textureSize - 1);
                float v = (float)py / (textureSize - 1);

                if (!IsAllowedAtUV(u, v))
                    continue;

                int idx = py * textureSize + px;

                byte currentA = paintPixels[idx].a;
                int addA = Mathf.RoundToInt(255f * brushStrength);
                byte newA = (byte)Mathf.Clamp(currentA + addA, 0, 255);

                Color32 prev = paintPixels[idx];
                float t = (addA / 255f);

                byte rCol = (byte)Mathf.RoundToInt(Mathf.Lerp(prev.r, target.r, t));
                byte gCol = (byte)Mathf.RoundToInt(Mathf.Lerp(prev.g, target.g, t));
                byte bCol = (byte)Mathf.RoundToInt(Mathf.Lerp(prev.b, target.b, t));

                paintPixels[idx] = new Color32(rCol, gCol, bCol, newA);
            }
        }

        dirty = true;
    }

    private bool IsAllowedAtUV(float u, float v)
    {
        if (maskPixels == null || maskW <= 0 || maskH <= 0)
            return true;

        float vv = invertMaskV ? (1f - v) : v;

        int mx = Mathf.Clamp(Mathf.RoundToInt(u * (maskW - 1)), 0, maskW - 1);
        int my = Mathf.Clamp(Mathf.RoundToInt(vv * (maskH - 1)), 0, maskH - 1);

        int midx = my * maskW + mx;

        float a = maskPixels[midx].a / 255f;
        return a > maskThreshold;
    }

    private Color32 SampleLookTinted(float u, float v)
    {
        Color look = Color.white;

        if (lookPixels != null && lookW > 0 && lookH > 0)
        {
            float tu = u * lookTiling;
            float tv = v * lookTiling;

            tu = tu - Mathf.Floor(tu);
            tv = tv - Mathf.Floor(tv);

            int tx = Mathf.Clamp(Mathf.RoundToInt(tu * (lookW - 1)), 0, lookW - 1);
            int ty = Mathf.Clamp(Mathf.RoundToInt(tv * (lookH - 1)), 0, lookH - 1);

            int tidx = ty * lookW + tx;
            Color32 lp = lookPixels[tidx];
            look = new Color(lp.r / 255f, lp.g / 255f, lp.b / 255f, lp.a / 255f);
        }

        Color mixed = new Color(maskTint.r * look.r, maskTint.g * look.g, maskTint.b * look.b, 1f);
        return (Color32)mixed;
    }
}
