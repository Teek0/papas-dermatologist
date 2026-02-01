using UnityEngine;
using System;
using System.Globalization;
using System.Text;

public class SkinCondition
{
    private readonly string type;         
    private readonly string afflictedArea; 
    private readonly Sprite appearance;

    public SkinCondition(Sprite _skinConditionSprite)
    {
        if (_skinConditionSprite == null)
            throw new ArgumentNullException(nameof(_skinConditionSprite));

        string rawName = _skinConditionSprite.name ?? "";
        string name = NormalizeKey(rawName);

        if (name.Contains("acne") || name.Contains("espinilla") || name.Contains("espinillas"))
        {
            type = "acné";
        }
        else if (name.Contains("arruga") || name.Contains("arrugas"))
        {
            type = "arrugas";
        }
        else if (name.Contains("cicatriz") || name.Contains("cicatrices"))
        {
            type = "cicatrices";
        }
        else
        {
            throw new Exception($"Invalid skin condition sprite name (type not found): '{rawName}'");
        }

        if (name.Contains("frente"))
        {
            afflictedArea = "frente";
        }
        else if (name.Contains("mejilla") || name.Contains("mejillas"))
        {
            afflictedArea = "mejillas";
        }
        else if (name.Contains("barbilla") || name.Contains("menton") || name.Contains("mentón"))
        {
            afflictedArea = "barbilla";
        }
        else
        {
            afflictedArea = string.Empty;
            Debug.LogError($"SkinCondition: sprite '{rawName}' has no recognized area (Frente/Mejillas/Barbilla/Mentón).");
        }

        appearance = _skinConditionSprite;
    }

    public Sprite Sprite => appearance;
    public string Type => type;
    public string AfflictedArea => afflictedArea;

    private static string NormalizeKey(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        string formD = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);

        foreach (char c in formD)
        {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }
}
