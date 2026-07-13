using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIDragHandle : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private bool keepInsideParent = true;

    private RectTransform parentRect;
    private Vector2 pointerOffset;

    private void Awake()
    {
        EnsureRaycastTarget();
    }

    public void Configure(RectTransform targetToMove)
    {
        target = targetToMove;
        EnsureRaycastTarget();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CachePointerOffset(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (target != null)
            target.SetAsLastSibling();

        CachePointerOffset(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!ResolveTarget() || !TryGetPointerLocalPosition(eventData, out Vector2 pointerPosition))
            return;

        Vector2 newPosition = pointerPosition + pointerOffset;
        target.anchoredPosition = keepInsideParent ? ClampToParent(newPosition) : newPosition;
    }

    private void CachePointerOffset(PointerEventData eventData)
    {
        if (!ResolveTarget() || !TryGetPointerLocalPosition(eventData, out Vector2 pointerPosition))
            return;

        pointerOffset = target.anchoredPosition - pointerPosition;
    }

    private bool ResolveTarget()
    {
        if (target == null)
            target = transform.parent as RectTransform;

        if (target == null)
            return false;

        parentRect = target.parent as RectTransform;
        return parentRect != null;
    }

    private bool TryGetPointerLocalPosition(PointerEventData eventData, out Vector2 pointerPosition)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out pointerPosition);
    }

    private Vector2 ClampToParent(Vector2 position)
    {
        Rect parent = parentRect.rect;
        Rect rect = target.rect;
        Vector2 pivot = target.pivot;

        float minX = parent.xMin + rect.width * pivot.x;
        float maxX = parent.xMax - rect.width * (1f - pivot.x);
        float minY = parent.yMin + rect.height * pivot.y;
        float maxY = parent.yMax - rect.height * (1f - pivot.y);

        if (minX <= maxX)
            position.x = Mathf.Clamp(position.x, minX, maxX);

        if (minY <= maxY)
            position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private void EnsureRaycastTarget()
    {
        Graphic graphic = GetComponent<Graphic>();

        if (graphic == null)
        {
            Image image = gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
            return;
        }

        graphic.raycastTarget = true;
    }
}
