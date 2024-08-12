using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_SetCategory : Undo_Base
{
    public WiringDocument.Category category;

    public Undo_SetCategory(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        WiringDocument doc,
        WiringDocument.Category category)
        : base("Set Category", isUndo, app, collection, doc)
    { 
        this.category = category;
    }

    public override string GetName()
    {
        return $"Set Category To {WiringDocument.GetCategoryName(this.category)}";
    }

    public override PxPre.Undo.BaseUndo Undo()
    { 
        WiringDocument.Category oldCat = this.wiring.category;

        this.app.SetWiringCategory( 
            this.collection,
            this.wiring,
            this.category);

        return new Undo_SetCategory(
            !this.isUndo, 
            this.app, 
            this.collection, 
            this.wiring, 
            oldCat);
    }
}
