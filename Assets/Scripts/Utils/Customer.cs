using UnityEngine;

public class Customer
{
    // Attributes related to appearance
    private readonly Sprite body;
    private readonly Sprite mouth;
    private readonly Sprite eyes;

    // Attributes related to treatment
    private Treatment treatment; // Not readonly, 'cause we might want to reduce severity

    public Customer(GameConstantsSO _constants, Sprite[] _bodyOptions, Sprite[] _mouthOptions, Sprite[] _eyesOptions, Sprite[] _skinConditionOptions)
    {
        body = _bodyOptions[Random.Range(0, _bodyOptions.Length)];
        mouth = _mouthOptions[Random.Range(0, _mouthOptions.Length)];
        eyes = _eyesOptions[Random.Range(0, _eyesOptions.Length)];

        treatment = new Treatment(Random.Range(1, 3), _constants, _skinConditionOptions);
    }

    // For testing
    public Customer(GameConstantsSO _constants, Sprite[] _bodyOptions, Sprite[] _mouthOptions, Sprite[] _eyesOptions)
    {
        body = _bodyOptions[Random.Range(0, _bodyOptions.Length)];
        mouth = _mouthOptions[Random.Range(0, _mouthOptions.Length)];
        eyes = _eyesOptions[Random.Range(0, _eyesOptions.Length)];
    }

    public Sprite GetBody()
    {
        return body;
    }

    public Sprite GetMouth()
    {
        return mouth;
    }

    public Sprite GetEyes()
    {
        return eyes;
    }

    public Treatment GetTreatment()
    {
        return treatment;
    }
}
