using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAppEventSink
{
    void OnWiringCleared(WiringCollection collection);

    void OnWiringAdded(WiringCollection collection, WiringDocument wd);

    void OnWiringActiveChanges(WiringCollection collection, WiringDocument wd);

    void OnWiringLinkChanged(WiringCollection collection, WiringDocument wd, LLDNBase outNode, LLDNBase inNode, ParamConnection inSocket);

    void OnWiringParamValueChanged(WiringCollection collection, WiringDocument wd, LLDNBase owner, ParamBase param);

    void OnWiringNodeAdded(WiringCollection collection, WiringDocument wd, LLDNBase newNode);

    void OnWiringNodeDeleted(WiringCollection collection, WiringDocument wd, LLDNBase delNode);

    void OnWiringNodeMoved(WiringCollection collection, WiringDocument wd, LLDNBase moved);

    void OnWiringChangeSelection(WiringCollection collection, WiringDocument wd, LLDNBase newSel);

    void OnWiringRenamed(WiringCollection collection, WiringDocument wd);

    void OnWiringDeleted(WiringCollection collection, WiringDocument wd);

    void OnDocumentChanging();

    void OnDocumentChanged(WiringCollection collection, WiringDocument active);

    void OnShiftDocument(WiringCollection collection, WiringDocument wd, Vector2 shiftAmount);

    void OnUndoRedo(string name, bool undo);

    void OnNoteStart(Application.NoteStartEvent eventType, int noteID, float velocity, int pressHandle);

    void OnNoteEnd(int pressHandle);

}
