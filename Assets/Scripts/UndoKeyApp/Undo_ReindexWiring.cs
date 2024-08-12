using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_ReindexWiring : Undo_Base
{
    int newIndex = -1;

    public Undo_ReindexWiring(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        WiringDocument movedDoc,
        int newIndex)
        : base(
            "Reindex Wiring", 
            isUndo, 
            app, 
            collection, 
            movedDoc)
    { 
        this.newIndex = newIndex;
    }

    public override PxPre.Undo.BaseUndo Undo()
    { 
        int oldIndx = 
            this.collection.GetWiringIndex(this.wiring);

        this.app.SetWiringIndex(
            this.collection, 
            this.wiring, 
            this.newIndex);

        return new Undo_ReindexWiring(
            !isUndo,
            this.app,
            this.collection,
            this.wiring,
            oldIndx);
    }
}
