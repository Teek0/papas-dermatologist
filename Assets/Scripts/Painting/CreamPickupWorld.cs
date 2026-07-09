using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CreamPickupWorld : MonoBehaviour
{
    [SerializeField] private CreamType creamType;
    [SerializeField] private CreamSelectionManager selectionManager;
    [SerializeField] private Camera cam;

    private Collider2D pickupCollider;

    private void Reset()
    {
        cam = Camera.main;
    }

    private void Awake()
    {
        if (selectionManager == null)
            selectionManager = FindFirstObjectByType<CreamSelectionManager>();

        if (cam == null)
            cam = Camera.main;

        pickupCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (selectionManager == null) return;
        if (pickupCollider == null || cam == null) return;
        if (!GameplayPointerInput.WasPressedThisFrame()) return;

        Vector2 worldPos = cam.ScreenToWorldPoint(GameplayPointerInput.ScreenPosition);
        if (!pickupCollider.OverlapPoint(worldPos)) return;

        selectionManager.SelectCream(creamType);
    }
}
