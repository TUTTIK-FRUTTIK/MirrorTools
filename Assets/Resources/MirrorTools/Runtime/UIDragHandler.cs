using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RectTransform dragRectTransform;
    public Canvas canvas;
    public bool canBeMoved;
    public Vector2 lastAnchoredPosition;
    
    private Vector2 offset;
    private bool canDrag;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        canDrag = results.Count == 1;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragRectTransform.parent as RectTransform, 
                eventData.position, 
                canvas?.worldCamera, 
                out Vector2 localCursor))
        {
            offset = dragRectTransform.anchoredPosition - localCursor;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !canDrag || !canBeMoved)
            return;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragRectTransform.parent as RectTransform, 
                eventData.position, 
                canvas?.worldCamera, 
                out Vector2 localCursor))
        {
            dragRectTransform.anchoredPosition = localCursor + offset;
            lastAnchoredPosition = dragRectTransform.anchoredPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canBeMoved) return;
        
        PlayerPrefs.SetFloat("WindowPositionX", dragRectTransform.anchoredPosition.x);
        PlayerPrefs.SetFloat("WindowPositionY", dragRectTransform.anchoredPosition.y);
        PlayerPrefs.Save();
    }
}