using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class TreatmentEvaluator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FacePaintSurfaceWorld paintSurface;
    [SerializeField] private CreamSelectionManager creamSelection;

    [Header("Zone Masks")]
    [SerializeField] private Texture2D tzoneMask;
    [SerializeField] private MaskTransform tzoneTransform = new MaskTransform { scale = Vector2.one, offset = Vector2.zero, clamp = true };
    [SerializeField] private bool tzoneInvertV = true;

    [SerializeField] private Texture2D cheeksMask;
    [SerializeField] private MaskTransform cheeksTransform = new MaskTransform { scale = Vector2.one, offset = Vector2.zero, clamp = true };
    [SerializeField] private bool cheeksInvertV = true;

    [SerializeField] private Texture2D chinMask;
    [SerializeField] private MaskTransform chinTransform = new MaskTransform { scale = Vector2.one, offset = Vector2.zero, clamp = true };
    [SerializeField] private bool chinInvertV = true;

    [Header("Thresholds")]
    [SerializeField, Range(0f, 1f)] private float maskThreshold = 0.1f;
    [SerializeField, Range(0, 255)] private byte paintedAlphaThreshold = 20;
    [SerializeField, Range(0, 255)] private int colorTolerance = 40;

    [Header("Scoring")]
    [Tooltip("Si está OFF: lo pintado fuera del objetivo NO penaliza (se ignora).")]
    [SerializeField] private bool penalizePaintOutsideRequested = false;

    [SerializeField] private float dirtyPenaltyWeight = 0.8f;
    [SerializeField] private float wrongColorPenaltyWeight = 0.9f;
    [SerializeField] private float timeBonusWeight = 0.2f;

    [Header("Debug Mask Preview")]
    [Tooltip("SpriteRenderer encima del rostro para visualizar las máscaras mapeadas al PaintTexture.")]
    [SerializeField] private SpriteRenderer debugPreviewRenderer;

    [SerializeField, Range(0f, 1f)] private float debugPreviewAlpha = 0.35f;

    [System.Serializable]
    public struct MaskTransform
    {
        public Vector2 scale;
        public Vector2 offset;
        public bool clamp;
    }

    public struct EvalResult
    {
        public float correctCoverage;
        public float wrongColorRate;
        public float dirtyRate;
        public float finalScore;
        public int finalPayment;
    }

    private void Awake()
    {
        if (paintSurface == null)
            paintSurface = FindFirstObjectByType<FacePaintSurfaceWorld>();
        if (creamSelection == null)
            creamSelection = FindFirstObjectByType<CreamSelectionManager>();
    }

    public EvalResult Evaluate(
        List<SkinCondition> conditions,
        int basePay,
        float remainingTime,
        float roundDuration
    )
    {
        var result = new EvalResult();

        if (conditions == null || conditions.Count == 0 ||
            paintSurface == null || paintSurface.PaintTexture == null)
        {
            result.finalScore = 0f;
            result.finalPayment = 0;
            return result;
        }

        Texture2D tex = paintSurface.PaintTexture;
        int w = tex.width;
        int h = tex.height;
        Color32[] paint = tex.GetPixels32();

        bool[] requested = new bool[paint.Length];
        CreamType[] requiredCream = new CreamType[paint.Length];

        for (int i = 0; i < requiredCream.Length; i++)
            requiredCream[i] = CreamType.Acne;

        foreach (var sc in conditions)
        {
            if (!TryGetMaskForArea(sc.AfflictedArea, out var zone, out var tr, out var invertV))
                continue;

            if (zone == null)
                continue;

            if (!zone.isReadable)
            {
                Debug.LogWarning($"[TreatmentEvaluator] Mask '{zone.name}' NO es legible. Activa Read/Write en Import Settings.");
                continue;
            }

            CreamType cream = CreamSelectionManager.CreamForConditionType(sc.Type);
            MarkRequested(zone, tr, invertV, w, h, requested, requiredCream, cream);
        }

        int requestedCount = 0;

        int paintedInRequestedCount = 0;
        int paintedOutsideCount = 0;

        int correctCount = 0;
        int wrongColorCount = 0;

        for (int i = 0; i < paint.Length; i++)
        {
            if (requested[i]) requestedCount++;

            if (paint[i].a < paintedAlphaThreshold)
                continue;

            if (!requested[i])
            {
                if (penalizePaintOutsideRequested)
                    paintedOutsideCount++;
                continue;
            }

            paintedInRequestedCount++;

            if (IsColorMatch(paint[i], ColorForCream(requiredCream[i])))
                correctCount++;
            else
                wrongColorCount++;
        }

        if (paintedInRequestedCount == 0)
        {
            result.correctCoverage = 0f;
            result.wrongColorRate = 0f;
            result.dirtyRate = 0f;
            result.finalScore = 0f;
            result.finalPayment = 0;
            return result;
        }

        if (requestedCount == 0)
        {
            Debug.LogWarning("[TreatmentEvaluator] requestedCount == 0. No hay objetivo marcado. Revisa: masks, Read/Write, maskThreshold, transforms e invertV.");
            result.correctCoverage = 0f;
            result.wrongColorRate = 0f;
            result.dirtyRate = 0f;
            result.finalScore = 0f;
            result.finalPayment = Mathf.RoundToInt(basePay * 0.2f);
            return result;
        }

        result.correctCoverage = correctCount / (float)requestedCount;

        result.wrongColorRate = (paintedInRequestedCount > 0)
            ? wrongColorCount / (float)paintedInRequestedCount
            : 0f;

        if (penalizePaintOutsideRequested)
        {
            int totalPainted = paintedInRequestedCount + paintedOutsideCount;
            result.dirtyRate = (totalPainted > 0)
                ? paintedOutsideCount / (float)totalPainted
                : 0f;
        }
        else
        {
            result.dirtyRate = 0f;
        }

        float timeBonus = (roundDuration > 0f)
            ? Mathf.Clamp01(remainingTime / roundDuration) * timeBonusWeight
            : 0f;

        result.finalScore = Mathf.Clamp01(
            result.correctCoverage
            - result.dirtyRate * dirtyPenaltyWeight
            - result.wrongColorRate * wrongColorPenaltyWeight
            + timeBonus
        );

        float multiplier = Mathf.Lerp(0.2f, 1.2f, result.finalScore);
        result.finalPayment = Mathf.RoundToInt(basePay * multiplier);

        return result;
    }

    private bool TryGetMaskForArea(string area, out Texture2D mask, out MaskTransform tr, out bool invertV)
    {
        mask = null;
        tr = default;
        invertV = false;

        string key = NormalizeKey(area);

        if (key == "frente" || key == "tzone" || key == "zonat" || key == "zona t")
        {
            mask = tzoneMask;
            tr = tzoneTransform;
            invertV = tzoneInvertV;
            return true;
        }

        if (key == "mejillas" || key == "mejilla" || key == "cara" || key == "cachetes")
        {
            mask = cheeksMask;
            tr = cheeksTransform;
            invertV = cheeksInvertV;
            return true;
        }

        if (key == "barbilla" || key == "menton" || key == "mentón")
        {
            mask = chinMask;
            tr = chinTransform;
            invertV = chinInvertV;
            return true;
        }

        return false;
    }

    private void MarkRequested(
        Texture2D mask, MaskTransform tr, bool invertV,
        int w, int h,
        bool[] requested, CreamType[] requiredCream, CreamType cream
    )
    {
        Color32[] mp = mask.GetPixels32();
        int mw = mask.width;
        int mh = mask.height;

        bool useAlpha = HasAlphaVariation(mp);

        for (int y = 0; y < h; y++)
        {
            float v = (h <= 1) ? 0f : y / (float)(h - 1);

            for (int x = 0; x < w; x++)
            {
                float u = (w <= 1) ? 0f : x / (float)(w - 1);

                float uu = u * tr.scale.x + tr.offset.x;
                float vv = v * tr.scale.y + tr.offset.y;

                if (tr.clamp)
                {
                    uu = Mathf.Clamp01(uu);
                    vv = Mathf.Clamp01(vv);
                }
                else
                {
                    uu = uu - Mathf.Floor(uu);
                    vv = vv - Mathf.Floor(vv);
                }

                if (invertV)
                    vv = 1f - vv;

                int mx = Mathf.Clamp(Mathf.RoundToInt(uu * (mw - 1)), 0, mw - 1);
                int my = Mathf.Clamp(Mathf.RoundToInt(vv * (mh - 1)), 0, mh - 1);

                float m = SampleMaskValue(mp[my * mw + mx], useAlpha);
                if (m > maskThreshold)
                {
                    int i = y * w + x;
                    requested[i] = true;
                    requiredCream[i] = cream;
                }
            }
        }
    }

    private bool IsColorMatch(Color32 painted, Color expected)
    {
        Color32 e = expected;
        if (painted.a == 0)
            return false;

        float a = painted.a / 255f;
        float invA = a > 0f ? (1f / a) : 0f;

        byte pr = (byte)Mathf.Clamp(Mathf.RoundToInt(painted.r * invA), 0, 255);
        byte pg = (byte)Mathf.Clamp(Mathf.RoundToInt(painted.g * invA), 0, 255);
        byte pb = (byte)Mathf.Clamp(Mathf.RoundToInt(painted.b * invA), 0, 255);

        return Mathf.Abs(pr - e.r) <= colorTolerance &&
               Mathf.Abs(pg - e.g) <= colorTolerance &&
               Mathf.Abs(pb - e.b) <= colorTolerance;
    }

    private Color ColorForCream(CreamType cream)
    {
        if (creamSelection != null)
            return creamSelection.ColorForCream(cream);

        return cream switch
        {
            CreamType.Acne => new Color(1f, 0.35f, 0.75f),
            CreamType.Wrinkles => new Color(0.35f, 0.22f, 0.12f),
            _ => new Color(0.2f, 1f, 0.4f)
        };

    }

    private static bool HasAlphaVariation(Color32[] pixels)
    {
        if (pixels == null || pixels.Length == 0) return false;

        byte minA = 255;
        byte maxA = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            byte a = pixels[i].a;
            if (a < minA) minA = a;
            if (a > maxA) maxA = a;
            if (maxA - minA > 10) return true;
        }
        return false;
    }

    private static float SampleMaskValue(Color32 c, bool useAlpha)
    {
        if (useAlpha)
            return c.a / 255f;

        return (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b) / 255f;
    }

    private static string NormalizeKey(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";

        s = s.Trim().ToLowerInvariant();
        s = s.Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(s[i]);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(s[i]);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    [ContextMenu("DEBUG/Preview Zone Masks (Tzone=Red, Cheeks=Green, Chin=Blue)")]
    private void DebugPreviewZoneMasks()
    {
        if (paintSurface == null || paintSurface.PaintTexture == null)
        {
            Debug.LogWarning("[TreatmentEvaluator] No puedo previsualizar: falta paintSurface o PaintTexture.");
            return;
        }

        if (debugPreviewRenderer == null)
        {
            Debug.LogWarning("[TreatmentEvaluator] Asigna un SpriteRenderer a 'debugPreviewRenderer' para ver la previsualización.");
            return;
        }

        int w = paintSurface.PaintTexture.width;
        int h = paintSurface.PaintTexture.height;

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        var px = new Color32[w * h];

        Color32[] tpx = (tzoneMask != null && tzoneMask.isReadable) ? tzoneMask.GetPixels32() : null;
        int tw = (tzoneMask != null) ? tzoneMask.width : 0;
        int th = (tzoneMask != null) ? tzoneMask.height : 0;
        bool tUseA = (tpx != null) && HasAlphaVariation(tpx);

        Color32[] cpx = (cheeksMask != null && cheeksMask.isReadable) ? cheeksMask.GetPixels32() : null;
        int cw = (cheeksMask != null) ? cheeksMask.width : 0;
        int ch = (cheeksMask != null) ? cheeksMask.height : 0;
        bool cUseA = (cpx != null) && HasAlphaVariation(cpx);

        Color32[] kpx = (chinMask != null && chinMask.isReadable) ? chinMask.GetPixels32() : null;
        int kw = (chinMask != null) ? chinMask.width : 0;
        int kh = (chinMask != null) ? chinMask.height : 0;
        bool kUseA = (kpx != null) && HasAlphaVariation(kpx);

        for (int y = 0; y < h; y++)
        {
            float v = (h <= 1) ? 0f : y / (float)(h - 1);

            for (int x = 0; x < w; x++)
            {
                float u = (w <= 1) ? 0f : x / (float)(w - 1);

                bool inT = SampleZone(tpx, tw, th, tUseA, u, v, tzoneTransform, tzoneInvertV);
                bool inC = SampleZone(cpx, cw, ch, cUseA, u, v, cheeksTransform, cheeksInvertV);
                bool inK = SampleZone(kpx, kw, kh, kUseA, u, v, chinTransform, chinInvertV);

                Color32 col = new Color32(0, 0, 0, 0);
                if (inT) { col.r = 255; col.a = 255; }
                if (inC) { col.g = 255; col.a = 255; }
                if (inK) { col.b = 255; col.a = 255; }

                if (col.a > 0)
                    col.a = (byte)Mathf.RoundToInt(debugPreviewAlpha * 255f);

                px[y * w + x] = col;
            }
        }

        tex.SetPixels32(px);
        tex.Apply(false);

        var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        debugPreviewRenderer.sprite = sprite;

        Debug.Log("[TreatmentEvaluator] Preview generada. Si se ve espejada o corrida, ajusta *InvertV* y/o Transform (scale/offset).");
    }

    private bool SampleZone(
        Color32[] mp, int mw, int mh, bool useAlpha,
        float u, float v,
        MaskTransform tr, bool invertV
    )
    {
        if (mp == null || mw <= 0 || mh <= 0) return false;

        float uu = u * tr.scale.x + tr.offset.x;
        float vv = v * tr.scale.y + tr.offset.y;

        if (tr.clamp)
        {
            uu = Mathf.Clamp01(uu);
            vv = Mathf.Clamp01(vv);
        }
        else
        {
            uu = uu - Mathf.Floor(uu);
            vv = vv - Mathf.Floor(vv);
        }

        if (invertV)
            vv = 1f - vv;

        int mx = Mathf.Clamp(Mathf.RoundToInt(uu * (mw - 1)), 0, mw - 1);
        int my = Mathf.Clamp(Mathf.RoundToInt(vv * (mh - 1)), 0, mh - 1);

        float m = SampleMaskValue(mp[my * mw + mx], useAlpha);
        return m > maskThreshold;
    }
}
