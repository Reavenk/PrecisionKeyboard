// <copyright file="KeyDispBase.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class KeyDispBase : MonoBehaviour
{
    protected Application app;
    protected PaneKeyboard keyboard;

    [UnityEngine.SerializeField]
    protected Sprite tabIcon;

    [UnityEngine.SerializeField]
    protected string tabLabel = "";

    public abstract bool AvailableDuringExercise(PaneKeyboard.ExerciseDisplayMode exDisp);

    public virtual Sprite GetTabIcon(bool exerciseRunning)
    { 
        return this.tabIcon;
    }

    public virtual string GetTabString(bool exerciseRunning)
    { 
        return this.tabLabel;
    }

    public virtual void OnShow(bool exercise)
    { 
    }

    public virtual void OnHide()
    { 
    }

    public virtual void Init(Application app, PaneKeyboard keyboard)
    { 
        this.keyboard = keyboard;
        this.app = app;
    }

    public virtual void OnNoteStart(Application.NoteStartEvent eventType, int noteID, float velocity, int handle)
    { }

    public virtual void OnNoteEnd(int handle)
    { }

    public virtual void OnEStop()
    { }
}
