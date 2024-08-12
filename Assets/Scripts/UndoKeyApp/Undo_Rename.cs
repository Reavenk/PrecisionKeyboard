using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_Rename : Undo_Base
{
    string origName = string.Empty;

    public Undo_Rename(
        bool isUndo, 
        IKeyboardApp app, 
        WiringCollection collection, 
        WiringDocument doc, 
        string origName)
        : base("Rename Wiring", isUndo, app, collection, doc)
    { 
        this.origName = origName;
    }

    public override string GetName()
    {
        return $"Rename {this.origName} to {this.wiring.GetName()}";
    }

    public override PxPre.Undo.BaseUndo Undo()
    { 
        string newOName = this.wiring.GetName();

        this.app.RenameWiring(
            this.collection, 
            this.wiring, 
            this.origName);

        return new Undo_Rename(
            !this.isUndo, 
            this.app, 
            this.collection, 
            this.wiring, 
            newOName);
    }
}
