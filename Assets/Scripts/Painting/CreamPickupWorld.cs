using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CreamPickupWorld : MonoBehaviour
{
    [SerializeField] private CreamType creamType;
    [SerializeField] private CreamSelectionManager selectionManager;

    private void Awake()
    {
        if (selectionManager == null)
            selectionManager = FindFirstObjectByType<CreamSelectionManager>();
    }

    private void OnMouseDown()
    {
        if (selectionManager == null) return;
        selectionManager.SelectCream(creamType);
    }
}
