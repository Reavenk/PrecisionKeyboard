using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: This is probably obsolete
public interface IWiringEditorBridge
{
    // Called when something could change the state of nodes being
    // in the output network.
    void FlagOutputNetworkDirty();
}
