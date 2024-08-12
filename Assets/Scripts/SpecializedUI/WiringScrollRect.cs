// <copyright file="WiringScrollRect.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>
// The derived ScrollRect used to implement the scrollable content
// for the Wiring being shown. Its main feature is to support multi-touch gestures.
//
// This class may need to be generalized later to be for multi-touch scrollable "content"
// instead of specifically for wiring content.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WiringScrollRect : 
    UnityEngine.UI.MyScrollRect,
    UnityEngine.EventSystems.IPointerDownHandler,
    UnityEngine.EventSystems.IPointerUpHandler

{
    public PaneWiring controller;
    public MovementType originalMovementType;


    public class TouchRecord
    {
        public UnityEngine.EventSystems.PointerEventData.InputButton buttonType;
        public int pointerID;

        public Vector3 cachedWorld;
        public Vector3 cachedLocal;

        public TouchRecord(UnityEngine.EventSystems.PointerEventData.InputButton buttonType, int pointerID, Vector3 originalWorld, Vector3 originalLocal)
        { 
            this.buttonType = buttonType;
            this.pointerID = pointerID;

            this.cachedWorld = originalWorld;
            this.cachedLocal = originalLocal;
        }
    }

    public List<TouchRecord> touchRecords = 
        new List<TouchRecord>();

    public List<PointerEventData> dirtyPointerEvents = 
        new List<PointerEventData>();

    protected override void Awake()
    { 
        base.Awake();

        this.dirtyPointerEvents.Clear();
        this.originalMovementType = this.movementType;
    }

    bool RemoveTouchRecord(UnityEngine.EventSystems.PointerEventData.InputButton buttonType, int pointerID)
    { 
        for(int i = 0; i < this.touchRecords.Count; ++i)
        { 
            if(this.touchRecords[i].pointerID == pointerID && this.touchRecords[i].buttonType == buttonType)
            { 
                this.touchRecords.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool HasTouchRecord(UnityEngine.EventSystems.PointerEventData.InputButton buttonType, int pointerID)
    {
        for (int i = 0; i < this.touchRecords.Count; ++i)
        {
            if (this.touchRecords[i].pointerID == pointerID && this.touchRecords[i].buttonType == buttonType)
                return true;
        }
        return false;
    }

    public override void OnScroll(UnityEngine.EventSystems.PointerEventData data)
    {
        const float scrollDampen = 10.0f;
        float scrollAmt = data.scrollDelta.y / scrollDampen;

        this.controller.wiringZoom =
            Mathf.Clamp(
                this.controller.wiringZoom + scrollAmt,
                PaneWiring.minZoom,
                PaneWiring.maxZoom);

        Vector3 v3Glob = data.position;
        Vector3 v3Loc =
            this.content.worldToLocalMatrix.MultiplyPoint(v3Glob);

        this.content.localScale =
            new Vector3(
                this.controller.wiringZoom,
                this.controller.wiringZoom, 1.0f);

        Vector3 v3GlP = this.content.localToWorldMatrix.MultiplyPoint(v3Loc);

        Vector3 endpos = this.content.anchoredPosition3D + v3Glob - v3GlP;
        endpos.x = Mathf.Min(endpos.x, 0.0f);
        endpos.y = Mathf.Max(endpos.y, 0.0f);
        endpos.z = 0.0f;

        Vector2 contentSz = this.content.rect.size;
        contentSz.x *= this.content.localScale.x;
        contentSz.y *= this.content.localScale.y;

        Vector3 viewportSz = this.viewport.rect.size;

        if(contentSz.x < viewportSz.x)
            endpos.x = 0.0f;

        if (contentSz.y < viewportSz.y)
            endpos.y = 0.0f;

        //if(contentSz.x + endpos.x > viewportSz.x)
        //    endpos.x -= contentSz.x + endpos.x - viewportSz.x;
        //
        //if (contentSz.y + endpos.y > viewportSz.y)
        //    endpos.y -= contentSz.y + endpos.y - viewportSz.y;

        this.content.anchoredPosition3D = endpos;
        this.velocity = Vector2.zero;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        this.dirtyPointerEvents.Add(eventData);

        if(this.dirtyPointerEvents.Count == 1)
            this.StartCoroutine(this.ProcessDirtyPointerEvents());
    }

    IEnumerator ProcessDirtyPointerEvents()
    { 
        yield return new WaitForEndOfFrame();

        //this.velocity = Vector2.zero;

        int touchRecordsCt = this.touchRecords.Count;
        if(touchRecordsCt == 0 || this.dirtyPointerEvents.Count == 0)
        {
            this.dirtyPointerEvents.Clear();
            Debug.LogError("ProcessDirtyPointerEvents() encountered with nothing to process.");
            yield break;
        }

        // Get some stats on how things were before
        // up integrate the new input changes.

        Vector3 oldAvgWorld = Vector3.zero;
        Vector3 oldAvgLocal = Vector3.zero;
        foreach(TouchRecord tr in this.touchRecords)
        { 
            oldAvgWorld += tr.cachedWorld;
            oldAvgLocal += tr.cachedLocal;
        }
        oldAvgWorld /= touchRecordsCt;
        oldAvgLocal /= touchRecordsCt;

        float oldScaleFac = 0.0f;

        if(touchRecordsCt >= 2)
        { 
            foreach(TouchRecord tr in this.touchRecords)
                oldScaleFac += (tr.cachedWorld - oldAvgWorld).magnitude;

            oldScaleFac /= touchRecordsCt;

            this.movementType = MovementType.Unrestricted;
        }

        // Update the inputs
        foreach(PointerEventData ped in this.dirtyPointerEvents)
        {
            for(int i = 0; i < this.touchRecords.Count; ++i)
            { 
                if(this.touchRecords[i].buttonType != ped.button ||
                    this.touchRecords[i].pointerID != ped.pointerId)
                { 
                    continue;
                }

                Vector2 wp = ped.position;
                this.touchRecords[i].cachedWorld = wp;
                break;
            }
        }

        // Get the same stats as before but with the new information
        Vector3 newAvgWorld = Vector3.zero;
        
        foreach(TouchRecord tr in this.touchRecords)
            newAvgWorld += tr.cachedWorld;

        newAvgWorld /= touchRecordsCt;

        //  Do the scaling if needed
        //
        //////////////////////////////////////////////////

        float newScaleFac = 0.0f;
        if(touchRecordsCt >= 2)
        { 
            foreach(TouchRecord tr in this.touchRecords)
                newScaleFac += (tr.cachedWorld - newAvgWorld).magnitude;

            newScaleFac /= touchRecordsCt;
        }

        if(touchRecordsCt >= 2)
        {
            Vector3 oldLocalPos = this.content.localPosition;

            float ratio = newScaleFac / oldScaleFac;
            float newZoom = this.controller.wiringZoom * ratio;
            newZoom = Mathf.Clamp( newZoom, PaneWiring.minZoom, PaneWiring.maxZoom);
        
            this.content.localScale = new Vector3(newZoom, newZoom, 1.0f);
            this.controller.wiringZoom = newZoom;

            this.content.localPosition = oldLocalPos;
        }

        // Do the translation
        //
        //////////////////////////////////////////////////

        // Calculate the local positions. We don't do this until after the scale.
        foreach(TouchRecord tr in this.touchRecords)
            tr.cachedLocal = this.content.worldToLocalMatrix.MultiplyPoint(tr.cachedWorld);
        
        Vector3 newAvgLocal = Vector3.zero;
        
        foreach(TouchRecord tr in this.touchRecords)
            newAvgLocal += tr.cachedLocal;
        
        newAvgLocal /= touchRecordsCt;
        
        // Finally doing the translation
        Vector3 transOffset = newAvgLocal - oldAvgLocal;
        transOffset = this.content.localToWorldMatrix.MultiplyVector(transOffset); // Apply UI scale
        transOffset = this.viewport.worldToLocalMatrix.MultiplyVector(transOffset);
        this.content.localPosition += transOffset;

        // One moar time! Since we moved the content, if we want the cached local position
        // to be up to date, it needs to be updated after the content move.
        //////////////////////////////////////////////////
        ///
        foreach(TouchRecord tr in this.touchRecords)
            tr.cachedLocal = this.content.worldToLocalMatrix.MultiplyPoint(tr.cachedWorld);

        //if(this.touchRecords.Count == 1)
        //{
        //    
        //    this.touchRecords[0].cachedWorld = eventData.position;
        //    Vector3 target = this.content.localToWorldMatrix.MultiplyPoint(this.touchRecords[0].cachedLocal);
        //
        //    Vector3 v3 = eventData.position;
        //    this.content.anchoredPosition3D += v3 - target;
        //}
        //else
        //{
        //    // Get the average of the original and local, 
        //    // as well as the average radius
        //    //////////////////////////////////////////////////
        //    Vector3 origWorld   = Vector3.zero;
        //    foreach (TouchRecord tr in this.touchRecords)
        //        origWorld += tr.cachedWorld;
        //
        //    origWorld /= this.touchRecords.Count;
        //    float origWorldRad = 0.0f;
        //    foreach (TouchRecord tr in this.touchRecords)
        //        origWorldRad += (tr.cachedWorld - origWorld).magnitude;
        //    //
        //    origWorldRad /= this.touchRecords.Count;
        //
        //
        //    // Update the correct single entry.
        //    //////////////////////////////////////////////////
        //    for (int i = 0; i < this.touchRecords.Count; ++i)
        //    {
        //        if(this.touchRecords[i].pointerID == eventData.pointerId)
        //        { 
        //            this.touchRecords[i].cachedWorld = eventData.position;
        //            //this.touchRecords[i].cachedLocal = this.content.worldToLocalMatrix.MultiplyPoint(eventData.position);
        //            break;
        //        }
        //    }
        //
        //    // Get the average of the updated and local, 
        //    // as well as the average radius
        //    //////////////////////////////////////////////////
        //    Vector3 recWorld    = Vector3.zero;
        //    Vector3 recLocal    = Vector3.zero;
        //    foreach (TouchRecord tr in this.touchRecords)
        //    {
        //        recWorld += tr.cachedWorld;
        //        recLocal += tr.cachedLocal;
        //    }
        //    recWorld /= this.touchRecords.Count;
        //    recLocal /= this.touchRecords.Count;
        //    float recWorldRad = 0.0f;
        //    foreach(TouchRecord tr in this.touchRecords)
        //        recWorldRad += (tr.cachedWorld - recWorld).magnitude;
        //    //
        //    recWorldRad /= this.touchRecords.Count;
        //
        //    // Figure out the update & Execute
        //    //////////////////////////////////////////////////
        //    float ratio = recWorldRad / origWorldRad;
        //    float newZoom = this.controller.wiringZoom * ratio;
        //    newZoom = Mathf.Clamp( newZoom, PaneWiring.minZoom, PaneWiring.maxZoom);
        //
        //    this.content.localScale = new Vector3(newZoom, newZoom, 1.0f);
        //    this.controller.wiringZoom = newZoom;
        //
        //    Vector3 newScaleGlobal = this.content.localToWorldMatrix.MultiplyPoint(recLocal);
        //    this.content.anchoredPosition3D += origWorld - newScaleGlobal;
        //    this.content.anchoredPosition3D += origWorld - recWorld;
        //}
        //
        //for(int i = 0; i < this.touchRecords.Count; ++i)
        //{ 
        //    this.touchRecords[i].cachedLocal = this.touchRecords[i].cachedNewLocal;
        //    this.touchRecords[i].cachedWorld = this.touchRecords[i].cachedNewWorld;
        //}

        //Vector3 origEndpos = this.content.anchoredPosition3D;
        //Vector3 endpos = this.content.anchoredPosition3D;
        ////endpos.x = Mathf.Min(endpos.x, 0.0f);
        ////endpos.y = Mathf.Max(endpos.y, 0.0f);
        ////endpos.z = 0.0f;
        //
        //Vector2 contentSz = this.content.rect.size;
        //contentSz.x *= this.content.localScale.x;
        //contentSz.y *= this.content.localScale.y;
        //
        //Vector3 viewportSz = this.viewport.rect.size;
        //
        //if (contentSz.x < viewportSz.x)
        //    endpos.x = 0.0f;
        //
        //if (contentSz.y < viewportSz.y)
        //    endpos.y = 0.0f;
        //
        ////if(contentSz.x + endpos.x > viewportSz.x)
        ////    endpos.x -= contentSz.x + endpos.x - viewportSz.x;
        ////
        ////if (contentSz.y + endpos.y > viewportSz.y)
        ////    endpos.y -= contentSz.y + endpos.y - viewportSz.y;
        //
        //this.content.anchoredPosition3D = (endpos + origEndpos) * 0.5f;
        this.dirtyPointerEvents.Clear();
        //this.velocity = Vector2.zero;
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        base.OnInitializePotentialDrag(eventData);
    }

    void UnityEngine.EventSystems.IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    { 
        eventData.useDragThreshold = false;

        this.RemoveTouchRecord(eventData.button, eventData.pointerId);

        Vector3 loc = this.content.worldToLocalMatrix.MultiplyPoint(eventData.position);

        TouchRecord tr = 
            new TouchRecord(
                eventData.button,
                eventData.pointerId, 
                eventData.position, 
                loc);

        this.touchRecords.Add(tr);

        if(this.touchRecords.Count > 1)
            this.movementType = MovementType.Unrestricted;
    }

    void UnityEngine.EventSystems.IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        this.RemoveTouchRecord(eventData.button, eventData.pointerId);
        if(this.touchRecords.Count == 0)
            this.movementType = this.originalMovementType;

        //if (this.touchRecords.Count == 1 && eventData.dragging == true)
        //{
        //    eventData.pointerId = this.touchRecords[0].pointerID;
        //    base.OnBeginDrag(eventData);
        //}
    }

    protected override void OnEnable()
    {
        this.touchRecords.Clear();
        this.dirtyPointerEvents.Clear();
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        this.touchRecords.Clear();
        this.dirtyPointerEvents.Clear();
        base.OnDisable();
    }
}
