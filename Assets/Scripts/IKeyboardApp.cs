using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WiringIndexPair
{ 
    public int idx;
    public WiringDocument doc;

    public WiringIndexPair(WiringDocument doc, int idx)
    { 
        this.doc = doc;
        this.idx = idx;
    }
}

public interface IKeyboardApp
{
    bool RenameWiring(WiringCollection collection, WiringDocument wd, string name);

    bool SetLLDAWParamValue(WiringCollection collection, WiringDocument wd, LLDNBase owner, ParamBase parameter, string newStringValue);

    void AddWiringDocument(WiringCollection collection, WiringDocument wd);

    bool RewireNodeAudio(WiringCollection collection, WiringDocument wd, LLDNBase output, LLDNBase input, ParamConnection parameter);

    bool SetWiringCategory(WiringCollection collection, WiringDocument wd, WiringDocument.Category category);

    bool InsertWirings(WiringCollection collection, params WiringIndexPair [] docs);

    bool DeleteWirings(WiringCollection collection, params WiringDocument [] docs);

    bool SetActiveDocument(WiringCollection collection, WiringDocument active);

    bool SetWiringIndex(WiringCollection collection, WiringDocument wd, int index);

    bool ResetWirings(WiringCollection collection, IEnumerable<WiringDocument> newWirings);

    bool AddWiringNode(WiringCollection collection, WiringDocument wd, LLDNBase node);

    bool DeleteWiringNode(WiringCollection collection, WiringDocument wd, LLDNBase node);

    bool MoveWiringNode(WiringCollection collection, WiringDocument wd, LLDNBase node, Vector2 position);

    bool ShiftDocument(WiringCollection collection, WiringDocument wd, Vector2 offset);

}
