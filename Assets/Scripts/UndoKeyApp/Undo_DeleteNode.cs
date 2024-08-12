using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_DeleteNode : Undo_Base
{
    LLDNBase lld;

    public Undo_DeleteNode(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        WiringDocument wiring,
        LLDNBase lld)
        : base(
            "Remove Node",
            isUndo,
            app,
            collection,
            wiring)
    { 
        this.lld = lld;
    }

    public override PxPre.Undo.BaseUndo Undo()
    {
        this.app.AddWiringNode(
            this.collection, 
            this.wiring, 
            this.lld);

        return 
            new Undo_AddNode(
                !this.isUndo, 
                this.app, 
                this.collection, 
                this.wiring, 
                this.lld);
    }
}
