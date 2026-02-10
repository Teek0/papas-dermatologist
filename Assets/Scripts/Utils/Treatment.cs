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

        if (_constants == null)
        {
            Debug.LogError("Treatment: GameConstantsSO es null. No se puede calcular pago/tiempo ni generar condiciones.");
            difficultyLevel = 1;
            payment = 0;
            timeLimit = 0;
            LogFinalSummary();
            return;
        }

        if (_appearanceOptions == null || _appearanceOptions.Length == 0)
        {
            Debug.LogError("Treatment: _appearanceOptions es null o vacío. No se pueden generar SkinConditions.");
            difficultyLevel = 1;
            payment = _constants.BasePayment * difficultyLevel;
            timeLimit = Mathf.CeilToInt(_constants.BaseTimeLimit / (float)difficultyLevel);
            LogFinalSummary();
            return;
        }

        List<string> remainingTypes = new()
        {
            "acné",
            "arrugas",
            "cicatrices"
        };

        HashSet<int> usedAreas = new();
        int attempts = 0;
        int maxAttempts = 40;

        int totalAreas = _appearanceOptions.Length;

        while (skinConditions.Count < requestedDifficulty &&
               remainingTypes.Count > 0 &&
               attempts < maxAttempts)
        {
            attempts++;

            int typePick = UnityEngine.Random.Range(0, remainingTypes.Count);
            string type = remainingTypes[typePick];
            int typeIndex = GetTypeSpriteIndex(type);

            int allowedAreas = type == "arrugas"
                ? Mathf.Min(2, totalAreas)
                : totalAreas;

            List<int> validAreas = new();
            for (int a = 0; a < allowedAreas; a++)
            {
                if (ValidateSkinCondition(_appearanceOptions, a, typeIndex))
                    validAreas.Add(a);
            }

            if (validAreas.Count == 0)
            {
                remainingTypes.RemoveAt(typePick);
                continue;
            }

            List<int> preferred = new();
            foreach (int a in validAreas)
                if (!usedAreas.Contains(a))
                    preferred.Add(a);

            int area = (preferred.Count > 0)
                ? preferred[UnityEngine.Random.Range(0, preferred.Count)]
                : validAreas[UnityEngine.Random.Range(0, validAreas.Count)];

            Sprite sprite = _appearanceOptions[area][typeIndex];
            if (sprite == null)
            {
                continue;
            }

            SkinCondition sc = new SkinCondition(sprite);

            skinConditions.Add(sc);
            conditionByArea[area] = sc;
            usedAreas.Add(area);

            remainingTypes.RemoveAt(typePick);
        }

        difficultyLevel = Mathf.Clamp(skinConditions.Count, 1, 3);
        payment = _constants.BasePayment * difficultyLevel;
        timeLimit = Mathf.CeilToInt(_constants.BaseTimeLimit / (float)difficultyLevel);

        if (difficultyLevel != requestedDifficulty)
        {
            Debug.LogWarning($"Treatment: requestedDifficulty={requestedDifficulty} pero effectiveDifficulty={difficultyLevel} (faltan zonas/tipos/sprites).");
        }

// ---------- LOG RESUMEN FINAL ----------
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
// ---------- LOG RESUMEN FINAL ----------

    public int Payment => payment;
    public int TimeLimit => timeLimit;
    public int RequestedDifficulty => requestedDifficulty;
    public int DifficultyLevel => difficultyLevel;
    public List<SkinCondition> SkinConditions => skinConditions;
}
