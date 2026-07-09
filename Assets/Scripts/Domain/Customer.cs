using UnityEngine;

public class Customer
{
    private readonly Sprite body;
    private readonly Sprite hair;
    private readonly Sprite eyes;

    private Treatment treatment;

    public Customer(
        GameConstantsSO _constants,
        Sprite[] _bodyOptions,
        Sprite[] _hairOptions,
        Sprite[] _eyesOptions,
        Sprite[][] _skinConditionOptions
    )
    {
        body = PickOrNull(_bodyOptions, "BodyOptions");
        hair = PickOrNull(_hairOptions, "HairOptions");
        eyes = PickOrNull(_eyesOptions, "EyesOptions");

        if (_constants == null)
        {
            Debug.LogError("Customer: GameConstantsSO es null. Treatment no se puede generar.");
            treatment = null;
            return;
        }

        treatment = new Treatment(Random.Range(1, 4), _constants, _skinConditionOptions);
    }

    private static Sprite PickOrNull(Sprite[] options, string label)
    {
        if (options == null || options.Length == 0)
        {
            Debug.LogError($"Customer: '{label}' está vacío o null en el inspector. Se usará sprite null.");
            return null;
        }

        return options[Random.Range(0, options.Length)];
    }

    public Sprite Body => body;
    public Sprite Hair => hair;
    public Sprite Eyes => eyes;
    public Treatment Treatment => treatment;
}

