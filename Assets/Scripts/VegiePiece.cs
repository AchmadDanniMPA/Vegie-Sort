using UnityEngine;
using UnityEngine.EventSystems;

public class VegiePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string vegieName;
    public Vector3 startPosition;
    public bool isMatched = false;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = transform.position;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Input.mousePosition;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        bool foundTarget = false;
        foreach (TargetSlot slot in FindObjectsOfType<TargetSlot>())
        {
            float distance = Vector2.Distance(rectTransform.position, slot.GetComponent<RectTransform>().position);
            if (distance < 50f)
            {
                foundTarget = true;
                if (slot.expectedVegie == vegieName)
                {
                    GameManager.instance.OnCorrectDrop(this, slot);
                }
                else
                {
                    GameManager.instance.OnWrongDrop(this);
                }
                break;
            }
        }
        if (!foundTarget && !isMatched)
        {
            GameManager.instance.OnWrongDrop(this);
        }
    }
}
