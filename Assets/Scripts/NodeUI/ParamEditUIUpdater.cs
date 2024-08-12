using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamEditUIUpdater : ParamUIUpdater
{
    ParamThumbBase thumb;

    public ParamEditUIUpdater(RectTransform rt, ParamThumbBase thumb)
        : base(rt)
    { 
        this.thumb = thumb;
    }

    public override void Update(WiringDocument owningDoc, WiringCollection collection)
    {
        this.thumb.UpdateDisplayValue();
    }
}
