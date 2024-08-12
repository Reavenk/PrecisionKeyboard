using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleUIUpdater : ParamUIUpdater
{
    ParamBool paramBool;
    ParamThumbToggle toggle;

    public ToggleUIUpdater(RectTransform rt, ParamBool paramBool, ParamThumbToggle toggle)
        : base(rt)
    { 
        this.paramBool = paramBool;
        this.toggle = toggle;
    }

    public override void Update(WiringDocument owningDoc, WiringCollection collection)
    {
        this.toggle.SetValue(this.paramBool.value);
    }
}
