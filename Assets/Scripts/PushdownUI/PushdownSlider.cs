using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PushdownSlider : 
    UnityEngine.UI.Slider
{
    private Dictionary<Transform, Vector3> originalChildrenPos = null;
    public Vector3 moveOnPress = Vector3.zero;
    protected int pushdowns = 0;

    void ReleasePushdownPress()
    {
        if (this.originalChildrenPos == null)
            return;

        foreach (Transform t in this.targetGraphic.transform)
        {
            Vector3 v;
            if (this.originalChildrenPos.TryGetValue(t, out v) == true)
                t.localPosition = v;
        }

        this.originalChildrenPos.Clear();
    }

    void DoPushdownPress()
    {
        if (this.interactable == false)
            return;

        this.originalChildrenPos = new Dictionary<Transform, Vector3>();
        foreach (Transform t in this.targetGraphic.transform)
        {
            this.originalChildrenPos.Add(t, t.localPosition);
            t.localPosition += this.moveOnPress;
        }

    }

    protected override void OnEnable()
    {
        base.OnEnable();

        this.pushdowns = 0;
        this.ReleasePushdownPress();
    }

    //public override void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    //{
    //    base.OnPointerEnter(eventData);
    //
    //    if(this.pushdowns > 0)
    //    {
    //        if(this.enabled == true && this.interactable == true)
    //            this.DoStateTransition(SelectionState.Pressed, true);
    //        else
    //            this.DoStateTransition(SelectionState.Disabled, true);
    //    }
    //}
    //
    //public override void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    //{
    //    base.OnPointerExit(eventData);
    //
    //    if(this.pushdowns > 0)
    //    {
    //        if(this.enabled == true && this.interactable == true)
    //            this.DoStateTransition(SelectionState.Highlighted, true);
    //        else
    //            this.DoStateTransition(SelectionState.Disabled, true);
    //    }
    //}

    public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
        base.OnPointerDown(eventData);

        ++this.pushdowns;
    }

    public override void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    { 
        base.OnPointerUp(eventData);

        // 2019 has an issue where DoStateTransition isn't triggered on mouse up.
        --this.pushdowns;

        if(this.pushdowns == 0)
        {
            this.ReleasePushdownPress();
        }

    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        if (this.interactable == false)
        {
            this.ReleasePushdownPress();
            return;
        }

        switch (state)
        {
            case SelectionState.Disabled:
            case SelectionState.Selected:
            case SelectionState.Normal:
            case SelectionState.Highlighted:
                this.ReleasePushdownPress();
                break;

            case SelectionState.Pressed:
                this.ReleasePushdownPress();
                this.DoPushdownPress();
                break;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        this.ReleasePushdownPress();
    }

}
