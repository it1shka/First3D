using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchArea : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    //[HideInInspector]
    public Vector2 delta;
    //[HideInInspector]
    public bool isPressed;

    private Vector3 startPosition;

    void Start()
    {
        delta = Vector2.zero;
        startPosition = Vector3.zero;
        isPressed = false;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        isPressed = true;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            transform as RectTransform,
            e.position, 
            e.enterEventCamera,
            out startPosition
        );
    }

    public void OnDrag(PointerEventData e)
    {
        var currentPosition = Vector3.zero;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            transform as RectTransform,
            e.position,
            e.enterEventCamera,
            out currentPosition
        );

        delta = currentPosition - startPosition;
        var size = (transform as RectTransform).rect.size;
        delta /= size / 2.0f;

    }
    public void OnEndDrag(PointerEventData e)
    {
        delta = Vector2.zero;
        startPosition = Vector3.zero;
        isPressed = false;
    }

}
