using UnityEngine;

public class SkinCondition
{
    private readonly Sprite appearance;
    private readonly int severityScore;

    public SkinCondition(Sprite[] _appearanceOptions, int _severityScore)
    {
        appearance = _appearanceOptions[Random.Range(0, _appearanceOptions.Length - 1)];
        severityScore = _severityScore;
    }

    public SkinCondition(Sprite[] _appearanceOptions)
    {
        appearance = _appearanceOptions[Random.Range(0, _appearanceOptions.Length - 1)];
        severityScore = 1;
    }

    public Sprite Sprite => appearance;
    public int SeverityScore => severityScore;
}
