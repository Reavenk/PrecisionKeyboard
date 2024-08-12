using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_ConnectAudio : Undo_Base
{
    LLDNBase oldOutput;
    LLDNBase input;
    ParamConnection parameter;

    public Undo_ConnectAudio(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        WiringDocument doc,
        LLDNBase output,
        LLDNBase input,
        ParamConnection parameter)
        : base("Reconnect Audio", isUndo, app, collection, doc)
    { 
        this.oldOutput = output;
        this.input = input;
        this.parameter = parameter;
    }

    public override string GetName()
    {
        if(oldOutput == null)
            return $"Disconnecting {parameter.name}";
        else
            return $"Connecting {parameter.name}";
    }

    public override PxPre.Undo.BaseUndo Undo()
    { 
        LLDNBase old = parameter.Reference;

        this.app.RewireNodeAudio( 
            this.collection, 
            this.wiring, 
            this.oldOutput, 
            this.input, 
            this.parameter);

        return new Undo_ConnectAudio(
            !this.isUndo,
            this.app,
            this.collection,
            this.wiring,
            old,
            this.input,
            this.parameter);
    }
}
