using System;
using UnityEngine;

public class Treatment
{
    private int difficultyLevel; // Not readonly 'cause we might want to reduce difficulty along the way for reasons
    private readonly SkinCondition[] skinConditions;
    private readonly int payment;
    private readonly int timeLimit; // in seconds

    public Treatment(int _difficultyLevel, GameConstantsSO _constants, Sprite[] _appearanceOptions)
    {
        try
        {
            difficultyLevel = _difficultyLevel;
            payment = _constants.BasePayment * difficultyLevel;
            timeLimit = _constants.BasePayment / difficultyLevel;
            for (int i = 0; i < difficultyLevel; i++)
            {
                skinConditions[i] = new SkinCondition(_appearanceOptions);
            }
        } catch (Exception ex)
        {
            Debug.LogException(ex);
        }

    }

    public int Payment => payment;
    public int TimeLimit => timeLimit;
    public SkinCondition[] SkinConditions => skinConditions;
}
