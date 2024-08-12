using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerQuick
{
    public UnityEngine.EventSystems.EventTrigger trigger;

    public EventTriggerQuick(UnityEngine.EventSystems.EventTrigger et)
    { 
        this.trigger = et;
    }

    public void AddOnBeginDrag(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.BeginDrag);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnCancel(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Cancel);

        e.callback.AddListener( (x)=>{ cb(x); });
    }

    public void AddOnDeselect(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Deselect);

        e.callback.AddListener( (x)=>{ cb(x); });
    }

    public void AddOnDrag(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Drag);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnDrop(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Drop);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnEndDrag(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.EndDrag);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnInitializePotentialDrag(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Drag);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnMove(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.AxisEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Move);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.AxisEventData)x); });
    }

    public void AddOnPointerClick(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.PointerClick);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnPointerDown(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.PointerDown);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnPointerEnter(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.PointerEnter);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnPointerExit(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.PointerExit);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnPointerUp(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.PointerUp);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnScroll(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Scroll);

        e.callback.AddListener( (x)=>{ cb((UnityEngine.EventSystems.PointerEventData)x); });
    }

    public void AddOnSelect(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Select);

        e.callback.AddListener( (x)=>{ cb(x); });
    }

    public void AddOnSubmit(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> cb)
    {
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Submit);

        e.callback.AddListener( (x)=>{ cb(x); });
    }

    public void AddOnUpdateSelected(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> cb)
    { 
        UnityEngine.EventSystems.EventTrigger.Entry e = 
            this.trigger.GetEntry(UnityEngine.EventSystems.EventTriggerType.Select);

        e.callback.AddListener( (x)=>{ cb(x); });
    }
}

public static class EventTriggerQuickUtil
{ 
    public static EventTriggerQuick ETQ(this GameObject go)
    {
        UnityEngine.EventSystems.EventTrigger et = go.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if(et == null)
            et = go.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        return new EventTriggerQuick(et);
    }

     public static UnityEngine.EventSystems.EventTrigger.Entry GetEntry(
         this UnityEngine.EventSystems.EventTrigger et,
         UnityEngine.EventSystems.EventTriggerType ty)
    { 
        if(et.triggers == null)
            et.triggers = new List<UnityEngine.EventSystems.EventTrigger.Entry>();

        foreach(UnityEngine.EventSystems.EventTrigger.Entry e in et.triggers)
        {
            if(e.eventID == ty)
                return e;
        }

        UnityEngine.EventSystems.EventTrigger.Entry ret = new UnityEngine.EventSystems.EventTrigger.Entry();
        ret.eventID = ty;

        et.triggers.Add(ret);
        return ret;
    }
}
