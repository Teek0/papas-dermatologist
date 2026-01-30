using System;
using System.Collections.Generic;
using UnityEngine;

public class Treatment
{
    private int difficultyLevel; // Not readonly 'cause we might want to reduce difficulty along the way for reasons
    private readonly List<SkinCondition> skinConditions;
    private readonly int payment;
    private readonly int timeLimit; // in seconds

    public Treatment(int _difficultyLevel, GameConstantsSO _constants, Sprite[][] _appearanceOptions)
    {
        try
        {
            skinConditions = new();

            difficultyLevel = _difficultyLevel;
            payment = _constants.BasePayment * difficultyLevel;
            timeLimit = _constants.BasePayment / difficultyLevel;

            List<string> skinConditionTypes = new();
            skinConditionTypes.Add("acné");
            skinConditionTypes.Add("arrugas");
            skinConditionTypes.Add("cicatrices");

            string currentType;
            int typeIndex;
            int areaIndex = UnityEngine.Random.Range(0, 2); // 0 -> forehead; 1 -> cheeks; 2 -> chin
            int numberOfAreas;
            List<int> usedAreas = new();

            for (int i = 0; i < difficultyLevel; i++)
            {
                typeIndex = UnityEngine.Random.Range(0, skinConditionTypes.Count);
                currentType = skinConditionTypes[typeIndex];
                skinConditionTypes.RemoveAt(typeIndex);

                if (currentType == "arrugas")
                {
                    numberOfAreas = UnityEngine.Random.Range(1, 3);
                    for (int j = 0; j < numberOfAreas; j++)
                    {
                        while (usedAreas.Contains(areaIndex))
                        {
                            areaIndex = UnityEngine.Random.Range(0, 1);
                        }
                        usedAreas.Add(areaIndex);
                        skinConditions.Add(new SkinCondition(_appearanceOptions[areaIndex][typeIndex]));
                    }
                } else
                {
                    numberOfAreas = UnityEngine.Random.Range(1, 4);
                    for (int j = 0; j < numberOfAreas; j++)
                    {
                        while (usedAreas.Contains(areaIndex))
                        {
                            areaIndex = UnityEngine.Random.Range(0, 2);
                        }
                        usedAreas.Add(areaIndex);
                        skinConditions.Add(new SkinCondition(_appearanceOptions[areaIndex][typeIndex]));
                    }
                }

                usedAreas.Clear();
            }



        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }

    public int Payment => payment;
    public int TimeLimit => timeLimit;
    public List<SkinCondition> SkinConditions => skinConditions;
}
