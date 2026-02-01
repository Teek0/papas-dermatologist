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

    [SerializeField] private Texture2D cheeksMask;
    [SerializeField] private MaskTransform cheeksTransform = new MaskTransform { scale = Vector2.one, offset = Vector2.zero, clamp = true };

    [SerializeField] private Texture2D chinMask;
    [SerializeField] private MaskTransform chinTransform = new MaskTransform { scale = Vector2.one, offset = Vector2.zero, clamp = true };

    [Header("Thresholds")]
    [SerializeField, Range(0f, 1f)] private float maskThreshold = 0.1f;
    [SerializeField, Range(0, 255)] private byte paintedAlphaThreshold = 20;
    [SerializeField, Range(0, 255)] private int colorTolerance = 40;

    [Header("Scoring")]
    [SerializeField] private float dirtyPenaltyWeight = 0.8f;
    [SerializeField] private float wrongColorPenaltyWeight = 0.9f;
    [SerializeField] private float timeBonusWeight = 0.2f;

    public struct EvalResult
    {
        public float correctCoverage;   // Correcto dentro del target / total target
        public float wrongColorRate;    // Incorrecto de color dentro del target / pintado dentro del target
        public float dirtyRate;         // Pintado fuera / total pintado
        public float finalScore;
        public int finalPayment;
    }

    [System.Serializable]
    public struct MaskTransform
    {
        public Vector2 scale;   // (1,1) = sin escala
        public Vector2 offset;  // (0,0) = sin desplazamiento
        public bool clamp;      // true recomendado
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
            if (!TryGetMaskForArea(sc.AfflictedArea, out var zone, out var tr)) continue;
            if (zone == null) continue;

            CreamType cream = CreamSelectionManager.CreamForConditionType(sc.Type);
            MarkRequested(zone, tr, w, h, requested, requiredCream, cream);
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
                paintedOutsideCount++;
                continue;
            }

            paintedInRequestedCount++;

            if (IsColorMatch(paint[i], ColorForCream(requiredCream[i])))
                correctCount++;
            else
                wrongColorCount++;
        }

        if (requestedCount == 0)
        {
            Debug.LogWarning("[TreatmentEvaluator] requestedCount == 0. No hay objetivo marcado. Revisa: mascaras (contenido), Read/Write, threshold y MaskTransform.");
            result.correctCoverage = 0f;
            result.wrongColorRate = 0f;
            result.dirtyRate = (paintedOutsideCount > 0) ? 1f : 0f;
            result.finalScore = 0f;
            result.finalPayment = Mathf.RoundToInt(basePay * 0.2f);
            return result;
        }

        result.correctCoverage = correctCount / (float)requestedCount;

        result.wrongColorRate = (paintedInRequestedCount > 0)
            ? wrongColorCount / (float)paintedInRequestedCount
            : 0f;

        int totalPainted = paintedInRequestedCount + paintedOutsideCount;
        result.dirtyRate = (totalPainted > 0)
            ? paintedOutsideCount / (float)totalPainted
            : 0f;

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

    // ---------------- helpers ----------------

    private bool TryGetMaskForArea(string area, out Texture2D mask, out MaskTransform tr)
    {
        mask = null;
        tr = default;

        string key = NormalizeKey(area);

        if (key == "frente" || key == "tzone" || key == "zonat" || key == "zona t")
        {
            mask = tzoneMask;
            tr = tzoneTransform;
            return true;
        }

        if (key == "mejillas" || key == "mejilla" || key == "cara" || key == "cachetes")
        {
            mask = cheeksMask;
            tr = cheeksTransform;
            return true;
        }

        if (key == "barbilla" || key == "menton" || key == "mentón")
        {
            mask = chinMask;
            tr = chinTransform;
            return true;
        }

        return false;
    }

    private void MarkRequested(
        Texture2D mask, MaskTransform tr, int w, int h,
        bool[] requested, CreamType[] requiredCream, CreamType cream
    )
    {
        if (mask == null || !mask.isReadable) return;

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
            CreamType.Acne => new Color(0.2f, 0.6f, 1f),
            CreamType.Wrinkles => new Color(0.2f, 1f, 0.4f),
            _ => new Color(1f, 0.35f, 0.75f)
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
}
