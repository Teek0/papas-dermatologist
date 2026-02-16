using UnityEngine;

public class CustomerViewCamilla : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer eyes;

    [Header("Skin Conditions")]
    [SerializeField] private SpriteRenderer forehead;
    [SerializeField] private SpriteRenderer cheeks;
    [SerializeField] private SpriteRenderer chin;

    private void Start()
    {
        var customer = GameSession.I != null ? GameSession.I.CurrentCustomer : null;
        if (customer == null)
        {
            Debug.LogWarning("No hay CurrentCustomer en GameSession. ¿Entraste a CamillaScene sin aceptar uno?");
            return;
        }

        if (customer.Body != null) body.sprite = customer.Body;
        else Debug.LogError("Error al obtener 'body' desde customer.");
        if (customer.Body != null) eyes.sprite = customer.Eyes;
        else Debug.LogError("Error al obtener 'eyes' desde customer.");

        forehead.sprite = null;
        cheeks.sprite = null;
        chin.sprite = null;

        foreach (var sc in customer.Treatment.SkinConditions)
            Debug.Log($"SC: type={sc.Type} area='{sc.AfflictedArea}' sprite='{sc.Sprite.name}'");

        foreach (var sc in customer.Treatment.SkinConditions)
        {
            switch (sc.AfflictedArea)
            {
                case "frente":
                    forehead.sprite = sc.Sprite;
                    break;

                case "mejillas":
                case "mejilla":
                case "cara":
                case "cachetes":
                    cheeks.sprite = sc.Sprite;
                    break;

                case "barbilla":
                case "menton":
                case "mentón":
                    chin.sprite = sc.Sprite;
                    break;
            }
        }

        Debug.Log($"Customer en camilla. Pago: {customer.Treatment.Payment}, Tiempo: {customer.Treatment.TimeLimit}s, Afecciones: {customer.Treatment.SkinConditions.Count}");
    }
}
