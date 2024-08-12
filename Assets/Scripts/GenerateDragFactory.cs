using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateDragFactory : 
    UnityEngine.UI.Button,
    UnityEngine.EventSystems.IBeginDragHandler,
    UnityEngine.EventSystems.IEndDragHandler,
    UnityEngine.EventSystems.IDragHandler
{
    public PaneWiring parent;
    public Application app;

    public LLDNBase.NodeType nodeType;
    public Sprite dragSprite;

    void UnityEngine.EventSystems.IBeginDragHandler.OnBeginDrag(
        UnityEngine.EventSystems.PointerEventData eventData)
    { 
        this.parent.OnDefferedDragFactoryOnBeginDrag(this, eventData);
    }

    void UnityEngine.EventSystems.IEndDragHandler.OnEndDrag(
        UnityEngine.EventSystems.PointerEventData eventData)
    { 
        this.parent.OnDefferedDragFactoryOnEndDrag(this, eventData);
    }

    void UnityEngine.EventSystems.IDragHandler.OnDrag(
        UnityEngine.EventSystems.PointerEventData eventData)
    { 
        this.parent.OnDefferedDragFactoryOnDrag(this, eventData);
    }
}
