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
        } else if (_skinConditionSprite.name.Contains("Wrinkles"))
        {
            type = "arrugas";
        } else if (_skinConditionSprite.name.Contains("Scar"))
        {
            type = "cicatrices";
        } else
        {
            throw new Exception("Invalid skin condition sprite name");
        }

        if (_skinConditionSprite.name.Contains("forehead"))
        {
            afflictedArea = "frente";
        } else if (_skinConditionSprite.name.Contains("cheeks"))
        {
            afflictedArea = "cara";
        } else if (_skinConditionSprite.name.Contains("chin"))
        {
            afflictedArea = "mentón";
        }

        appearance = _skinConditionSprite;
    }

    public Sprite Sprite => appearance;

    public string Type => type;

    public string AfflictedArea => afflictedArea;
    
}
