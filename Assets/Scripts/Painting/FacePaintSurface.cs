using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class FacePaintSurface : MonoBehaviour
{
    [Header("Paint Texture (Runtime Canvas)")]
    [Tooltip("Resolución de la textura pintable (lienzo). Más alto = más detalle, más costo.")]
    [SerializeField] private int textureSize = 256;

    [Header("Paint Area Mask (Where painting is allowed)")]
    [Tooltip("Máscara que define dónde se puede pintar. Alpha > Threshold = permitido. Debe tener Read/Write Enabled.")]
    [SerializeField] private Texture2D paintMask;

    [Tooltip("Umbral de alpha para permitir pintar en la máscara.")]
    [SerializeField, Range(0f, 1f)] private float maskThreshold = 0.1f;

    [Tooltip("Invierte el eje V al samplear la máscara (útil si la máscara está al revés).")]
    [SerializeField] private bool invertMaskV = false;

    [Header("Mask Look (Visual Appearance)")]
    [Tooltip("Textura de patrón (ruido/grano) para que la mascarilla no sea un color plano. Read/Write Enabled recomendado.")]
    [SerializeField] private Texture2D maskLookTexture;

    [Tooltip("Cuántas veces se repite el patrón visual sobre la cara. 1 = se estira una vez, 4 = se repite 4 veces.")]
    [SerializeField, Min(0.1f)] private float lookTiling = 4f;

    [Tooltip("Color base de la mascarilla. Se multiplica por el patrón visual.")]
    [SerializeField] private Color maskTint = new Color(1f, 0.47f, 0.7f, 1f);

    [Header("Brush Settings")]
    [Tooltip("Radio del pincel en píxeles (en la textura pintable).")]
    [SerializeField, Min(1)] private int brushRadius = 10;

    [Tooltip("Qué tan rápido se acumula la cobertura al pintar. 1 = cobertura completa inmediata.")]
    [SerializeField, Range(0.01f, 1f)] private float brushStrength = 0.25f;

    private RawImage rawImage;
    private Texture2D paintTexture;
    private Color32[] paintPixels;

    private Color32[] maskPixels;
    private int maskW, maskH;

    private Color32[] lookPixels;
    private int lookW, lookH;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        CreatePaintTexture();
        CacheMaskPixels();
        CacheLookPixels();
    }

    private void OnValidate()
    {
        textureSize = Mathf.Max(16, textureSize);
        brushRadius = Mathf.Max(1, brushRadius);
        lookTiling = Mathf.Max(0.1f, lookTiling);

        if (Application.isPlaying)
        {
            CacheMaskPixels();
            CacheLookPixels();
        }
    }

    private void CreatePaintTexture()
    {
        paintTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        paintTexture.filterMode = FilterMode.Point;
        paintTexture.wrapMode = TextureWrapMode.Clamp;

        paintPixels = new Color32[textureSize * textureSize];
        ClearTexture();

        rawImage.texture = paintTexture;
    }

    public void ClearTexture()
    {
        var clear = new Color32(0, 0, 0, 0);
        for (int i = 0; i < paintPixels.Length; i++)
            paintPixels[i] = clear;

        ApplyPixels();
    }

    private void CacheMaskPixels()
    {
        maskPixels = null;
        maskW = maskH = 0;

        if (paintMask == null)
            return;

        // Requiere Read/Write enabled. Si no, maskPixels quedará null y solo no bloqueará.
        try
        {
            maskPixels = paintMask.GetPixels32();
            maskW = paintMask.width;
            maskH = paintMask.height;
        }
        catch
        {
            maskPixels = null;
            maskW = maskH = 0;
        }
    }

    private void CacheLookPixels()
    {
        lookPixels = null;
        lookW = lookH = 0;

        if (maskLookTexture == null)
            return;

        try
        {
            lookPixels = maskLookTexture.GetPixels32();
            lookW = maskLookTexture.width;
            lookH = maskLookTexture.height;
        }
        catch
        {
            lookPixels = null;
            lookW = lookH = 0;
        }
    }

    public void PaintAtUV(Vector2 uv)
    {
        int cx = Mathf.RoundToInt(Mathf.Clamp01(uv.x) * (textureSize - 1));
        int cy = Mathf.RoundToInt(Mathf.Clamp01(uv.y) * (textureSize - 1));

        int r = brushRadius;
        int r2 = r * r;

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

                Color32 col = SampleLookTinted(u, v);
                paintPixels[idx] = new Color32(col.r, col.g, col.b, newA);
            }
        }

        ApplyPixels();
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

    private void ApplyPixels()
    {
        paintTexture.SetPixels32(paintPixels);
        paintTexture.Apply(false);
    }
}