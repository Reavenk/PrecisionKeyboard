using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardScrollbar : 
    MultitouchScrollbar,
    UnityEngine.EventSystems.IScrollHandler
{
    public PaneKeyboard keyboard;

    void UnityEngine.EventSystems.IScrollHandler.OnScroll(
        UnityEngine.EventSystems.PointerEventData eventData)
    { 
        // This is null if it's the parameter edit keyboard
        // instead of the main keyboard.
        if(this.keyboard == null)
            return;

        const float wheelScale = 0.05f;

        float per = this.keyboard.GetKeyWidthPercent();
        per = Mathf.Clamp(per + eventData.scrollDelta.y * wheelScale, 0.0f, 1.0f);

        this.keyboard.SetKeyWidthPercent(per);
    }
}
