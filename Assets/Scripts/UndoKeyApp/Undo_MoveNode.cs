using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo_MoveNode : Undo_Base
{
    LLDNBase target;
    Vector2 oldPosition;

    public Undo_MoveNode(
        bool isUndo,
        IKeyboardApp app,
        WiringCollection collection,
        WiringDocument doc,
        LLDNBase target,
        Vector2 oldPosition)
        :
        base(
            "Move Node",
            isUndo,
            app,
            collection,
            doc)
    {
        this.target = target;
        this.oldPosition = oldPosition;
    }

    public override PxPre.Undo.BaseUndo Undo()
    {
        Vector2 oldPos = target.cachedUILocation;

        this.app.MoveWiringNode(
            this.collection, 
            this.wiring, 
            this.target, 
            this.oldPosition);

        return new Undo_MoveNode(
            !this.isUndo,
            this.app,
            this.collection,
            this.wiring,
            this.target,
            oldPos);
    }
}
