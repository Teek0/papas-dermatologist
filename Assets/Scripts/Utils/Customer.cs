using UnityEngine;

public class Customer
{
    // Attributes related to appearance
    private readonly Sprite body;
    private readonly Sprite hair;
    private readonly Sprite eyes;

    // Attributes related to treatment
    private Treatment treatment; // Not readonly, 'cause we might want to reduce severity

    public Customer(GameConstantsSO _constants, Sprite[] _bodyOptions, Sprite[] _hairOptions, Sprite[] _eyesOptions, Sprite[][] _skinConditionOptions)
    {
        body = _bodyOptions[Random.Range(0, _bodyOptions.Length)];
        hair = _hairOptions[Random.Range(0, _hairOptions.Length)];
        eyes = _eyesOptions[Random.Range(0, _eyesOptions.Length)];

        treatment = new Treatment(Random.Range(1, 4), _constants, _skinConditionOptions);
    }

    // For testing
    public Customer(GameConstantsSO _constants, Sprite[] _bodyOptions, Sprite[] _hairOptions, Sprite[] _eyesOptions)
    {
        body = _bodyOptions[Random.Range(0, _bodyOptions.Length)];
        hair = _hairOptions[Random.Range(0, _hairOptions.Length)];
        eyes = _eyesOptions[Random.Range(0, _eyesOptions.Length)];
    }

    public Sprite Body => body;

    public Sprite Hair => hair;

    public Sprite Eyes => eyes;

    public Treatment Treatment => treatment;
}
