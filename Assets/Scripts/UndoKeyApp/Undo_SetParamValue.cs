using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_SetParamValue : Undo_Base
{
    LLDNBase owner;
    ParamBase parameter;
    string origStringValue;

    public Undo_SetParamValue(
        bool isUndo, 
        IKeyboardApp app, 
        WiringCollection collection, 
        WiringDocument doc, 
        LLDNBase owner,
        ParamBase parameter,
        string origStringValue)
        : base("Set Param Value", isUndo, app, collection, doc)
    { 
        this.owner = owner;
        this.parameter = parameter;
        this.origStringValue = origStringValue;
    }

    public override string GetName()
    {
        return $"Editing {parameter.name}";
    }

    public override PxPre.Undo.BaseUndo Undo()
    { 
        string newOStrVal = parameter.GetStringValue();

        this.app.SetLLDAWParamValue(
            this.collection,
            this.wiring,
            this.owner,
            this.parameter, 
            this.origStringValue);

        return 
            new Undo_SetParamValue(
                !this.isUndo, 
                this.app, 
                this.collection, 
                this.wiring, 
                owner, 
                parameter, 
                newOStrVal);
    }
}
