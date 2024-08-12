using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamThumbWRef : ParamThumbBase
{
    public UnityEngine.UI.Image icon;

    protected ParamWireReference reference;

    public override bool UpdateDisplayValue()
    {
        if(this.reference == null)
            return false;

        if(
            string.IsNullOrEmpty(this.reference.referenceGUID) == true ||
            this.Collection.HasWiringGUID(this.reference.referenceGUID) == false)
        { 
            icon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
        else
            icon.color = Color.white;

        return true;
    }
}
