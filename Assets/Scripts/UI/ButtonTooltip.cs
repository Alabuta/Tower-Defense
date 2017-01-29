using UnityEngine.EventSystems;

public sealed class ButtonTooltip : EventTrigger {

    void Start()
    {
        HideToolTip();
    }

    public override void OnPointerEnter(PointerEventData data)
    {
        ShowToolTip();
    }

    public override void OnPointerExit(PointerEventData data)
    {
        HideToolTip();
    }

    public override void OnPointerClick(PointerEventData data)
    {
        HideToolTip();
    }

    void ShowToolTip()
    {
        transform.FindChild("TooltipPanel").gameObject.SetActive(true);
    }

    void HideToolTip()
    {
        transform.FindChild("TooltipPanel").gameObject.SetActive(false);
    }
}