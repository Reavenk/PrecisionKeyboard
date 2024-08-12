using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The various nodes supported and basic information tied to them.
/// </summary>
public struct NodeInfo
{ 
    /// <summary>
    /// The node type.
    /// </summary>
    public LLDNBase.NodeType type;

    /// <summary>
    /// The string value to call the node type.
    /// </summary>
    public string label;

    /// <summary>
    /// The icon to show for the node icon.
    /// </summary>
    public Sprite icon;

    public NodeInfo(LLDNBase.NodeType type, string label, Sprite icon)
    { 
        this.type = type;
        this.label = label;
        this.icon = icon;
    }
}