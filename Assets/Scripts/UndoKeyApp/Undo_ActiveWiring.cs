using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_ActiveWiring : Undo_Base
{
    // The new document that we set to. This isn't too relevant to the undo,
    // but comes into play for the redo.
    public WiringDocument newDoc;
    
    public Undo_ActiveWiring(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        WiringDocument newDoc,
        WiringDocument oldUndo)
        : base("Set Active", isUndo, app, collection, oldUndo)
    { 
        this.newDoc = newDoc;
    }

    public override string GetName()
    {
        return $"Set {this.newDoc.GetProcessedWiringName(this.collection)} Active";
    }

    public override PxPre.Undo.BaseUndo Undo()
    { 
        this.app.SetActiveDocument(this.collection, this.wiring);

        return new Undo_ActiveWiring(
            !this.isUndo, 
            this.app, 
            this.collection, 
            this.wiring, 
            newDoc);
    }
}
