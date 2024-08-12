using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_ShiftDocument : Undo_Base
{
    Vector2 offset;

    public Undo_ShiftDocument(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        WiringDocument wiring,
        Vector2 offset)
        : base(
            "Offset",
            isUndo,
            app,
            collection,
            wiring)
    { 
        this.offset = offset;
    }

    public override string GetName()
    {
        return $"Repositioning {this.wiring.GetProcessedWiringName(this.collection)}";
    }

    public override PxPre.Undo.BaseUndo Undo()
    { 
        this.app.ShiftDocument(
            this.collection,
            this.wiring,
            -this.offset);

        return new 
            Undo_ShiftDocument(
                !this.isUndo,
                this.app,
                this.collection,
                this.wiring,
                -this.offset);
    }

}
