using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Prototype information for creating the drag-plate that can be
/// dragged and dropped for creating new nodes.
/// </summary>
public struct WiringFactoryListing
{ 
    /// <summary>
    /// The string to show in the plate.
    /// </summary>
    public string labelName;

    /// <summary>
    /// The node value.
    /// </summary>
    public LLDNBase.NodeType nodeType;

    /// <summary>
    /// The category of the node, use for visual grouping - mainly color coding.
    /// </summary>
    public LLDNBase.Category category;

    public WiringFactoryListing(string label, LLDNBase.Category category)
    { 
        this.labelName = label;
        this.nodeType = LLDNBase.NodeType.Void;
        this.category = category;
    }

    public WiringFactoryListing(LLDNBase.NodeType nodeType, LLDNBase.Category category)
    { 
        this.labelName = null;
        this.nodeType = nodeType;
        this.category = category;
    }
}