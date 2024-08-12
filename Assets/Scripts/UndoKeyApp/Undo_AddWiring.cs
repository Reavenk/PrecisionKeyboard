using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_AddWiring : Undo_Base
{
    WiringDocument [] docs;

    public Undo_AddWiring(
        bool isUndo, 
        IKeyboardApp app, 
        WiringCollection collection, 
        params WiringDocument [] docs)
        : base("Add Wiring", isUndo, app, collection, null)
    { 
        this.docs = docs;
    }

    public override PxPre.Undo.BaseUndo Undo()
    {
        List<WiringIndexPair> rdes = new List<WiringIndexPair>();


        foreach(WiringDocument doc in this.docs)
        { 
            int idx = this.collection.GetWiringIndex(doc);
            rdes.Add(new WiringIndexPair(doc, idx));
        }

        this.app.DeleteWirings(this.collection, this.docs);

        return new Undo_DeleteWiring(
            !this.isUndo, 
            this.app, 
            this.collection, 
            rdes);
    }
}
