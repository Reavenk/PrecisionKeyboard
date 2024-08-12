﻿// <copyright file=".cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketUIInput : 
    UnityEngine.UI.Button,
    UnityEngine.EventSystems.IBeginDragHandler,
    UnityEngine.EventSystems.IDragHandler,
    UnityEngine.EventSystems.IEndDragHandler,
    UnityEngine.EventSystems.IPointerEnterHandler,
    UnityEngine.EventSystems.IPointerExitHandler

{
    public PaneWiring parentPane;

    public ParamConnection connection;
    public GNUIHost host;


    void UnityEngine.EventSystems.IBeginDragHandler.OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    { 
        this.parentPane.HandleSocketInput_BeginDrag(this, eventData);
    }

    void UnityEngine.EventSystems.IDragHandler.OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.parentPane.HandleSocketInput_Drag(this, eventData);
    }

    void UnityEngine.EventSystems.IEndDragHandler.OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.parentPane.HandleSocketInput_EndDrag(this, eventData);
    }

    void UnityEngine.EventSystems.IPointerEnterHandler.OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.parentPane.HandleSocketInput_PointerEnter(this, eventData);
    }

    void UnityEngine.EventSystems.IPointerExitHandler.OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.parentPane.HandleSocketInput_PointerExit(this, eventData);
    }
}