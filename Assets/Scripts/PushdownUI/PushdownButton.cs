using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PushdownButton : UnityEngine.UI.Button
{
    private Dictionary<Transform, Vector3> originalChildrenPos = null;
    public Vector3 moveOnPress = Vector3.zero;

    public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (this.interactable == false)
            return;

        this.originalChildrenPos = new Dictionary<Transform, Vector3>();
        foreach(Transform t in this.transform)
        { 
            this.originalChildrenPos.Add(t, t.localPosition);
            t.localPosition += this.moveOnPress;
        }
    }

    public override void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if(this.interactable == false)
            return;

        if(this.originalChildrenPos != null)
        {
            foreach (Transform t in this.transform)
            {
                Vector3 v;
                if (this.originalChildrenPos.TryGetValue(t, out v) == false)
                    continue;

                t.localPosition = v + this.moveOnPress;
            }
        }
    }

    public override void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if (this.originalChildrenPos == null)
            return;

        if (this.originalChildrenPos != null)
        {
            foreach (Transform t in this.transform)
            {
                Vector3 v;
                if (this.originalChildrenPos.TryGetValue(t, out v) == true)
                    t.localPosition = v;
            }

            // Unity has some weird new thing in 2019 where moving the button off
            // doesn't bring it back to a normal state,... so we need to do that ourselves
            // when OnPointerExit is called as a drag-off from a pressed button.
            if(this.enabled == true && this.interactable == true)
                this.DoStateTransition(SelectionState.Selected, true);
            else
                this.DoStateTransition(SelectionState.Disabled, true);
        }
    }

    public override void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if(this.originalChildrenPos == null)
            return;

        foreach (Transform t in this.transform)
        { 
            Vector3 v;
            if(this.originalChildrenPos.TryGetValue(t, out v) == true)
                t.localPosition = v;
        }

        this.originalChildrenPos = null;
    }
}
