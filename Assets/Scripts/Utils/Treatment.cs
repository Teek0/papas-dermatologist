using System;
using System.Collections.Generic;
using UnityEngine;

public class Treatment
{
    private int difficultyLevel; // Not readonly 'cause we might want to reduce difficulty along the way for reasons
    private readonly List<SkinCondition> skinConditions;
    private readonly int payment;
    private readonly int timeLimit; // in seconds
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

    private static bool ValidateSkinCondition(Sprite[][] _appearanceOptions, int _areaIndex, int _typeIndex)
    {
        if (_appearanceOptions.Length <= _areaIndex) return false;
        if (_appearanceOptions[_areaIndex].Length <= _typeIndex) return false;
        return true;
    }

    public Treatment(int _difficultyLevel, GameConstantsSO _constants, Sprite[][] _appearanceOptions)
    {
        skinConditions = new();

        try
        {
            difficultyLevel = _difficultyLevel;

            payment = _constants.BasePayment * difficultyLevel;
            timeLimit = Mathf.CeilToInt(_constants.BaseTimeLimit / (float)difficultyLevel);

            List<string> skinConditionTypes = new()
            {
                "acné",
                "arrugas",
                "cicatrices"
            };

            string currentType;
            int pickIndexInList;
            int areaIndex; // 0 -> forehead; 1 -> cheeks; 2 -> chin
            int numberOfAreas;
            int[] maxAreasPerCondition = { 3, 2, 1 };
            int[] usedAreasPerCondition = { 0, 0, 0 };
            List<int> usedAreas = new();
            
            for (int i = 0; i < difficultyLevel; i++)
            {
                pickIndexInList = UnityEngine.Random.Range(0, skinConditionTypes.Count);
                currentType = skinConditionTypes[pickIndexInList];
                skinConditionTypes.RemoveAt(pickIndexInList);

                int spriteTypeIndex = GetTypeSpriteIndex(currentType);


                if (currentType == "arrugas")
                {
                    numberOfAreas = UnityEngine.Random.Range(1, 3);
                    areaIndex = UnityEngine.Random.Range(0, numberOfAreas);

                    for (int j = 0; j < numberOfAreas; j++)
                    {
                        if (usedAreasPerCondition[spriteTypeIndex] == maxAreasPerCondition[difficultyLevel - 1]) break;
                        while (usedAreas.Contains(areaIndex))
                            areaIndex = UnityEngine.Random.Range(0, numberOfAreas);
                    }
                }
                else
                {
                    numberOfAreas = UnityEngine.Random.Range(1, 4);
                    areaIndex = UnityEngine.Random.Range(0, numberOfAreas);     

                    for (int j = 0; j < numberOfAreas; j++)
                    {
                        if (usedAreasPerCondition[spriteTypeIndex] == maxAreasPerCondition[difficultyLevel - 1]) break;
                        while (usedAreas.Contains(areaIndex))
                            areaIndex = UnityEngine.Random.Range(0, numberOfAreas);
                    }
                }

                if (!ValidateSkinCondition(_appearanceOptions, areaIndex, spriteTypeIndex)) continue;
                skinConditions.Add(new SkinCondition(_appearanceOptions[areaIndex][spriteTypeIndex]));

                usedAreas.Add(areaIndex);
                usedAreasPerCondition[spriteTypeIndex]++;
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
