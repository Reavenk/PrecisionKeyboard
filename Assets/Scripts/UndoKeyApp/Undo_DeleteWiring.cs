using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_DeleteWiring : Undo_Base
{
    List<WiringIndexPair> entries = null;

    public Undo_DeleteWiring(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        List<WiringIndexPair> entries)
        : base("Delete Wiring(s)", isUndo, app, collection, null)
    { 
        this.entries = entries;
    }

    public override string GetName()
    {
        return $"Deleted {this.wiring.GetProcessedWiringName(this.collection)}";
    }


    public override PxPre.Undo.BaseUndo Undo()
    {
        this.entries.Sort((x, y)=>{ return x.idx - y.idx; });

        List<WiringDocument> docs = new List<WiringDocument>();
        foreach(WiringIndexPair wip in this.entries)
            docs.Add(wip.doc);

        this.app.InsertWirings(this.collection, this.entries.ToArray());

        return new Undo_AddWiring(
            !this.isUndo,
            this.app,
            this.collection,
            docs.ToArray());
    }
}
