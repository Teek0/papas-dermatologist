using UnityEngine;
using System;

public class SkinCondition
{
    private readonly string type;
    private readonly string afflictedArea;
    private readonly Sprite appearance;

    public SkinCondition(Sprite _skinConditionSprite)
    {
        if (_skinConditionSprite.name.Contains("Acne"))
        {
            type = "acné";
        } else if (_skinConditionSprite.name.Contains("Arrugas"))
        {
            type = "arrugas";
        } else if (_skinConditionSprite.name.Contains("Cicatriz"))
        {
            type = "cicatrices";
        } else
        {
            throw new Exception("Invalid skin condition sprite name");
        }

        if (_skinConditionSprite.name.Contains("Frente"))
        {
            afflictedArea = "frente";
        } else if (_skinConditionSprite.name.Contains("Mejillas"))
        {
            afflictedArea = "mejillas";
        } else if (_skinConditionSprite.name.Contains("Barbilla"))
        {
            afflictedArea = "barbilla";
        }

        appearance = _skinConditionSprite;
    }

    public Sprite Sprite => appearance;

    public string Type => type;

    public string AfflictedArea => afflictedArea;
    
}
