using System.Globalization;
using System.Text;
using UnityEngine;

public enum CreamType
{
    Acne,
    Wrinkles,
    Scars
}

public class CreamSelectionManager : MonoBehaviour
{
    [Header("Default")]
    [SerializeField] private CreamType defaultCream = CreamType.Acne;

    [Header("Cream Colors")]
    [SerializeField] private Color acneColor = new Color(1f, 0.35f, 0.75f, 1f);
    [SerializeField] private Color wrinklesColor = new Color(0.35f, 0.22f, 0.12f, 1f);
    [SerializeField] private Color scarsColor = new Color(0.2f, 1f, 0.4f, 1f);

    public CreamType CurrentCream { get; private set; }

    public Color CurrentColor =>
        CurrentCream == CreamType.Acne ? acneColor :
        CurrentCream == CreamType.Wrinkles ? wrinklesColor :
        scarsColor;

    public Color ColorForCream(CreamType cream) =>
        cream == CreamType.Acne ? acneColor :
        cream == CreamType.Wrinkles ? wrinklesColor :
        scarsColor;

    private void Awake()
    {
        CurrentCream = defaultCream;
    }

    public void SelectCream(CreamType cream)
    {
        CurrentCream = cream;
        Debug.Log($"Crema seleccionada: {cream}");
    }

    public static CreamType CreamForConditionType(string type)
    {
        string key = NormalizeKey(type);

        if (key == "acne" || key.StartsWith("acn") || key.Contains("espinilla") || key.Contains("pimple"))
            return CreamType.Acne;
        if (key == "arrugas" || key == "arruga" || key.Contains("wrinkle"))
            return CreamType.Wrinkles;

        if (key == "cicatrices" || key == "cicatriz" || key.Contains("scar"))
            return CreamType.Scars;

        Debug.LogWarning($"CreamForConditionType: tipo desconocido '{type}' (key normalizada '{key}'). Usando Acne por defecto.");
        return CreamType.Acne;
    }

    private static string NormalizeKey(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";

        s = s.Replace('\uFFFD', ' ');

        s = s.Trim().ToLowerInvariant();

        string formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);

        foreach (char ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).Trim();
    }
}
