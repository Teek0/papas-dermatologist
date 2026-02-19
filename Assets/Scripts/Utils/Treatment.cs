using System;
using System.Collections.Generic;
using UnityEngine;

public class Treatment
{
    private int difficultyLevel;
    private readonly int requestedDifficulty;

    private readonly List<SkinCondition> skinConditions;

    private int payment;
    private int timeLimit;

    private readonly Dictionary<int, SkinCondition> conditionByArea = new();

    private static int GetTypeSpriteIndex(string type)
    {
        return type switch
        {
            "acné" => 0,
            "arrugas" => 1,
            "cicatrices" => 2,
            _ => throw new Exception("Unknown skin condition type: " + type)
        };
    }

    private static bool ValidateSkinCondition(Sprite[][] appearanceOptions, int areaIndex, int typeIndex)
    {
        if (appearanceOptions == null) return false;
        if (areaIndex < 0 || areaIndex >= appearanceOptions.Length) return false;
        if (appearanceOptions[areaIndex] == null) return false;
        if (typeIndex < 0 || typeIndex >= appearanceOptions[areaIndex].Length) return false;
        return appearanceOptions[areaIndex][typeIndex] != null;
    }

    private static string AreaName(int index)
    {
        return index switch
        {
            0 => "frente",
            1 => "mejillas",
            2 => "mentón",
            _ => $"area_{index}"
        };
    }

    private static string InferConditionTypeFromSprite(Sprite sprite)
    {
        if (sprite == null) return "desconocido";

        string n = sprite.name.ToLower();
        if (n.Contains("acne") || n.Contains("espinilla")) return "acné";
        if (n.Contains("arruga")) return "arrugas";
        if (n.Contains("cicatriz")) return "cicatrices";
        return "desconocido";
    }

    public Treatment(int _difficultyLevel, GameConstantsSO _constants, Sprite[][] _appearanceOptions)
    {
        skinConditions = new();

        requestedDifficulty = Mathf.Clamp(_difficultyLevel, 1, 3);

        if (_constants == null || _appearanceOptions == null || _appearanceOptions.Length == 0)
        {
            Debug.LogError("Treatment: datos insuficientes para generar tratamiento.");
            difficultyLevel = 1;
            payment = 0;
            timeLimit = 0;
            LogFinalSummary();
            return;
        }

        List<(string type, int area)> candidates = new();

        string[] allTypes = { "acné", "arrugas", "cicatrices" };
        int totalAreas = _appearanceOptions.Length;

        foreach (string type in allTypes)
        {
            int typeIndex = GetTypeSpriteIndex(type);

            int allowedAreas = type == "arrugas"
                ? Mathf.Min(2, totalAreas)
                : totalAreas;

            for (int area = 0; area < allowedAreas; area++)
            {
                if (ValidateSkinCondition(_appearanceOptions, area, typeIndex))
                {
                    candidates.Add((type, area));
                }
            }
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, candidates.Count);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        HashSet<int> usedAreas = new();
        HashSet<string> usedTypes = new();

        foreach (var pair in candidates)
        {
            if (skinConditions.Count >= requestedDifficulty)
                break;

            if (usedAreas.Contains(pair.area))
                continue;

            if (usedTypes.Contains(pair.type))
                continue;

            int typeIndex = GetTypeSpriteIndex(pair.type);
            Sprite sprite = _appearanceOptions[pair.area][typeIndex];
            if (sprite == null)
                continue;

            SkinCondition sc = new SkinCondition(sprite);

            skinConditions.Add(sc);
            conditionByArea[pair.area] = sc;
            usedAreas.Add(pair.area);
            usedTypes.Add(pair.type);
        }

        difficultyLevel = Mathf.Clamp(conditionByArea.Count, 1, 3);
        payment = _constants.BasePayment + 5 * difficultyLevel;
        timeLimit = Mathf.CeilToInt(_constants.BaseTimeLimit + (float)difficultyLevel * 3);

        if (difficultyLevel != requestedDifficulty)
        {
            Debug.LogWarning(
                $"Treatment: requestedDifficulty={requestedDifficulty} pero effectiveDifficulty={difficultyLevel} (combinaciones insuficientes)."
            );
        }

        LogFinalSummary();
    }

    private void LogFinalSummary()
    {
        string header =
            $"dificultad - {difficultyLevel} :: " +
            $"tiempo - {timeLimit} :: " +
            $"dinero - {payment}";

        List<string> lines = new() { header };

        for (int area = 0; area < 3; area++)
        {
            if (conditionByArea.TryGetValue(area, out SkinCondition sc) && sc != null)
            {
                string type = InferConditionTypeFromSprite(sc.Sprite);
                lines.Add($"{AreaName(area)} - {type}");
            }
        }

        Debug.Log("[Treatment Summary]\n" + string.Join("\n", lines));
    }

    public int Payment => payment;
    public int TimeLimit => timeLimit;
    public int RequestedDifficulty => requestedDifficulty;
    public int DifficultyLevel => difficultyLevel;
    public List<SkinCondition> SkinConditions => skinConditions;
}
