using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParamUIUpdater
{
    public readonly RectTransform rectTransform;

    protected ParamUIUpdater(RectTransform rectTransform)
    { 
        this.rectTransform = rectTransform;
    }

    public abstract void Update(WiringDocument owningDoc, WiringCollection collection);
}
