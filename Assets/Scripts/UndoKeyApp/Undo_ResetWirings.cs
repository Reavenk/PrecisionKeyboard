using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_ResetWirings : Undo_Base
{
    List<WiringDocument> wiringSet;

    public Undo_ResetWirings(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        IEnumerable<WiringDocument> newDocs)
        : base(
            "Reset Wirings", 
            isUndo, 
            app, 
            collection, 
            null)
    { 
        this.wiringSet = 
            new List<WiringDocument>(newDocs);
    }

    public override string GetName()
    {
        return "Reordering wirings";
    }

    public override PxPre.Undo.BaseUndo Undo()
    { 
        List<WiringDocument> lst = this.collection.GetDocumentsListCopy();

        this.app.ResetWirings(collection, this.wiringSet);

        return 
            new Undo_ResetWirings(
                !isUndo,
                this.app,
                this.collection,
                lst);
    }
}
