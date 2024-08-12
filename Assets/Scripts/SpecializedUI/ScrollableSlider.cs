using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollableSlider : 
    PushdownSlider,
    UnityEngine.EventSystems.IScrollHandler
{
    // Support for manipulating the slider via the mouse wheel.
    public float scrollScale = 0.05f;
    void UnityEngine.EventSystems.IScrollHandler.OnScroll(
        UnityEngine.EventSystems.PointerEventData eventData)
    {
        float newVal = 
            Mathf.Clamp01(
                this.value + eventData.scrollDelta.y * this.scrollScale);

        this.value = newVal;
    }
}
