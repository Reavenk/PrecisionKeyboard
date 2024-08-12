// <copyright file="EditParamDlgRoot.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The base plate for param editing contexts.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditParamDlgRoot : UnityEngine.UI.Image
{
    public RectTransform pianoViewport;
    public RectTransform pianoRoot;
    public KeyboardScrollbar pianoScroll;

    public UnityEngine.UI.Text paramDocumentation;

    public UnityEngine.UI.Text title;

    public RectTransform contentSection;

    public UnityEngine.UI.Image eStop;

    string startingValue;

    public PaneWiring wiringPane;
    public Application application;
    public ParamBase param;
    public ParamThumbBase spawner;

    ParamEditBase editor;

    KeyCollection keyCollection = null;

    UnityEngine.UI.Image strobeTarget = null;
    Coroutine strobeSourceCoroutine = null;

    public System.Action onClose;

    public void OnButton_Cancel()
    {
        this.ResetToOriginalValue();
        this.application.DoVibrateButton();

        this.CloseDialog();
    }

    public void ResetToOriginalValue()
    { 
        if(this.param != null)
            this.param.SetValueFromString(this.startingValue);
    }

    public void OnButton_Confirm()
    { 
        this.editor.OnConfirm();

        this.application.DoVibrateButton();

        this.CloseDialog();
    }

    public void CloseDialog()
    {
        this.onClose?.Invoke();
        GameObject.Destroy(this.gameObject);
    }

    public void OnPiano_Scrollbar()
    { 
    }

    //void OnScroll_Scale(float f)
    //{
    //    this.SetKeyWidthPercent(f * this.GetKeyWidthPercent());
    //}
    //
    //public void SetKeyWidthPercent(float per,bool force = false)
    //{
    //    this.keyDims.whiteKeyWidth = Mathf.Lerp(minWhiteKeyWidth, maxWhiteKeyWidth, per);
    //    this.keyDims.blackKeyWidth = Mathf.Lerp(minBlackKeyWidth, maxBlackKeyWidth, per);
    //
    //    // This shouldn't normally happen
    //    if (this.keyCollection == null)
    //        return;
    //
    //    float len = this.keyCollection.AlignKeys();
    //    this.keyCollection.keyboardWidth = len;
    //    this.UpdateScrollDims();
    //    this.UpdateScroll();
    //
    //}
    //
    //public float GetKeyWidthPercent()
    //{
    //    return this.sliderKeyWidth.value;
    //}

    public void Initialize(
        IWiringEditorBridge editorBridge,
        WiringCollection collection, 
        WiringDocument parentDocument,
        KeyAssets keyAssets, 
        KeyDimParams keyDims, 
        LLDNBase owner,
        ParamBase param, 
        Application app, 
        PaneWiring wiringPane, 
        ParamThumbBase spawner, 
        GameObject editWidget, 
        UnityEngine.UI.Image strobeTarget)
    { 
        this.keyCollection = 
            new KeyCollection(
                keyAssets, 
                keyDims, 
                this.pianoRoot, 
                this.pianoViewport,
                keyAssets.whiteKeyColors,
                keyAssets.blackKeyColors);

        this.application = app;
        this.wiringPane = wiringPane;

        this.application.AddEStop(this.eStop, true);

        this.param = param;
        if(this.param != null)
        {
            this.startingValue = this.param.GetStringValue();
        }

        this.spawner = spawner;

        this.title.text = "Edit: " + param.name;

        this.keyCollection.keyboardWidth = 
            this.keyCollection.CreateKeyboardRange(3, 5, app.keyboardPane, true);

        this.keyCollection.SetKeyLabels(true, true, true, false);

        PaneKeyboard.CenterToNoteRange(
            this.keyCollection, 
            this.pianoViewport, 
            this.pianoRoot, 
            this.pianoScroll, 
            new KeyPair(PxPre.Phonics.WesternFreqUtils.Key.C, 4),
            new KeyPair(PxPre.Phonics.WesternFreqUtils.Key.B, 4));

        this.pianoScroll.onValueChanged.AddListener(
            (x)=>
            { 
                PaneKeyboard.UpdateScroll( 
                    this.keyCollection, 
                    this.pianoViewport, 
                    this.pianoRoot, 
                    this.pianoScroll);
            });

        if(strobeTarget != null)
        { 
            this.strobeTarget = strobeTarget;
            this.strobeSourceCoroutine = this.StartCoroutine(this.StrobeSource());
        }

        PaneKeyboard.UpdateScrollDims(
            this.keyCollection,
            this.pianoViewport,
            this.pianoRoot,
            this.pianoScroll);

        if (editWidget != null)
        {
            GameObject goWidget = GameObject.Instantiate<GameObject>(editWidget);
            goWidget.transform.SetParent(this.contentSection);
            goWidget.transform.localRotation = Quaternion.identity;
            goWidget.transform.localScale = Vector3.one;
            RectTransform rtW = goWidget.GetComponent<RectTransform>();
            rtW.anchorMin = Vector2.zero;
            rtW.anchorMax = Vector2.one;
            rtW.offsetMin = Vector2.zero;
            rtW.offsetMax = Vector2.zero;
            rtW.pivot = new Vector2(0.5f, 0.5f);
            ParamEditBase peb = goWidget.GetComponent<ParamEditBase>();
            peb.Initialize(this, owner, spawner);

            this.editor = peb;

            spawner.AttachToEdit(
                editorBridge,
                collection, 
                parentDocument, 
                peb);
        }

        this.paramDocumentation.text = param.description;
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        bool scrollActive = 
            PaneKeyboard.UpdateScrollDims(
                this.keyCollection,
                this.pianoViewport,
                this.pianoRoot,
                this.pianoScroll);

        if(scrollActive == true)
        {
            PaneKeyboard.UpdateScroll(
                this.keyCollection,
                this.pianoViewport,
                this.pianoRoot,
                this.pianoScroll);
        }
    }

    IEnumerator StrobeSource()
    { 
        while(true)
        {
            // Cycle ones every two seconds
            this.strobeTarget.color = 
                Color.Lerp(
                    Color.white, 
                    new Color(1.0f, 0.5f, 0.0f),
                    Mathf.Sin(Time.time * Mathf.PI)); 

            yield return null;
        }
    }

    protected override void OnDestroy()
    {

        if(this.application != null)
            this.application.RemoveEStop(this.eStop);

        if(this.strobeTarget != null)
            this.strobeTarget.color = Color.white;

        if(this.spawner != null)
            this.spawner.UpdateDisplayValue();

        base.OnDestroy();
    }
}
