using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_AddNode : Undo_Base
{
    LLDNBase lld;

    public Undo_AddNode(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        WiringDocument wiring,
        LLDNBase lld)
        : base(
            "Add Node", 
            isUndo, 
            app, 
            collection, 
            wiring)
    { 
        this.lld = lld;
    }

    public override PxPre.Undo.BaseUndo Undo()
    {
        this.app.DeleteWiringNode(
            this.collection, 
            this.wiring, 
            this.lld);

        return 
            new Undo_DeleteNode(
                !this.isUndo,
                this.app,
                this.collection,
                this.wiring,
                this.lld);
    }
}
