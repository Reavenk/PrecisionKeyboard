using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUndoReceiver
{
    void OnUndoClear();
    void OnAddUndo(int undoCount, int redoCount);
    void OnAddRedo(int undoCount, int redoCount);
}
