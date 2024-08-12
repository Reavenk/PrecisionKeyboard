using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Undo_Base : PxPre.Undo.BaseUndo
{
    protected IKeyboardApp app;
    protected WiringCollection collection;
    protected WiringDocument wiring;

    protected Undo_Base(string name, bool isUndo, IKeyboardApp app, WiringCollection collection, WiringDocument wiring)
        : base(name, isUndo)
    { 
        this.app = app;
        this.collection = collection;
        this.wiring = wiring;
    }
}
