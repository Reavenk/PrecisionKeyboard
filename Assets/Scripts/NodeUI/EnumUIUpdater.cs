using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumUIUpdater : ParamUIUpdater
{
    UnityEngine.UI.Text text;
    ParamEnum paramEnum;

    public EnumUIUpdater(RectTransform rt, UnityEngine.UI.Text text, ParamEnum paramEnum)
        : base(rt)
    { 
        this.text = text;
        this.paramEnum = paramEnum;
    }

    public override void Update(WiringDocument owningDoc, WiringCollection collection)
    {
        this.text.text = paramEnum.GetLabel();
    }
}
