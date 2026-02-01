using System;
using System.Collections.Generic;
using UnityEngine;

public class Treatment
{
    private int difficultyLevel;
    private readonly List<SkinCondition> skinConditions;
    private readonly int payment;
    private readonly int timeLimit;

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

    public Treatment(int _difficultyLevel, GameConstantsSO _constants, Sprite[][] _appearanceOptions)
    {
        skinConditions = new();

        try
        {
            difficultyLevel = Mathf.Clamp(_difficultyLevel, 1, 3);

            payment = _constants.BasePayment * difficultyLevel;
            timeLimit = Mathf.CeilToInt(_constants.BaseTimeLimit / (float)difficultyLevel);

            List<string> skinConditionTypes = new()
            {
                "acné",
                "arrugas",
                "cicatrices"
            };

            int totalAreas = (_appearanceOptions != null) ? _appearanceOptions.Length : 0;
            if (totalAreas <= 0)
            {
                Debug.LogError("Treatment: _appearanceOptions is null or empty. Cannot generate treatment.");
                return;
            }

            List<int> usedAreas = new();

            for (int i = 0; i < difficultyLevel; i++)
            {
                if (skinConditionTypes.Count == 0) break;

                int pickIndexInList = UnityEngine.Random.Range(0, skinConditionTypes.Count);
                string currentType = skinConditionTypes[pickIndexInList];
                skinConditionTypes.RemoveAt(pickIndexInList);

                int spriteTypeIndex = GetTypeSpriteIndex(currentType);

                int allowedAreasCount = currentType == "arrugas"
                    ? Mathf.Min(2, totalAreas)
                    : totalAreas;

                List<int> candidates = new();
                for (int a = 0; a < allowedAreasCount; a++)
                {
                    if (!usedAreas.Contains(a))
                        candidates.Add(a);
                }
                if (candidates.Count == 0)
                {
                    for (int a = 0; a < allowedAreasCount; a++)
                        candidates.Add(a);
                }

                int areaIndex = candidates[UnityEngine.Random.Range(0, candidates.Count)];

                if (!ValidateSkinCondition(_appearanceOptions, areaIndex, spriteTypeIndex))
                {
                    continue;
                }

                skinConditions.Add(new SkinCondition(_appearanceOptions[areaIndex][spriteTypeIndex]));
                if (!usedAreas.Contains(areaIndex))
                    usedAreas.Add(areaIndex);
            }

            if (skinConditions.Count == 0)
            {
                Debug.LogError("Treatment generated with ZERO skin conditions. Check _appearanceOptions and sprite naming.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public int Payment => payment;
    public int TimeLimit => timeLimit;
    public List<SkinCondition> SkinConditions => skinConditions;
}
