// <copyright file="MultitouchScrollbar.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>
// A scrollbar dervied from the Unity Scrollbar 
// that supports multitouch.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MultitouchScrollbar : UnityEngine.UI.MyScrollbar
{
    public struct TouchID
    { 
        public int touchID;
        public UnityEngine.EventSystems.PointerEventData.InputButton button;
        public Vector2 lastScrollAnchorPos;
    }

    [System.Serializable]
    public class ScrollScaleEvent : UnityEngine.Events.UnityEvent<float>
    { }

    // If the keyboard was multitouch, we have to remember that 
    // until they fully disengage all touches.
    bool wasMultitouch = false;

    List<TouchID> touches = null;

    public ScrollScaleEvent onScale = new ScrollScaleEvent();

    public bool pushdown = false;
    private Dictionary<Transform, Vector3> originalChildrenPos = null;
    public Vector3 moveOnPress = Vector3.zero;

    public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if(this.AnyTouches() == false)
            base.OnPointerDown(eventData);

        eventData.useDragThreshold = false;
        this.AddTouch(eventData);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        if(pushdown == true)
        {
            if(this.interactable == false)
            { 
                this.ReleasePushdownPress();
                return;
            }

            switch (state)
            { 
                case SelectionState.Normal:
                    this.ReleasePushdownPress();
                    break;

                case SelectionState.Highlighted:
                    this.ReleasePushdownPress();
                    break;

                case SelectionState.Pressed:
                    this.ReleasePushdownPress();
                    this.DoPushdownPress();
                    break;

                case SelectionState.Disabled:
                    this.ReleasePushdownPress();
                    break;
            }
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if(this.originalChildrenPos != null && this.originalChildrenPos.Count != 0)
        { 
            if(this.enabled == true && this.interactable == true)
                this.DoStateTransition(SelectionState.Pressed, true);
            else
                this.DoStateTransition(SelectionState.Disabled, true);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if(this.originalChildrenPos != null && this.originalChildrenPos.Count != 0)
        { 
            if(this.enabled == true && this.interactable == true)
                this.DoStateTransition(SelectionState.Highlighted, true);
            else
                this.DoStateTransition(SelectionState.Disabled, true);
        }
    }

    void ReleasePushdownPress()
    { 
        if(this.originalChildrenPos == null)
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
        if(this.interactable == false)
            return;

        this.originalChildrenPos = new Dictionary<Transform, Vector3>();
        foreach (Transform t in this.targetGraphic.transform)
        {
            this.originalChildrenPos.Add(t, t.localPosition);
            t.localPosition += this.moveOnPress;
        }

    }

    bool RemoveButtonID(int id, PointerEventData.InputButton button)
    { 
        if(this.touches == null)
            return false;

        for(int i = 0; i < this.touches.Count; ++i)
        { 
            if(this.touches[i].touchID == id && this.touches[i].button == button)
            { 
                this.touches.RemoveAt(i);

                if(this.touches.Count == 0)
                    this.ClearMultitouch();

                return true;
            }
        }
        return false;
    }

    bool AddTouch(UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.RemoveButtonID(eventData.pointerId, eventData.button);

        Vector3 localTouch = Vector3.zero;
        Vector3[] handleCorners = new Vector3[4];

        //localTouch = 
        //    this.handleRect.worldToLocalMatrix.MultiplyPoint(eventData.position);
        //
        // Check if the touch was in the scroll thumb
        //this.handleRect.GetLocalCorners(handleCorners);
        //
        //if (localTouch.x < handleCorners[0].x ||
        //    localTouch.x > handleCorners[2].x ||
        //    localTouch.y < handleCorners[0].y ||
        //    localTouch.y > handleCorners[2].y)
        //{ 
        //    return false;    
        //}

        // If it was in the touch we proceed.
        RectTransform rtThis = this.gameObject.GetComponent<RectTransform>();
        rtThis.GetLocalCorners(handleCorners);

        TouchID tid = new TouchID();
        tid.touchID = eventData.pointerId;
        tid.button = eventData.button;
        tid.lastScrollAnchorPos = 
            rtThis.worldToLocalMatrix.MultiplyPoint(eventData.position);

        if(this.touches == null)
            this.touches = new List<TouchID>();

        this.touches.Add(tid);

        if(this.touches.Count >= 2)
            this.wasMultitouch = true;

        return true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
            return;

        this.RemoveButtonID(eventData.pointerId, eventData.button);

        base.OnPointerUp(eventData);

        if(this.touches == null || this.touches.Count == 0)
        { 
            if(this.interactable == true && this.enabled == true)
                this.DoStateTransition(SelectionState.Normal, false);
            else
                this.DoStateTransition(SelectionState.Disabled, false);
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if(touches.Count <= 1)
            base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (this.AnyTouches() == true && this.wasMultitouch == false)
        {
            //base.OnDrag(eventData);
            //return;
        }

        RectTransform rtThis = 
            this.gameObject.GetComponent<RectTransform>();

        // Get the old average position
        Vector2 origAvg = Vector2.zero;
        foreach(TouchID touch in this.touches)
            origAvg += touch.lastScrollAnchorPos;
        
        if(this.touches.Count >= 1)
            origAvg /= this.touches.Count;


        // Get the average scale
        float origDst = 0.0f;
        if(this.touches.Count > 1)
        {
            foreach(TouchID touch in this.touches)
                origDst += (touch.lastScrollAnchorPos - origAvg).magnitude;

            origDst /= this.touches.Count;
        }
        else
            origDst = 1.0f;

        // Apply the new input
        for (int i = 0; i < this.touches.Count; ++i)
        { 
            if(this.touches[i].touchID == eventData.pointerId && this.touches[i].button == eventData.button)
            { 
                TouchID tid = this.touches[i];
                tid.lastScrollAnchorPos =
                    rtThis.worldToLocalMatrix.MultiplyPoint(eventData.position);

                this.touches[i] = tid;
                break;
            }
        }

        // Get the updated average
        Vector2 newAvg = Vector2.zero;
        foreach(TouchID touch in this.touches)
            newAvg += touch.lastScrollAnchorPos;

        if(this.touches.Count >= 1)
            newAvg /= this.touches.Count;

        // Get the updated scale;
        float newDst = 0.0f;
        if(this.touches.Count > 1)
        {
            foreach(TouchID touch in this.touches)
                newDst += (touch.lastScrollAnchorPos - newAvg).magnitude;

            newDst /= this.touches.Count;
        }
        else
            newDst = 1.0f; 

        // Figure out the scale;
        if(newDst != 0.0f && origDst != 0.0f)
        {
            float scaleDiff = (newDst / origDst) / (origDst/newDst);
            if(scaleDiff != 1.0f)
                onScale?.Invoke(scaleDiff);
        }

        // Figure out the offset;
        Vector3 [] vpCorners = new Vector3[4];
        Vector3 [] thumbCorners = new Vector3[4];
        rtThis.GetWorldCorners(vpCorners);
        this.handleRect.GetWorldCorners(thumbCorners);
        float vpW = vpCorners[2].x - vpCorners[0].x;
        float thumbW = thumbCorners[2].x - thumbCorners[0].x;

        if (thumbW >= vpW || this.size == 1.0f)
            return;

        float scrollArea = vpW - thumbW;

        float valueDiff = (newAvg.x - origAvg.x)/scrollArea;

        valueDiff = Mathf.Clamp(valueDiff, -1.0f, 1.0f);

        float newVal = this.value + valueDiff * this.transform.lossyScale.x;
        this.value = Mathf.Clamp01(newVal);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        this.ClearMultitouch();
        this.ReleasePushdownPress();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        this.ClearMultitouch();
    }

    void ClearMultitouch()
    { 
        this.touches = null;
        this.wasMultitouch = false;
    }

    bool AnyTouches()
    { 
        return this.touches != null && this.touches.Count > 0;
    }
}
