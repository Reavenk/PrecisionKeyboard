// <copyright file="BaseExercise.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The base class for an exercise.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class BaseExercise : BaseContent
{
    public struct TitlebarObjects
    { 
        public UnityEngine.UI.Text timerText;
        public UnityEngine.UI.Text progressText;
    }

    protected struct OctaveRange
    {
        public int min;
        public int max;

        public OctaveRange(int min, int max)
        { 
            this.min = min;
            this.max = max;
        }
    }

    public struct TutorialRegion
    { 
        public RectTransform main;

        public RectTransform exerciseStats;
        public UnityEngine.UI.Text questionCount;

        public UnityEngine.UI.Image correctIcon;
        public UnityEngine.UI.Text correctQuestions;

        public UnityEngine.UI.Image incorrectIcon;
        public UnityEngine.UI.Text incorrectQuestions;

        public int correctQuestionsCtr;
        public int incorrectQuestionCtr;
        public int questionCtr;
        public int totalQuestionCt;

        public RectTransform leaveButtonRect;
        public UnityEngine.UI.Button leaveButton;

        public RectTransform replayButtonRect;
        public UnityEngine.UI.Button replayButton;

        public float hostRightOffset;
    }

    public static TitlebarObjects titlebarObjs = new TitlebarObjects();
    public static TutorialRegion tutorialRegionObjs = new TutorialRegion();
    public static List<KeyPair> pressedNoteCache = new List<KeyPair>();

    public static bool HasCachedNote(KeyPair kp, bool onlyFirst, bool clear)
    { 
        if(pressedNoteCache.Count == 0)
            return false;

        bool ret = false;
        if(onlyFirst == true)
            ret = KeyPair.Matches(pressedNoteCache[0], kp);
        else
        { 
            foreach(KeyPair kpi in pressedNoteCache)
            { 
                if(KeyPair.Matches(kpi, kp) == true)
                { 
                    ret = true;
                    break;
                }
            }
        }

        if(clear == true)
            pressedNoteCache.Clear();

        return ret;
    }

    protected List<ExerciseEnumParam> exerciseParams = new List<ExerciseEnumParam>();
    public IEnumerable<ExerciseEnumParam> ExerciseParams {get{return this.exerciseParams; } }

    protected ExerciseEnumParam paramTimed;
    protected ExerciseEnumParam paramNumber;
    protected ExerciseEnumParam paramShowAnswers;

    protected const int questionFontSz = 50;
    protected const int questionEntryFontSz = 50;

    public const float messageTweenDist = 30.0f;

    public enum Grade
    { 
        APl,
        A,
        B,
        C,
        D,
        F
    }

    /// <summary>
    /// The initialization of the exercise. Called once at the 
    /// beginning of app initialization.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <returns></returns>
    public override bool Initialize(Application app, ExerciseAssets assets, IExerciseParamProvider sharedParams)
    { 
        base.Initialize(app, assets, sharedParams);

        this.paramTimed = sharedParams.paramTimed;
        this.paramNumber = sharedParams.paramNumber;
        this.paramShowAnswers = sharedParams.paramShowAnswers;

        return true;
    }

    /// <summary>
    /// Called when the exercise is started.
    /// </summary>
    public virtual void StartExercise(PaneKeyboard.ExerciseDisplayMode displayMode)
    {
        pressedNoteCache.Clear();

        int questions = this.GetQuestionsCount();
        bool showAnswers = this.GetQuestionsShowAnswers();
        float timer = this.GetQuestionExerciseTime();
        TweenUtil tweener = this.CreateTweener();
        ExerciseUtil exu = new ExerciseUtil(this.app, this, tweener, questions, timer);

        this.app.sleepHoldMgr.StayAwakeFor(Application.sleepOffsetEndExercise);

        this.app.StartExerciseCoroutine(
            this.MainExerciseCoroutine(
                questions,
                showAnswers,
                timer,
                tweener,
                exu));
    }

    /// <summary>
    /// Called when a piano key note is pressed while active.
    /// </summary>
    /// <param name="note"></param>
    public virtual void OnNoteStart(int pressHandle, KeyPair note)
    { 
        if(this.GetDisplayMode() == PaneKeyboard.ExerciseDisplayMode.KeyboardExercise)
            pressedNoteCache.Add(note);
    }

    /// <summary>
    /// Called when a piano key note is released while active.
    /// </summary>
    /// <param name="note"></param>
    public virtual void OnNoteEnd(int pressHandle)
    { }

    /// <summary>
    /// Called when the eStop is pressed while active.
    /// </summary>
    public virtual void OnEStop(){ }

    /// <summary>
    /// Called when the exercise is ended.
    /// </summary>
    public virtual void OnExerciseEnd(){ }

    public virtual bool RequiresWiringWarning()
    { return true; }

    /// <summary>
    /// Utility function to create the starting dialog.
    /// </summary>
    public override PxPre.UIL.Dialog CreateDialog(ExerciseAssets assets)
    { 
        PxPre.UIL.Dialog dlg = 
            this.app.dlgSpawner.CreateDialogTemplate(new Vector2(650.0f, -1.0f), PxPre.UIL.LFlag.AlignCenter, 0.0f);

        dlg.dialogSizer.border = new PxPre.UIL.PadRect(0.0f, 20.0f, 0.0f, 20.0f);

        dlg.AddDialogTemplateTitle("Exercise: " + this.title);

        dlg.AddDialogTemplateSeparator();

        dlg.AddDialogTemplateButtons(
            new PxPre.UIL.DlgButtonPair("Cancel", null),
            new PxPre.UIL.DlgButtonPair("Start", 
            (x)=>
            { 
                this.app.StartExercise(this, this.GetDisplayMode());
                dlg.onClose = null;
                dlg.onClose += () => { this.SaveParams(); };
                return true;
            }));

        this.CreateDialogImpl(this.app.uiFactory, dlg, assets);
        dlg.host.LayoutInRTSmartFit();

        dlg.onClose += ()=>{ this.SaveParams(); };

        return dlg;
    }

    public virtual PaneKeyboard.ExerciseDisplayMode GetDisplayMode()
    { 
        return PaneKeyboard.ExerciseDisplayMode.KeyboardExercise;
    }

    /// <summary>
    /// Implementation to fill in the starting dialog.
    /// </summary>
    /// <param name="dt">The dialog to insert content into.</param>
    protected virtual void CreateDialogImpl(PxPre.UIL.Factory uifactory, PxPre.UIL.Dialog dlg, ExerciseAssets assets)
    {
        this.CreateDialogHeaderContent(uifactory, dlg);

        PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(uifactory, dlg.rootParent, dlg.contentSizer);
        uiStack.PushImage(this.app.exerciseAssets.scrollRectBorder, 1.0f, PxPre.UIL.LFlag.Grow).Chn_SetImgSliced().Chn_SetImgFillCenter(false);
        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);

            PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> evsr = 
                uiStack.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(1.0f, PxPre.UIL.LFlag.GrowHoriz|PxPre.UIL.LFlag.GrowVertOnCollapse);

            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(10.0f, 0.0f, 10.0f, 0.0f);

                PxPre.UIL.ElePropGrid epg = new PxPre.UIL.ElePropGrid(evsr, uifactory.textTextAttrib);
                uiStack.GetHeadSizer().Add(epg, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

                this.PopulatePropertyGrid(uifactory, epg, evsr, dlg.host.RT, assets);

                this.EndOfDialogScroll(uifactory, evsr, uiStack.GetHeadSizer(), assets);

            uiStack.Pop();
            uiStack.Pop();
        uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(5.0f, 0.0f, 0);

        DoNothing dn = dlg.rootParent.RT.gameObject.AddComponent<DoNothing>();
        dn.StartCoroutine(PxPre.UIL.EleVertScrollRgn.SetVertScrollLater(evsr.ScrollRect, 1.0f));
    }

    protected virtual void EndOfDialogScroll(PxPre.UIL.Factory uifactory, PxPre.UIL.EleBaseRect rect, PxPre.UIL.EleBaseSizer sizer, ExerciseAssets assets)
    {
        if (this.RequiresWiringWarning() == true)
        {
            string warn = "This exercise uses the active wiring instrument. Make sure it can play audible and identifyable tones. Take into consideration your device's frequency response and the timbre of the wiring used.";


            if (this.app.wiringPane.ViewedWiringMakesNoise() == false)
            { 
                
                warn += "\n<b>From analyzing the current wiring, it is predicted to make no noise! Consider changing it.</b>";
                AddAlertHeader(uifactory, assets, rect, sizer, warn);
            }
            else
            { 
                AddInfoHeader(uifactory, assets, rect, sizer, warn);
            }
        }
    }

    protected void AddWarningHeader(PxPre.UIL.Factory uifactory, ExerciseAssets assets, PxPre.UIL.EleBaseRect rect, PxPre.UIL.EleBaseSizer sizer, string text)
    { 
        this.AddInfoHeader(
                uifactory, 
                assets,
                rect, 
                sizer,
                assets.sideWarning, 
                text,
                new Color(1.0f, 1.0f, 0.9f));
    }

    protected void AddInfoHeader(PxPre.UIL.Factory uifactory, ExerciseAssets assets, PxPre.UIL.EleBaseRect rect, PxPre.UIL.EleBaseSizer sizer, string text)
    { 
        this.AddInfoHeader(
                uifactory, 
                assets,
                rect, 
                sizer,
                assets.sideInfo, 
                text,
                new Color(0.9f, 0.9f, 1.0f));
    }

    protected void AddMonetizationHeader(PxPre.UIL.Factory uifactory, ExerciseAssets assets, PxPre.UIL.EleBaseRect rect, PxPre.UIL.EleBaseSizer sizer, string text)
    { 
        this.AddInfoHeader(
                uifactory, 
                assets,
                rect, 
                sizer,
                assets.sideMonetize, 
                text,
                new Color(0.9f, 1.0f, 0.9f));
    }

    protected void AddAlertHeader(PxPre.UIL.Factory uifactory, ExerciseAssets assets, PxPre.UIL.EleBaseRect rect, PxPre.UIL.EleBaseSizer sizer, string text)
    { 
        this.AddInfoHeader(
            uifactory,
            assets,
            rect,
            sizer,
            assets.sideError,
            text,
            new Color(1.0f, 0.9f, 0.9f));
    }

    protected void AddInfoHeader(PxPre.UIL.Factory uifactory, ExerciseAssets assets, PxPre.UIL.EleBaseRect rect, PxPre.UIL.EleBaseSizer sizer, Sprite icon, string text, Color bgCol)
    { 
        PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(uifactory, rect, sizer);

        PxPre.UIL.EleImg imgPlate = uiStack.PushImage(assets.plateRounder, 0.0f, PxPre.UIL.LFlag.GrowHoriz).Chn_SetImgSliced();
        imgPlate.Img.color = bgCol;

        uiStack.PushHorizSizer();

            uiStack.AddSpace(10.0f, 0.0f, 0);
            uiStack.AddImage(icon, 0.0f, PxPre.UIL.LFlag.AlignCenter);
            uiStack.AddSpace(10.0f, 0.0f, 0);
            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
                uiStack.AddSpace(10.0f, 0.0f, 0);
                uiStack.AddText(text, true, 1.0f, PxPre.UIL.LFlag.GrowHoriz|PxPre.UIL.LFlag.AlignVertCenter);
                uiStack.AddSpace(10.0f, 0.0f, 0);
            uiStack.Pop();
            uiStack.AddSpace(10.0f, 0.0f, 0);
    }

    protected void AddAccidentalHeader(PxPre.UIL.Factory uifactory, ExerciseAssets assets, PxPre.UIL.EleBaseRect rect, PxPre.UIL.EleBaseSizer sizer)
    { 
        PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(uifactory, rect, sizer);

        PxPre.UIL.EleImg imgPlate = uiStack.PushImage(assets.plateRounder, 0.0f, PxPre.UIL.LFlag.GrowHoriz).Chn_SetImgSliced();

        uiStack.PushHorizSizer().Chn_Border(10.0f);
            PxPre.UIL.EleGenButton<PushdownButton> pushB = uiStack.AddButton<PushdownButton>( "", 0.0f, PxPre.UIL.LFlag.AlignCenter);
            pushB.minSize = new Vector2(80.0f, 80.0f);
            uiStack.AddSpace(10.0f, 0.0f, 0);
            uiStack.AddText(
                "This exercise uses the accidental settings. Change it based on if you want accidentals to be called sharps (♯) or flats (♭).", 
                true, 
                1.0f, 
                PxPre.UIL.LFlag.GrowHoriz|PxPre.UIL.LFlag.AlignCenter);
        uiStack.Pop();

        pushB.Button.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);

        // These buttons have a top plate so pulldown and tranistion effects
        // aren't pullin on the same transforms, but instead on different
        // parts of the hierarchy.
        GameObject goTopPlate = new GameObject("AcciTopLayer");
        goTopPlate.transform.SetParent(pushB.RT, false);
        RectTransform rtTopPlate = goTopPlate.AddComponent<RectTransform>();
        rtTopPlate.pivot = new Vector2(0.0f, 1.0f);
        rtTopPlate.anchorMin = Vector2.zero;
        rtTopPlate.anchorMax = Vector2.one;
        rtTopPlate.offsetMin = Vector2.zero;
        rtTopPlate.offsetMax = Vector2.zero;

        Sprite ico = 
            (this.app.keyboardPane.GetAccidental() == KeyCollection.Accidental.Sharp) ?
                assets.accidentalSharp:
                assets.accidentalFlat;

        GameObject goIco = new GameObject("AcciIco");
        goIco.transform.SetParent(goTopPlate.transform, false);
        UnityEngine.UI.Image imgIco = goIco.AddComponent<UnityEngine.UI.Image>();
        imgIco.sprite = ico;
        RectTransform rtIco = imgIco.rectTransform;
        rtIco.pivot = new Vector2(0.5f, 0.5f);
        rtIco.anchorMin = new Vector2(0.5f, 0.5f);
        rtIco.anchorMax = new Vector2(0.5f, 0.5f);
        rtIco.anchoredPosition = new Vector2(0.0f, 5.0f);
        rtIco.sizeDelta = ico.rect.size;

        GameObject goChev = new GameObject("Chevron");
        goChev.transform.SetParent(goTopPlate.transform, false);
        UnityEngine.UI.Image imgChev = goChev.AddComponent<UnityEngine.UI.Image>();
        imgChev.sprite = assets.pulldownArrowSmaller;
        RectTransform rtChev = imgChev.rectTransform;
        rtChev.pivot = new Vector2(1.0f, 0.0f);
        rtChev.anchorMin = new Vector2(1.0f, 0.0f);
        rtChev.anchorMax = new Vector2(1.0f, 0.0f);
        rtChev.sizeDelta = assets.pulldownArrowSmaller.rect.size * 0.5f;
        rtChev.anchoredPosition = new Vector2(-10.0f, 10.0f);

        DoNothing dn = goTopPlate.AddComponent<DoNothing>();
        TweenUtil tu = new TweenUtil(dn);

        // This could probably use some logic pipelining with the real
        // keyboard pulldown - it's mostly duplicate stuff to the real
        // pulldown.
        pushB.Button.onClick.AddListener(
            ()=>
            { 
                PxPre.DropMenu.StackUtil stack = new PxPre.DropMenu.StackUtil();

                PulldownInfo.DoChevyDown(tu, imgChev);
                KeyCollection.Accidental acc = this.app.keyboardPane.GetAccidental();

                stack.AddAction(
                    acc == KeyCollection.Accidental.Sharp,
                    assets.accidentalSharp,
                    "Accidentals as Sharps",
                    () => 
                    { 
                        this.app.keyboardPane.SetAccidentalsMode(KeyCollection.Accidental.Sharp, false);
                        Application.DoDropdownIconUpdate(tu, imgIco);
                        imgIco.sprite = assets.accidentalSharp;
                    });

                stack.AddAction(
                    acc == KeyCollection.Accidental.Flat,
                    assets.accidentalFlat,
                    "Accidentals as Flats",
                    () => 
                    { 
                        this.app.keyboardPane.SetAccidentalsMode(KeyCollection.Accidental.Flat, false);
                        Application.DoDropdownIconUpdate(tu, imgIco);
                        imgIco.sprite = assets.accidentalFlat;
                    });


                PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
                    CanvasSingleton.canvas,
                    stack.Root,
                    rtTopPlate);

                this.app.DoVibrateButton();
            });
    }

    protected void CreateDialogHeaderContent(PxPre.UIL.Factory uifactory, PxPre.UIL.Dialog dlg)
    {
        PxPre.UIL.Ele e = uifactory.CreateText(dlg.rootParent, this.longDescription, true);
        dlg.contentSizer.Add(e, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
    }

    /// <summary>
    /// Setup the UI on the keyboard pane for the exercise.
    /// </summary>
    /// <param name="tutContainer">The container to place content into.</param>
    /// <returns>True if successful. Else false. If false is returned, the 
    /// exercise initialization should be aborted.</returns>
    public virtual bool SetupUI(RectTransform tutContainer, RectTransform mainTutBar, PaneKeyboard.ExerciseDisplayMode displayMode)
    {
        tutorialRegionObjs = 
            this.CreateExerciseRegion(
                tutContainer,
                displayMode,
                this.UsesReplayButton(displayMode));

        titlebarObjs = 
            this.CreateTitlebarContents(
                mainTutBar, 
                this.GetTitlebarIcons(), 
                this.GetTimeIcon());

        return true;
    }

    public static IEnumerator ResetScroll(UnityEngine.UI.ScrollRect sr, float val)
    {
        yield return new WaitForEndOfFrame();
        sr.verticalNormalizedPosition = val;
    }

    public virtual Sprite GetTimeIcon()
    {
        if(this.paramTimed == null)
            return null;

        if (this.paramTimed.GetInt() == -1)
            return null;

        return this.paramTimed.GetSprite();
    }

    public virtual bool UsesReplayButton(PaneKeyboard.ExerciseDisplayMode dm)
    { 
        return dm == PaneKeyboard.ExerciseDisplayMode.PlainExercise;
    }

    protected TitlebarObjects CreateTitlebarContents(RectTransform mainTutBar, Sprite[] titlebarIcons, Sprite clockIcon)
    {
        foreach(Transform t in mainTutBar.transform)
            GameObject.Destroy(t.gameObject);

        float fxL = 5.0f;

        TitlebarObjects ret = new TitlebarObjects();

        foreach (Sprite sp in titlebarIcons)
        {
            GameObject goIco = new GameObject("Ico");
            goIco.transform.SetParent(mainTutBar, false);
            UnityEngine.UI.Image imgIco = goIco.AddComponent<UnityEngine.UI.Image>();
            imgIco.color = Color.white;
            imgIco.sprite = sp;
            RectTransform rtHIco = imgIco.rectTransform;
            rtHIco.pivot = new Vector2(0.0f, 1.0f);
            rtHIco.anchorMin = new Vector2(0.0f, 1.0f);
            rtHIco.anchorMax = new Vector2(0.0f, 1.0f);
            rtHIco.anchoredPosition = new Vector2(fxL, -5.0f);
            rtHIco.sizeDelta = new Vector2(40.0f, 40.0f);
            fxL += 45.0f;
        }

        // CLOCK
        float fxR = 0.0f;

        if(clockIcon != null)
        {
            GameObject goTimeField = new GameObject("TimePlate");
            goTimeField.transform.SetParent(mainTutBar, false);
            UnityEngine.UI.Image imgTimeField = goTimeField.AddComponent<UnityEngine.UI.Image>();
            imgTimeField.sprite = this.app.exerciseAssets.plateRounder;
            imgTimeField.type = UnityEngine.UI.Image.Type.Sliced;
            RectTransform rtTimeField = imgTimeField.rectTransform;
            rtTimeField.anchorMin = new Vector2(1.0f, 0.0f);
            rtTimeField.anchorMax = new Vector2(1.0f, 1.0f);
            rtTimeField.pivot = new Vector2(0.0f, 1.0f);
            rtTimeField.offsetMin = new Vector2(-85.0f, 5.0f);
            rtTimeField.offsetMax = new Vector2(-10.0f, -5.0f);

            GameObject goTimeText = new GameObject("TimeText");
            goTimeText.transform.SetParent(goTimeField.transform, false);
            UnityEngine.UI.Text txtTime = goTimeText.AddComponent<UnityEngine.UI.Text>();
            this.app.uiFactory.ApplyTextStyle(txtTime);
            txtTime.text = "--";
            txtTime.alignment = TextAnchor.MiddleCenter;
            txtTime.rectTransform.anchorMin = Vector2.zero;
            txtTime.rectTransform.anchorMax = Vector2.one;
            txtTime.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            txtTime.rectTransform.anchoredPosition = Vector2.zero;
            txtTime.rectTransform.sizeDelta = Vector2.zero;
            txtTime.fontSize = 35;
            txtTime.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtTime.verticalOverflow = VerticalWrapMode.Overflow;
            ret.timerText = txtTime;

            GameObject goClockIco = new GameObject("TimeIco");
            goClockIco.transform.SetParent(mainTutBar, false);
            UnityEngine.UI.Image imgClockIco = goClockIco.AddComponent<UnityEngine.UI.Image>();
            imgClockIco.sprite = clockIcon;
            imgClockIco.color = Color.white;
            RectTransform rtClockIco = imgClockIco.rectTransform;
            rtClockIco.anchorMin = new Vector2(1.0f, 0.0f);
            rtClockIco.anchorMax = new Vector2(1.0f, 1.0f);
            rtClockIco.pivot = new Vector2(0.0f, 1.0f);
            rtClockIco.offsetMin = new Vector2(-130.0f, 5.0f);
            rtClockIco.offsetMax = new Vector2(-90.0f, -5.0f);

            fxR = 130.0f;
        }

        const float progressWidth = 120.0f;
        GameObject goProg = new GameObject("ProgressScore");
        goProg.transform.SetParent(mainTutBar, false);
        UnityEngine.UI.Text txtProg = goProg.AddComponent<UnityEngine.UI.Text>();
        this.app.uiFactory.textTextAttrib.Apply(txtProg);
        txtProg.text = "--/--";
        txtProg.fontSize = 30;
        txtProg.alignment = TextAnchor.MiddleCenter;
        txtProg.horizontalOverflow = HorizontalWrapMode.Overflow;
        txtProg.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rtProg = txtProg.rectTransform;
        rtProg.anchorMin = new Vector2(1.0f, 0.0f);
        rtProg.anchorMax = new Vector2(1.0f, 1.0f);
        rtProg.pivot = new Vector2(0.5f, 0.5f);
        rtProg.offsetMax = new Vector2(-fxR, 0.0f);
        rtProg.offsetMin = new Vector2(-fxR - progressWidth, 0.0f);
        fxR += progressWidth;
        ret.progressText = txtProg;
        

        // TITLE TEXT
        GameObject goTitle = new GameObject("Title");
        goTitle.transform.SetParent(mainTutBar, false);
        UnityEngine.UI.Text txtHeader = goTitle.AddComponent<UnityEngine.UI.Text>();
        RectTransform rtTitle = txtHeader.rectTransform;
        rtTitle.pivot = new Vector2(0.5f, 0.5f);
        rtTitle.anchorMin = Vector2.zero;
        rtTitle.anchorMax = Vector2.one;
        rtTitle.offsetMin = new Vector2(fxL, 0.0f);
        rtTitle.offsetMax = new Vector2(-fxR, 0.0f);
        txtHeader.fontSize = 20;
        txtHeader.alignment = TextAnchor.MiddleCenter;
        txtHeader.font = this.app.uiFactory.headerTextAttrib.font;
        txtHeader.text = this.title;
        txtHeader.color = Color.black;

        return ret;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="tutContainer"></param>
    protected TutorialRegion CreateExerciseRegion(
        RectTransform tutContainer,
        PaneKeyboard.ExerciseDisplayMode displayMode,
        bool replayButton)
    {
        TutorialRegion ret = new TutorialRegion();

        //
        //      LEAVE BUTTON
        //////////////////////////////////////////////////
        float leaveButtonWidth = 100.0f;
        float leaveButtonHeight = 80.0f;

        GameObject goBtn = new GameObject("LeaveButton");
        goBtn.transform.SetParent(tutContainer, false);
        UnityEngine.UI.Image imgBtn = goBtn.AddComponent<UnityEngine.UI.Image>();
        RectTransform rtBtn = imgBtn.rectTransform;
        rtBtn.pivot = new Vector2(1.0f, 0.0f);
        rtBtn.anchorMin = new Vector2(1.0f, 0.0f);
        rtBtn.anchorMax = new Vector2(1.0f, 0.0f);
        rtBtn.sizeDelta = new Vector2(leaveButtonWidth, leaveButtonHeight);
        rtBtn.anchoredPosition = new Vector2(-5.0f, 10.0f);
        PushdownButton pb = goBtn.AddComponent<PushdownButton>();
        this.app.uiFactory.ApplyButtonStyle(pb, imgBtn, null);
        pb.moveOnPress = new Vector3(0.0f, -5.0f);
        pb.onClick.AddListener(()=>{ this.app.keyboardPane.OnButton_StopExercise(); });

        GameObject goLeaveIco = new GameObject("LeaveIcon");
        goLeaveIco.transform.SetParent(rtBtn, false);
        UnityEngine.UI.Image imgLeaveIco = goLeaveIco.AddComponent<UnityEngine.UI.Image>();
        imgLeaveIco.color = Color.black;
        imgLeaveIco.sprite = this.app.leaveExerciseIcon;
        Sprite s = this.app.leaveExerciseIcon;
        RectTransform rtIco = imgLeaveIco.rectTransform;
        rtIco.pivot = new Vector2(0.5f, 0.5f);
        rtIco.anchorMin = new Vector2(0.5f, 0.5f);
        rtIco.anchorMax = new Vector2(0.5f, 0.5f);
        rtIco.anchoredPosition = new Vector3(0.0f, 3.0f);
        rtIco.sizeDelta = s.rect.size;
        rtIco.localScale = new Vector3(0.8f, 0.8f, 1.0f);

        if(replayButton == true)
        { 
            GameObject goReplayBtn = new GameObject("Replay Button");
            goReplayBtn.transform.SetParent(tutContainer, false);
            UnityEngine.UI.Image imgReplayBtn = goReplayBtn.AddComponent<UnityEngine.UI.Image>();
            RectTransform rtReplayBtn = imgReplayBtn.rectTransform;
            PushdownButton pbReplay = goReplayBtn.AddComponent<PushdownButton>();
            this.app.uiFactory.ApplyButtonStyle(pbReplay, imgReplayBtn, null);
            pbReplay.moveOnPress = new Vector3(0.0f, -5.0f);

            GameObject goReplayIco = new GameObject("ReplayIcon");
            goReplayIco.transform.SetParent(rtReplayBtn, false);
            UnityEngine.UI.Image imgReplayIco = goReplayIco.AddComponent<UnityEngine.UI.Image>();
            imgReplayIco.color = Color.black;
            imgReplayIco.sprite = this.app.replayExerciseIcon;
            Sprite srply = this.app.replayExerciseIcon;
            RectTransform rtReplayIco = imgReplayIco.rectTransform;
            rtReplayIco.pivot = new Vector2(0.5f, 0.5f);
            rtReplayIco.anchorMin = new Vector2(0.5f, 0.5f);
            rtReplayIco.anchorMax = new Vector2(0.5f, 0.5f);
            rtReplayIco.anchoredPosition = new Vector2(0.0f, 3.0f);
            rtReplayIco.sizeDelta = srply.rect.size;
            rtReplayIco.localScale = new Vector3(0.8f, 0.8f, 1.0f);

            if(displayMode == PaneKeyboard.ExerciseDisplayMode.KeyboardExercise)
            { 
                ret.hostRightOffset = 0.0f;
                const float border = 10.0f;
                ret.hostRightOffset = leaveButtonWidth + border;

                rtReplayBtn.anchorMin = new Vector2(1.0f, 0.0f);
                rtReplayBtn.anchorMax = new Vector2(1.0f, 0.0f);
                rtReplayBtn.pivot = new Vector2(1.0f, 0.0f);
                rtReplayBtn.sizeDelta = new Vector2(leaveButtonWidth, leaveButtonHeight);
                rtReplayBtn.anchoredPosition = new Vector2(-5.0f - ret.hostRightOffset, 10.0f);
            }
            else
            {

                rtReplayBtn.anchorMin = new Vector2(1.0f, 1.0f);
                rtReplayBtn.anchorMax = new Vector2(1.0f, 1.0f);
                rtReplayBtn.pivot = new Vector2(1.0f, 1.0f);
                rtReplayBtn.sizeDelta = new Vector2(leaveButtonWidth, leaveButtonHeight * 2.0f);
                rtReplayBtn.anchoredPosition = new Vector2(-5.0f, -10.0f);
            }

            ret.replayButton = pbReplay;
            ret.replayButtonRect = rtReplayBtn;
        }

        //
        //      STATS REGION
        //////////////////////////////////////////////////
        GameObject goStats = new GameObject("StatsRegion");
        goStats.transform.SetParent(tutContainer, false);
        RectTransform rtStats = goStats.AddComponent<RectTransform>();
        rtStats.pivot = new Vector2(1.0f, 0.0f);
        rtStats.anchorMin = new Vector2(1.0f, 0.0f);
        rtStats.anchorMax = new Vector2(1.0f, 1.0f);
        rtStats.offsetMin = new Vector2(-leaveButtonWidth - 160.0f, 10.0f);
        rtStats.offsetMax = new Vector2(-leaveButtonWidth - 10.0f, -10.0f);

        //
        //      CONTENT REGION
        //////////////////////////////////////////////////

        ret.leaveButtonRect = rtBtn;
        ret.leaveButton = pb;

        return ret;
    }

    protected void PopulatePropertyGrid(
        PxPre.UIL.Factory uifactory, 
        PxPre.UIL.ElePropGrid epg, 
        PxPre.UIL.EleBaseRect parent, 
        RectTransform rootPlate, 
        ExerciseAssets assets)
    {
        DoNothing dn = parent.RT.gameObject.AddComponent<DoNothing>();
        TweenUtil tweener = new TweenUtil(dn);

        for (int i = 0; i < this.exerciseParams.Count; ++i)
        {
            ExerciseEnumParam eep = this.exerciseParams[i];

            PxPre.UIL.EleGenButton<PushdownButton> btn = uifactory.CreateButton<PushdownButton>(parent, string.Empty);
            btn.Button.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);
            epg.Add(btn, 0.0f, PxPre.UIL.LFlag.GrowHoriz, eep.paramName);
            btn.border = new PxPre.UIL.PadRect(0.0f, 0.0f, 0.0f, 0.0f);

            PxPre.UIL.EleBoxSizer bsCon = uifactory.HorizontalSizer(btn);
            bsCon.border = new PxPre.UIL.PadRect(10.0f, 5.0f, 10.0f, 10.0f);
            PxPre.UIL.EleImg imgIco = uifactory.CreateImage(btn, eep.GetSprite());
            PxPre.UIL.EleText txtLabel = uifactory.CreateText(btn, eep.GetLabel(), false);
            PxPre.UIL.EleImg imgPulldown = uifactory.CreateImage(btn, assets.ellipses);
            txtLabel.text.alignment = TextAnchor.MiddleCenter;

            bsCon.Add(imgIco, 0.0f, PxPre.UIL.LFlag.AlignVertCenter);
            bsCon.Add(txtLabel, 1.0f, PxPre.UIL.LFlag.Grow | PxPre.UIL.LFlag.AlignCenter);
            bsCon.Add(imgPulldown, 0.0f, PxPre.UIL.LFlag.AlignVertCenter);

            btn.Button.onClick.AddListener(
                () =>
                {
                    PxPre.UIL.Dialog dlg = 
                        this.DoParamSelectionDialog(
                            uifactory,
                            eep,
                            imgIco,
                            txtLabel,
                            this.app,
                            tweener);

                    DoNothing dnEffect = dlg.host.RT.gameObject.AddComponent<DoNothing>();

                    dnEffect.StartCoroutine( 
                        this.app.TransitionReticule(
                            dlg.host.RT, 
                            btn.GetContentRect(), 
                            dlg.rootParent.RT, 
                            dlg.host.RT.gameObject.GetComponent<UnityEngine.UI.Graphic>() ));

                    dlg.onClose += 
                        ()=>
                        {
                            tweener.Host.StartCoroutine(
                                this.app.TransitionReticule(
                                    rootPlate,
                                    dlg.rootParent.RT,
                                    btn.GetContentRect(),
                                    null));
                        };
                });
        }
    }

    protected PxPre.UIL.Dialog DoParamSelectionDialog(
        PxPre.UIL.Factory uifactory, 
        ExerciseEnumParam eep, 
        PxPre.UIL.EleImg ico, 
        PxPre.UIL.EleText label, 
        Application app, 
        TweenUtil tweener)  // The RectTransform of the control (button) that spawned the UI
    {
        PxPre.UIL.Dialog dlg = app.dlgSpawner.CreateDialogTemplate(new Vector2(500.0f, -1.0f), PxPre.UIL.LFlag.AlignCenter, 1.0f);
        dlg.dialogSizer.border = new PxPre.UIL.PadRect(0.0f, 20.0f, 0.0f, 20.0f);

        dlg.AddDialogTemplateTitle( "Select: " + eep.paramName);

        PxPre.UIL.EleText txtDescr = uifactory.CreateText(dlg.rootParent, eep.description, true);
        dlg.contentSizer.Add(txtDescr, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

        PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> scrollRgn = 
            uifactory.CreateGenVerticalScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(dlg.rootParent);

        dlg.contentSizer.Add(scrollRgn, 1.0f, PxPre.UIL.LFlag.Grow);

        PxPre.UIL.EleBoxSizer bsScroll = uifactory.VerticalSizer(scrollRgn, "ExerciseScrollSizer");
        foreach (ExerciseEnumParam.Entry ee in eep.Entries())
        {
            PxPre.UIL.EleGenButton<PushdownButton> eleEntry = uifactory.CreateButton<PushdownButton>(scrollRgn, string.Empty);
            eleEntry.minSize = new Vector2(0.0f, 50.0f);
            eleEntry.Button.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);
            eleEntry.border = new PxPre.UIL.PadRect(0.0f);

            ExerciseEnumParam.Entry eeCpy = ee;
            eleEntry.Button.onClick.AddListener(
                () =>
                {
                    eep.SetInt(eeCpy.id);
                    ico.Img.sprite = eep.GetSprite();
                    label.text.text = eep.GetLabel();

                    tweener.SlidingAnchorFade(
                        ico.RT, 
                        null, 
                        new Vector2(30.0f, 0.0f), 
                        false, 
                        true, 
                        0.3f, 
                        TweenUtil.RestoreMode.RestoreLocal);

                    dlg.DestroyDialog();
                });

            bsScroll.Add(eleEntry, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

            // Add icon
            PxPre.UIL.EleBoxSizer bsMain = uifactory.HorizontalSizer(eleEntry);
            bsMain.Add(uifactory.CreateImage(eleEntry, ee.icon), 0.0f, PxPre.UIL.LFlag.AlignCenter);
            bsMain.border = new PxPre.UIL.PadRect(10.0f, 10.0f, 10.0f, 15.0f);
            bsMain.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);

            // Text Region
            PxPre.UIL.EleBoxSizer bsContent = uifactory.VerticalSizer(bsMain, 1.0f, PxPre.UIL.LFlag.Grow);

            // Title content
            PxPre.UIL.EleText txtTitle = uifactory.CreateText(eleEntry, ee.label, false);
            bsContent.Add(txtTitle, 1.0f, PxPre.UIL.LFlag.AlignCenter);
            txtTitle.text.fontSize = 30;

            bsContent.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);

            // Description
            PxPre.UIL.EleText txtEntryDescr = uifactory.CreateText(eleEntry, ee.description, true);
            bsContent.Add(txtEntryDescr, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

            if (ee.id == eep.GetInt())
                eleEntry.Button.targetGraphic.color = Color.green;
        }

        dlg.AddDialogTemplateSeparator();
        dlg.AddDialogTemplateButtons(new PxPre.UIL.DlgButtonPair("Cancel", null));

        dlg.host.LayoutInRTSmartFit();
        return dlg;
    }

    protected TweenUtil CreateTweener()
    {
        TweenUtil tweener = new TweenUtil(this.app);
        tweener.onEnded += (x) => { this.app.StopExerciseCoroutine(x.coroutine); };
        tweener.onStarted += (x) => { this.app.RecordExerciseCoroutine(x.coroutine); };

        return tweener;
    }

    protected IEnumerator TweenInQuizRecords(TweenUtil tweener, QuizProgressRecord correct, QuizProgressRecord incorrect)
    {
        incorrect.Reset();
        correct.Reset();
        incorrect.Deactivate();

        correct.Activate();
        tweener.ScaleIn(correct.plate.rectTransform, 0.5f);
        tweener.ScaleIn(correct.icon.rectTransform, 0.6f);
        tweener.ScaleIn(correct.text.rectTransform, 0.7f);
        yield return new WaitForSeconds(0.1f);

        incorrect.Activate();
        tweener.ScaleIn(incorrect.plate.rectTransform, 0.5f);
        tweener.ScaleIn(incorrect.icon.rectTransform, 0.6f);
        tweener.ScaleIn(incorrect.text.rectTransform, 0.7f);
    }

    protected IEnumerator ReadySetGo(TweenUtil tweener, float rightOffset)
    { 
        return ReadySetGo(tweener, "Starting", "Get Ready!", rightOffset);
    }

    protected IEnumerator ReadySetGo(TweenUtil tweener, string mainString, string substring, float rightOffset)
    {
        float startTime = Time.time;
        float RSGTime = 3.0f;

        const float defaultRightPadding = 120.0f;
        float rightPadding = defaultRightPadding + rightOffset;
        RectTransform rtTutCont = this.app.keyboardPane.tutorialContainer;

        // Create starting assets
        //////////////////////////////////////////////////
        // "Starting" text
        GameObject goStarting = new GameObject("Starting");
        goStarting.transform.SetParent(rtTutCont, false);
        UnityEngine.UI.Text txtStarting = goStarting.AddComponent<UnityEngine.UI.Text>();
        this.app.uiFactory.ApplyTextStyle(txtStarting);
        txtStarting.text = mainString;
        txtStarting.fontSize = 80;
        txtStarting.alignment = TextAnchor.MiddleCenter;
        txtStarting.horizontalOverflow = HorizontalWrapMode.Overflow;
        txtStarting.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rtStarting = txtStarting.rectTransform;
        rtStarting.anchorMin = new Vector2(0.0f, 0.0f);
        rtStarting.anchorMax = new Vector2(1.0f, 1.0f);
        rtStarting.pivot = new Vector2(0.5f, 0.5f);
        rtStarting.offsetMax = new Vector2(-rightPadding, 0.0f);
        rtStarting.offsetMin = new Vector2(0.0f, 60.0f);

        // Starting progress bar plate
        GameObject barPlate = new GameObject("Plate");
        barPlate.transform.SetParent(rtTutCont, false);
        UnityEngine.UI.Image imgPlate = barPlate.AddComponent<UnityEngine.UI.Image>();
        imgPlate.type = UnityEngine.UI.Image.Type.Sliced;
        imgPlate.sprite = this.app.exerciseAssets.plateRounder;
        imgPlate.color = new Color(1.0f, 0.5f, 0.25f);
        RectTransform rtBarPlate = imgPlate.rectTransform;
        rtBarPlate.anchorMin = new Vector2(0.0f, 0.0f);
        rtBarPlate.anchorMax = new Vector2(1.0f, 0.0f);
        rtBarPlate.offsetMax = new Vector2(-rightPadding, 40);
        rtBarPlate.offsetMin = new Vector2(0.0f, 0.0f);

        // progress bar
        GameObject barProg = new GameObject("Prog");
        barProg.transform.SetParent(rtBarPlate, false);
        UnityEngine.UI.Image imgProg = barProg.AddComponent<UnityEngine.UI.Image>();
        imgProg.color = new Color(0.25f, 1.0f, 0.5f);
        imgProg.type = UnityEngine.UI.Image.Type.Sliced;
        imgProg.sprite = this.app.exerciseAssets.plateRounder;
        RectTransform rtProg = imgProg.rectTransform;
        rtProg.pivot = new Vector2(0.5f, 0.5f);
        rtProg.anchorMin = new Vector2(0.0f, 0.0f);
        rtProg.anchorMax = new Vector2(0.0f, 1.0f);
        rtProg.sizeDelta = new Vector2(0.0f, 0.0f);
        rtProg.anchoredPosition = new Vector2(0.0f, 0.0f);

        //////////////////////////////////////////////////
        // Slide in "Starting"

        tweener.SlidingAnchorFade(rtStarting, txtStarting, new Vector2(50.0f, 0.0f), true, true, 0.3f);
        tweener.RectTransformLerpOffsets(rtProg, null, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), null, null, RSGTime);

        yield return new WaitForSeconds(1.0f);

        //////////////////////////////////////////////////
        // Slide in "GetReady"
        GameObject goGetReady = new GameObject("GetReady");
        goGetReady.transform.SetParent(rtTutCont, false);
        UnityEngine.UI.Text txtReady = goGetReady.AddComponent<UnityEngine.UI.Text>();
        this.app.uiFactory.ApplyTextStyle(txtReady);
        txtReady.text = substring;
        txtReady.fontSize = 20;
        txtReady.alignment = TextAnchor.MiddleCenter;
        txtReady.horizontalOverflow = HorizontalWrapMode.Overflow;
        txtReady.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rtReady = txtReady.rectTransform;
        rtReady.anchorMin = new Vector2(0.0f, 0.0f);
        rtReady.anchorMax = new Vector2(1.0f, 1.0f);
        rtReady.offsetMax = new Vector2(-rightPadding, -80);
        rtReady.offsetMin = new Vector2(0.0f, 40);

        tweener.SlidingAnchorFade(rtReady, txtReady, new Vector2(100.0f, 0.0f), true, true, 0.3f);

        yield return new WaitForSeconds(startTime + RSGTime - Time.time);

        // Fade out
        // We exit before we finish because we don't need to stay around any longer
        //////////////////////////////////////////////////

        const float fadeout = 0.3f;
        tweener.SlidingAnchorFade(rtStarting, txtStarting, new Vector2(100.0f, 0.0f), false, false, fadeout, TweenUtil.RestoreMode.Delete);
        tweener.SlidingAnchorFade(rtReady, txtReady, new Vector2(200.0f, 0.0f), false, false, fadeout, TweenUtil.RestoreMode.Delete);
        tweener.RectTransformLerpOffsets(rtBarPlate, null, new Vector2(1.0f, 0.0f), new Vector2(1.0f, 0.0f), null, null, fadeout, TweenUtil.RestoreMode.Delete);
        tweener.RectTransformLerpOffsets(rtProg, null, new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), null, null, fadeout, TweenUtil.RestoreMode.Delete);
    }

    public virtual Sprite[] GetTitlebarIcons()
    {
        List<Sprite> ret = new List<Sprite>();

        foreach (ExerciseEnumParam eep in this.ExerciseParams)
        {
            if (eep.titlebarIco == true)
                ret.Add(eep.GetSprite());
        }

        return ret.ToArray();
    }

    protected PxPre.UIL.EleHost CreateQuizRegion(float extraOffset = 0)
    {
        const float rightPadding = 120.0f;
        RectTransform rtTutCont = this.app.keyboardPane.tutorialContainer;

        GameObject goHost = new GameObject("QuizRegion");
        goHost.transform.SetParent(rtTutCont, false);

        RectTransform rtHost = goHost.AddComponent<RectTransform>();
        rtHost.anchorMin = new Vector2(0.0f, 0.0f);
        rtHost.anchorMax = new Vector2(1.0f, 1.0f);
        rtHost.pivot = new Vector2(0.0f, 1.0f);
        rtHost.offsetMin = new Vector2(0.0f, 0.0f);
        rtHost.offsetMax = new Vector2(-rightPadding + extraOffset, 0.0f);
        PxPre.UIL.EleHost ehost = new PxPre.UIL.EleHost(rtHost, true);

        return ehost;
    }

    public static List<int> GetRandomValues(int min, int max, int num)
    { 
        List<int> lst = new List<int>();
        for(int i = min; i < max; ++i)
            lst.Add(i);

        List<int> ret = new List<int>();

        while(lst.Count > 0 && ret.Count < num)
        { 
            int idx = Random.Range(0, lst.Count);
            ret.Add(lst[idx]);
            lst.RemoveAt(idx);
        }

        return ret;
    }

    public float GetQuestionExerciseTime()
    {
        switch (this.paramTimed.GetInt())
        {
            case 0:
                return 20.0f;

            case 1:
                return 10.0f;

            case 2:
                return 5.0f;
        }

        return float.PositiveInfinity;
    }

    public void SaveParams()
    { 
        foreach(ExerciseEnumParam eep in this.exerciseParams)
            eep.Save();
    }

    protected virtual int GetQuestionsCount()
    {
        switch (this.paramNumber.GetInt())
        {
            case 0:
                return 20;

            case 1:
                return 40;

            case 2:
                return 100;

            case -1:
                return int.MaxValue;
        }
        return 10;
    }

    protected virtual OctaveRange ? GetExerciseOctaveRange()
    { 
        return null;
    }

    protected virtual bool GetQuestionsShowAnswers()
    {
        return this.paramShowAnswers.GetInt() != 0;
    }

    protected IEnumerator PlayKeyEnum(PxPre.Phonics.WesternFreqUtils.Key k, int octave, float duration, List<int> handles)
    {
        int id = this.app.PressKey(k, octave, Application.NoteStartEvent.Started);
        handles.Add(id);
        yield return new WaitForSeconds(duration);
        this.StopKey(handles, id);
    }

    protected void StopKey(List<int> handles, int idx)
    {
        this.app.EndNote(idx, true);
        handles.Remove(idx);
    }

    protected void StopKeys(List<int> handles)
    {
        foreach (int i in handles)
            this.app.EndNote(i, true);

        handles.Clear();
    }

    protected IEnumerator MainExerciseCoroutine(int questions, bool showAnswers, float timer, TweenUtil tweener, ExerciseUtil exu)
    {
        //      ANIMATE IN
        ////////////////////////////////////////////////////////////////////////////////
        if (tutorialRegionObjs.replayButton != null)
            tutorialRegionObjs.replayButton.gameObject.SetActive(false);

        OctaveRange ? range = this.GetExerciseOctaveRange();
        if(range != null)
        {
            this.app.keyboardPane.PushKeyboardSetting(
                range.Value.min, 
                range.Value.max, 
                PxPre.Phonics.WesternFreqUtils.Key.C, 
                null);

            this.app.keyboardPane.SetOctaveBackground(
                KeyCollection.OctaveHighlighting.Black,
                false);
        }

        this.app.StartExerciseCoroutine(
            this.TweenInQuizRecords(
                tweener,
                this.app.keyboardPane.quizRecordCorrect,
                this.app.keyboardPane.quizRecordIncorrect));

        exu.UpdateQuestionsRegion(true);
        tweener.WobbleScale(titlebarObjs.progressText.rectTransform, 0.8f, 1.2f, 1.0f, 2.0f);
        this.app.keyboardPane.DisableAllKeys();

        Coroutine rsg = 
            this.app.StartExerciseCoroutine(
                this.ReadySetGo(
                    tweener, 
                    tutorialRegionObjs.hostRightOffset));

        yield return rsg;

        // EXERCISE IMPLEMENTATION
        ////////////////////////////////////////////////////////////////////////////////
        yield return ExerciseCoroutineImpl(questions, showAnswers, timer, tweener, exu, range);

        // ANIMATE OUT
        ////////////////////////////////////////////////////////////////////////////////
        if (tutorialRegionObjs.replayButton != null)
            tutorialRegionObjs.replayButton.gameObject.SetActive(false);

        pressedNoteCache.Clear();
        this.app.keyboardPane.DisableAllKeys();

        this.app.keyboardPane.PlayAudio_EndBell();
        yield return this.ReadySetGo(tweener, "Finished!", "Generating exercise results...", tutorialRegionObjs.hostRightOffset);
        this.app.StopExercise();

        // FINISH
        ////////////////////////////////////////////////////////////////////////////////
        this.app.SetTabFromType(Application.TabTypes.Options);

        this.app.sleepHoldMgr.StayAwakeFor(Application.sleepOffsetEndExercise);
        exu.DoResultsDialog();
        //yield return new WaitForSeconds(0.25f);
    }

    protected abstract IEnumerator ExerciseCoroutineImpl(
        int questions, 
        bool showAnswers, 
        float timer, 
        TweenUtil tweener, 
        ExerciseUtil exu, 
        OctaveRange? rang);
}
