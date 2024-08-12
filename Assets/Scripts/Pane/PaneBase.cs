// <copyright file="PaneBase.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The base options panel.</summary>

using UnityEngine;

public abstract class PaneBase : 
    UnityEngine.UI.Image,
    IAppEventSink
{
    /// <summary>
    /// The cached copy of the main application.
    /// </summary>
    private Application app;

    /// <summary>
    /// The accessor to the protected application cache.
    /// </summary>
    public Application App {get{return this.app; } }

    /// <summary>
    /// The type of tab the pane is.
    /// </summary>
    /// <returns></returns>
    public abstract Application.TabTypes TabType();

    /// <summary>
    /// Initialize the pane. This virtual function should
    /// call its base
    /// </summary>
    /// <param name="app">The main application.</param>
    public virtual void InitPane(Application app)
    { 
        this.app = app;
    }

    /// <summary>
    /// Called when the pane is being shown.
    /// </summary>
    /// <param name="exercise">If true</param>
    public virtual void OnShowPane(bool exercise)
    { }

    /// <summary>
    /// Called when a panel is being hidden after previously shown.
    /// </summary>
    public virtual void OnHidePane()
    { }

    /// <summary>
    /// Called when the master volume is changed. This function could be called
    /// multiple times in a row from a drag operation, such as from a slider.
    /// </summary>
    /// <param name="newVal">The new master volume, a value between [0.0, 1.0]</param>
    public virtual void OnMasterVolumeChange(float newVal)
    { }

    public virtual void OnMetronomeVolumeChange(float newVal)
    { }

    /// <summary>
    /// Called after an eStop has occured.
    /// </summary>
    public virtual void OnEStop()
    { }

    /// <summary>
    /// Called when the metronome is started.
    /// </summary>
    public virtual void OnMetronomeStart()
    { }

    /// <summary>
    /// Called when the metronome is stopped.
    /// </summary>
    public virtual void OnMetronomeStop()
    { }

    /// <summary>
    /// Called when an exercise is started.
    /// </summary>
    public virtual void OnExerciseStart()
    { }

    /// <summary>
    /// Called after an exercise is ended.
    /// </summary>
    public virtual void OnExerciseStop()
    { }

    /// <summary>
    /// Called when the BPM is changed.
    /// </summary>
    /// <param name="bpm">The new BPM</param>
    /// <param name="str">The preffered string representation of the BPM.</param>
    public virtual void OnBPMChange(float bpm, string str)
    { }

    /// <summary>
    /// Called right after a wiring document is saved.
    /// </summary>
    public virtual void OnSave()
    { }

    /// <summary>
    /// Called right after the wiring document is changed.
    /// 
    /// This includes loading and clearing the document.
    /// </summary>
    public virtual void OnDocumentChanged(WiringFile newWiring, WiringDocument active, bool appended)
    { }

    public virtual void OnChangedAccidentalMode(KeyCollection.Accidental acc)
    { }

    void IAppEventSink.OnWiringCleared(WiringCollection collection)
    { 
        this.AppSink_OnWiringCleared(collection);
    }

    protected virtual void AppSink_OnWiringCleared(WiringCollection collection)
    { }

    void IAppEventSink.OnWiringAdded(WiringCollection collection, WiringDocument wd)
    { 
        this.AppSink_OnWiringAdded(collection, wd);
    }

    protected virtual void AppSink_OnWiringAdded(WiringCollection collection, WiringDocument wd)
    { }

    void IAppEventSink.OnWiringActiveChanges(WiringCollection collection, WiringDocument wd)
    {
        this.AppSink_OnWiringActiveChanges(collection, wd);
    }

    protected virtual void AppSink_OnWiringActiveChanges(WiringCollection collection, WiringDocument wd)
    { }

    void IAppEventSink.OnWiringLinkChanged(WiringCollection collection, WiringDocument wd, LLDNBase outNode, LLDNBase inNode, ParamConnection inSocket)
    { 
        this.AppSink_OnWiringLinkChanged(collection, wd, outNode, inNode, inSocket);
    }

    protected virtual void AppSink_OnWiringLinkChanged(WiringCollection collection, WiringDocument wd, LLDNBase outNode, LLDNBase inNode, ParamConnection inSocket)
    { }

    void IAppEventSink.OnWiringParamValueChanged(WiringCollection collection, WiringDocument wd, LLDNBase owner, ParamBase param)
    { 
        this.AppSink_OnWiringParamValueChanged(collection, wd, owner, param);
    }

    protected virtual void AppSink_OnWiringParamValueChanged(WiringCollection collection, WiringDocument wd, LLDNBase owner, ParamBase param)
    { }

    void IAppEventSink.OnWiringNodeAdded(WiringCollection collection, WiringDocument wd, LLDNBase newNode)
    { 
        this.AppSink_OnWiringNodeAdded(collection, wd, newNode);
    }

    protected virtual void AppSink_OnWiringNodeAdded(WiringCollection collection, WiringDocument wd, LLDNBase newNode)
    { }

    void IAppEventSink.OnWiringNodeDeleted(WiringCollection collection, WiringDocument wd, LLDNBase delNode)
    { 
        this.AppSink_OnWiringNodeDeleted(collection, wd, delNode);
    }

    protected virtual void AppSink_OnWiringNodeDeleted(WiringCollection collection, WiringDocument wd, LLDNBase delNode)
    { }

    void IAppEventSink.OnWiringNodeMoved(WiringCollection collection, WiringDocument wd, LLDNBase moved)
    { 
        this.AppSink_OnWiringNodeMoved(collection, wd, moved);
    }

    protected virtual void AppSink_OnWiringNodeMoved(WiringCollection collection, WiringDocument wd, LLDNBase moved)
    { }

    void IAppEventSink.OnWiringChangeSelection(WiringCollection collection, WiringDocument wd, LLDNBase newSel)
    { 
        this.AppSink_OnWiringChangeSelection(collection, wd, newSel);
    }

    protected virtual void AppSink_OnWiringChangeSelection(WiringCollection collection, WiringDocument wd, LLDNBase newSel)
    { }

    void IAppEventSink.OnWiringRenamed(WiringCollection collection, WiringDocument wd)
    { 
        this.AppSink_OnWiringRenamed(collection, wd);
    }

    protected virtual void AppSink_OnWiringRenamed(WiringCollection collection, WiringDocument wd)
    { }

    void IAppEventSink.OnDocumentChanging()
    { 
        this.AppSink_OnDocumentChanging();
    }

    protected virtual void AppSink_OnDocumentChanging()
    { }

    void IAppEventSink.OnDocumentChanged(WiringCollection collection, WiringDocument active)
    { 
        this.AppSink_OnDocumentChanged(collection, active);
    }

    public virtual void AppSink_OnDocumentChanged(WiringCollection collection, WiringDocument active)
    { }

    void IAppEventSink.OnWiringDeleted(WiringCollection collection, WiringDocument wd)
    { 
        this.AppSink_OnWiringDeleted(collection, wd);
    }

    public virtual void AppSink_OnWiringDeleted(WiringCollection collection, WiringDocument wd)
    { }

    void IAppEventSink.OnShiftDocument(WiringCollection collection, WiringDocument wd, Vector2 shiftAmount)
    { 
        this.AppSink_OnShiftDocument(collection, wd, shiftAmount);
    }

    public virtual void AppSink_OnShiftDocument(WiringCollection collection, WiringDocument wd, Vector2 shiftAmount)
    { }

    void IAppEventSink.OnUndoRedo(string name, bool undo)
    { 
        this.AppSink_OnUndoRedo(name, undo);
    }

    public virtual void AppSink_OnUndoRedo(string name, bool undo){ }

    void IAppEventSink.OnNoteStart(Application.NoteStartEvent eventType, int noteID, float velocity, int pressHandle)
    { 
        this.AppSink_OnNoteStart(eventType, noteID, velocity, pressHandle);
    }

    public virtual void AppSink_OnNoteStart(Application.NoteStartEvent eventType, int noteID, float velocity, int pressHandle)
    { }

    void IAppEventSink.OnNoteEnd( int pressHandle)
    { 
        this.AppSink_OnNoteEnd(pressHandle);
    }

    public virtual void AppSink_OnNoteEnd( int pressHandle)
    { }
}
