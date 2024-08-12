using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiringRefUIUpdater : ParamUIUpdater
{
    ParamWireReference reference;
    UnityEngine.UI.Text text;


    public WiringRefUIUpdater(RectTransform rt, ParamWireReference reference, UnityEngine.UI.Text text)
        : base(rt)
    { 
        this.reference = reference;
        this.text = text;
    }

    public override void Update(WiringDocument owningDoc, WiringCollection collection)
    {
        WiringDocument wd = collection.GetDocument(reference.referenceGUID);
        if(wd != null)
            text.text = wd.GetProcessedWiringName(collection);
        else
            text.text = "--";
    }
}
