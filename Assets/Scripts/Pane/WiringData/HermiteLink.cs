using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The UI record of all connections shown for the current wiring document.
/// </summary>
public struct HermiteLink
{ 
    /// <summary>
    /// The output socket for the link.
    /// </summary>
    public LLDNBase nodeOutput;

    /// <summary>
    /// The input socket owner for the link
    /// </summary>
    public LLDNBase nodeInput;

    /// <summary>
    /// The parameter tied to the input socket for the link.
    /// </summary>
    public ParamConnection inputParam;

    /// <summary>
    /// The graphic mesh representing the curve.
    /// </summary>
    public HermiteUICurve uiCurve;
}