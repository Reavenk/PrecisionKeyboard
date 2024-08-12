using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamWireReference : ParamBase
{
    public string referenceGUID = string.Empty;
    public bool cannotCycle = true;

    public ParamWireReference(string name, string reference, string widgetType = "")
        : base(name, Type.WireReference, widgetType)
    { 
    }

    public override string GetStringValue()
    {
        return this.referenceGUID;
    }

    public override bool SetValueFromString(string str)
    {
        this.referenceGUID = str;
        return true;
    }

    public override string unit => "Wire Reference";

    public override bool PostLoad(Dictionary<string, LLDNBase> directory, Dictionary<string, WiringDocument> scopedDocs)
    {
        if(string.IsNullOrEmpty(this.referenceGUID) == false)
            return false;

        WiringDocument refDoc;
        scopedDocs.TryGetValue(this.referenceGUID, out refDoc);
        this.referenceGUID = refDoc.guid;

        return true;
    }

    public override bool ReferencesDocument(WiringDocument wd)
    { 
        return wd.guid == this.referenceGUID;
    }
}
