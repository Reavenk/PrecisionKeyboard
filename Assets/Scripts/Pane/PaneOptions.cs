// <copyright file="PaneOptions.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The options panel for exercises and various control pane functionality.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaneOptions : 
    PaneBase,
    IExerciseParamProvider
{

    public enum ExerciseFilter
    { 
        ShowAll,
        ShowExercises,
        ShowNotes,
        EarTraining,
        Keyboard
    }

    // 
    //      PLAYER PREFERENCE VARIABLES
    //////////////////////////////////////////////////
    
    public const string prefKey_UIScale = "uiscale";    // PlayerPreferences ui scale value.
    public const string prefOpt_ContentFilter = "contFilter";   // The filter for listing the exercise and readable content

    public const string prefHapt_Key = "haptKey";
    public const string prefHapt_Btn = "haptBtn";

    public const string prefKey_FPSMeter = "showfps";       // If 0, start without showing FPS meter, else show.
    public const string prefKey_FPSScale = "fpsscale";      // The scale of the FPS meter

    public const string prefOpt_ExerVol = "exervol";
    public const string prefOpt_AskLink = "asklink";

    // 
    //      OPTIONS UI AND REFERENCES
    //////////////////////////////////////////////////

    const int minFPFont = 14;
    const int maxFPFont = 30;

    /// <summary>
    /// The CanvasScaler controlled by sliderUIScale.
    /// </summary>
    public UnityEngine.UI.CanvasScaler canvasScaler;

    /// <summary>
    /// The slider to control the UI scale.
    /// </summary>
    public UnityEngine.UI.Slider sliderUIScale;

    /// <summary>
    /// The slider to control the master volume of the app.
    /// </summary>
    public UnityEngine.UI.Slider sliderUIMaster;

    public RectTransform optionsScrollToHostTarget;
    public UnityEngine.UI.ScrollRect optionsScrollRect;

    // 
    //      EXERCISE CONTENT
    //////////////////////////////////////////////////
    
    /// <summary>
    /// The scrollable region where the exercises are listed.
    /// </summary>
    public RectTransform exerciseRegion;
    PxPre.UIL.EleHost exerciseHost;
    PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> exerciseScrollRgn = null;

    /// <summary>
    /// A list of all the exercises implemented into the app.
    /// </summary>
    List<BaseContent> exercises = new List<BaseContent>();


    public override Application.TabTypes TabType()
    {
        return Application.TabTypes.Options;
    }

    const int haptQuantFactor = 2;

    /// <summary>
    /// The min allowed UI scale factor.
    /// </summary>
    const float minScale = 1.5f;

    /// <summary>
    /// The max allowed UI scale factor.
    /// </summary>
    const float maxScale = 0.5f;

    public Sprite pushdownToggleUp;
    public Sprite pushdownToggleUpSel;
    public Sprite pushdownToggleDown;
    public Sprite pushdownToggleDownSel;
    public Sprite pushdownToggleTrans;

    public Sprite quizIcoInput;
    public Sprite quizIcoIdentify;
    public Sprite noteIco;

    public Sprite quizMicroWildcard;
    public Sprite quizMicroInput;
    public Sprite quizMicroIdentify;
    public Sprite quizMicroNotes;

    public Sprite iconExplanation;
    public Sprite iconCredits;
    public Sprite iconClearPrefs;

    public Sprite creditsLogo;

    public Sprite horizontalThumbArrows;

    public UnityEngine.UI.Image exerciseFilterIcon;
    public UnityEngine.UI.Text exerciseFilterText;
    public RectTransform rtExerciseFilter;


    ExerciseEnumParam paramNumber;
    ExerciseEnumParam paramTimed;
    ExerciseEnumParam paramShowAnswers;
    ExerciseEnumParam paramIntervalLabel;
    ExerciseEnumParam paramBreak;
    ExerciseEnumParam paramDir;

    public ExerciseFilter exerciseFilter = ExerciseFilter.ShowAll;

    bool initialized = false;

    UnityEngine.UI.Toggle toggleFPSMeter;
    UnityEngine.UI.Slider sliderFPSScale;
    UnityEngine.UI.Slider sliderTabHeight;
    UnityEngine.UI.Toggle toggleAskBeforeLink;
    UnityEngine.UI.Slider sliderHapticKeys;
    UnityEngine.UI.Slider sliderHapticButton;
    UnityEngine.UI.Slider sliderExerciseVolume;
    UnityEngine.UI.Slider sliderMetronomeVolume;

    bool askBeforeOpeningLink = true;
    public bool AskBeforeOpeningLink() => this.askBeforeOpeningLink;

    // Cache the intro, because it's used outside the options...
    //
    // It's also shown automatically by the app the first time the user opens the app.
    public NoteContentResource introContent;

    public override void InitPane(Application app)
    {
        base.InitPane(app);

        float canvasScale = 1.0f;
        if(PlayerPrefs.HasKey(prefKey_UIScale) == true)
            canvasScale = PlayerPrefs.GetFloat(prefKey_UIScale, 1.0f);
        else
        { 
            float predSlider = PredictBestScale();
            // This is not reversed, maxScale should come first - this is because these
            // const names are somewhat a pejorative.
            //
            // It's a little weird that we lerp, just to eventually invlerp back to the 
            // slider value , but the first time detection is somewhat a special occasion 
            // so a little oddity and overhead is accepted.
            canvasScale = Mathf.Lerp(maxScale, minScale, predSlider);
        }
        canvasScale = Mathf.Clamp(canvasScale, maxScale, minScale);
        this.SetUIScale(canvasScale);
        this.sliderUIScale.value = Mathf.InverseLerp(maxScale, minScale, canvasScale);

        this.sliderUIMaster.value = this.App.MasterVolume();

        PxPre.UIL.EleHost host = new PxPre.UIL.EleHost(this.optionsScrollToHostTarget, true);
        PxPre.UIL.UILStack uiOptStk = new PxPre.UIL.UILStack(this.App.uiFactory, host);
        uiOptStk.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);
        this.optionsScrollRect = uiOptStk.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(1.0f, PxPre.UIL.LFlag.GrowVertOnCollapse|PxPre.UIL.LFlag.GrowHoriz).ScrollRect;
            uiOptStk.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(0.0f, 0.0f, 5.0f, 0.0f);
                PxPre.UIL.ElePropGrid epg = uiOptStk.PushPropGrid(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                epg.border.Set(0.0f);

                    PxPre.UIL.EleGenToggle<PushdownToggle> ptShowFPS = uiOptStk.CreateToggle<PushdownToggle>(null);
                    this.SetupNewPushdownToggle(ptShowFPS.toggle);
                    this.toggleFPSMeter = ptShowFPS.toggle;
                    epg.Add(ptShowFPS, 0.0f, PxPre.UIL.LFlag.GrowHoriz, "FPS Meter");

                    PxPre.UIL.EleGenSlider<PushdownSlider> psFPSScale = uiOptStk.CreateHorizontalSlider<PushdownSlider>();
                    this.sliderFPSScale = psFPSScale.Slider;
                    epg.Add(psFPSScale, 0.0f, PxPre.UIL.LFlag.GrowHoriz, "FPS Scale");

                    PxPre.UIL.EleGenSlider<PushdownSlider> psTabHeight = uiOptStk.CreateHorizontalSlider<PushdownSlider>();
                    this.sliderTabHeight = psTabHeight.Slider;
                    epg.Add(psTabHeight, 0.0f, PxPre.UIL.LFlag.GrowHoriz, "Tab Height");

                    PxPre.UIL.EleGenSlider<PushdownSlider> psExerVol = uiOptStk.CreateHorizontalSlider<PushdownSlider>();
                    this.sliderExerciseVolume = psExerVol.Slider;
                    epg.Add(psExerVol, 0.0f, PxPre.UIL.LFlag.GrowHoriz, "Exer. Vol");

                    PxPre.UIL.EleGenSlider<PushdownSlider> psMetroVol = uiOptStk.CreateHorizontalSlider<PushdownSlider>();
                    this.sliderMetronomeVolume = psMetroVol.Slider;
                    epg.Add(psMetroVol, 0.0f, PxPre.UIL.LFlag.GrowHoriz, "Metro Vol");

                    PxPre.UIL.EleGenToggle<PushdownToggle> ptAskLink = uiOptStk.CreateToggle<PushdownToggle>("Ask Before Opening");
                    this.SetupNewPushdownToggle(ptAskLink.toggle);
                    this.toggleAskBeforeLink = ptAskLink.toggle;
                    epg.Add(ptAskLink, 0.0f, PxPre.UIL.LFlag.GrowHoriz, "Ask Link");

                if(Vibrator.HasVibrator() == true)
                {
                    epg.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
                    epg.Add(uiOptStk.CreateText("Haptic vibration power for piano key", 16, true), 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                    PxPre.UIL.EleGenSlider<PushdownSlider> psKeyVib = uiOptStk.CreateHorizontalSlider<PushdownSlider>();
                    this.sliderHapticKeys = psKeyVib.Slider;
                    epg.Add(psKeyVib, 0.0f, PxPre.UIL.LFlag.GrowHoriz, "Key Hapt");

                    epg.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
                    epg.Add(uiOptStk.CreateText("Haptic vibration power for UI buttons", 16, true), 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                    PxPre.UIL.EleGenSlider<PushdownSlider> psButtonVib = uiOptStk.CreateHorizontalSlider<PushdownSlider>();
                    this.sliderHapticButton = psButtonVib.Slider;
                    epg.Add(psButtonVib, 0.0f, PxPre.UIL.LFlag.GrowHoriz, "Btn Hapt");
                }
                uiOptStk.Pop();

                uiOptStk.AddVertSpace(50.0f, 0.0f, 0);

                PxPre.UIL.EleGenButton<PushdownButton> explBtn = AddOptionsButtion(uiOptStk, this.iconExplanation, "Explanation of Options", null);
                explBtn.Button.onClick.AddListener( 
                    ()=>
                    { 
                        this.App.DoVibrateButton();
                        this.ShowExplainOptionsButton(explBtn.RT); 
                    });

                uiOptStk.AddSpace(5.0f, 0.0f, 0);
                PxPre.UIL.EleGenButton<PushdownButton> creditsBtn = AddOptionsButtion(uiOptStk, this.iconCredits, "Credits", null );
                creditsBtn.Button.onClick.AddListener(
                    ()=>
                    { 
                        this.App.DoVibrateButton();
                        this.ShowCreditsButton(creditsBtn.RT); 
                    });
                

                uiOptStk.AddSpace(5.0f, 0.0f, 0);
                PxPre.UIL.EleGenButton<PushdownButton> clearPrefBtn =  AddOptionsButtion(uiOptStk, this.iconClearPrefs, "Clear Prefs", null ); 
                clearPrefBtn.Button.onClick.AddListener(
                    ()=>
                    { 
                        this.App.DoVibrateButton();
                        this.ShowClearPrefsQuestion(clearPrefBtn.RT); 
                    });

#if !DEPLOYMENT
                    uiOptStk.AddSpace(5.0f, 0.0f, 0);
                    PxPre.UIL.EleGenButton<PushdownButton> diagScrnBtn = uiOptStk.AddButton<PushdownButton>("Diag Screen", 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                    diagScrnBtn.Button.onClick.AddListener(()=>{ UnityEngine.SceneManagement.SceneManager.LoadScene("Diagnostic"); });
#endif

                uiOptStk.AddVertSpace(50.0f, 0.0f, 0);
                uiOptStk.AddText("Version " + Application.VersionString, false, 0.0f, 0);
                uiOptStk.AddText("Build " + Application.BuildString, false, 0.0f, 0);
                uiOptStk.AddText("Build Date " + Application.BuildDate, false, 0.0f, 0);
                uiOptStk.AddSpace(20.0f, 0.0f, 0);
                uiOptStk.AddText($"<i>{Application.GetRandomQuote()}</i>", 16, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiOptStk.AddVertSpace(0.0f, 1.0f, 0); // If there's any space left, it goes in the bottom
        host.LayoutInRTSmartFit();

        // The top just sets up the layout, more code to actually hook up the 
        // UI and apply additional settings.

        this.App.StartCoroutine(PxPre.UIL.EleVertScrollRgn.SetVertScrollLater(this.optionsScrollRect, 1.0f));

        //      SETUP TOGGLE FPS
        //////////////////////////////////////////////////
        bool showFPS = PlayerPrefs.GetInt(prefKey_FPSMeter, 0) != 0;
        if(showFPS == false)
            this.fpsCounter.gameObject.SetActive(false);

        this.toggleFPSMeter.isOn = showFPS;
        // // Set is as false first, we have issues turning it on at initialization
        // // if it's not set to be off first.
        this.toggleFPSMeter.onValueChanged.AddListener( this.OnToggle_ToggleFPS );

        //      SETUP FPS SCALE SLIDER
        //////////////////////////////////////////////////
        float fpsScale = PlayerPrefs.GetFloat(prefKey_FPSScale, 1.0f);
        this.SetFPSPercent(fpsScale);
        this.sliderFPSScale.value = this.GetFPSPercent();
        this.sliderFPSScale.onValueChanged.AddListener( this.OnSlider_FPSScale);

        //      SETUP EXERCISE VOLUME SLIDER
        //////////////////////////////////////////////////
        float exrVol = this.App.keyboardPane.GetExerciseVolume();
        exrVol = PlayerPrefs.GetFloat(prefOpt_ExerVol, exrVol);
        this.sliderExerciseVolume.normalizedValue = exrVol;
        this.App.keyboardPane.SetExerciseVolume(exrVol);
        this.sliderExerciseVolume.onValueChanged.AddListener( 
            (x)=>
            { 
                PlayerPrefs.SetFloat(prefOpt_ExerVol, x);
                this.App.keyboardPane.SetExerciseVolume(x);
            });

        //      SETUP METRONOME VOLUME SLIDER
        //////////////////////////////////////////////////
        this.sliderMetronomeVolume.value = this.App.GetMetronomeVolume();
        this.sliderMetronomeVolume.onValueChanged.AddListener(
            (x)=>
            { 
                this.App.SetMetronomeVolume(x);
            });

        //      SETUP ASK LINK TOGGLE
        //////////////////////////////////////////////////
        this.askBeforeOpeningLink = PlayerPrefs.GetInt( prefOpt_AskLink, this.askBeforeOpeningLink ? 1 : 0) != 0;
        this.toggleAskBeforeLink.isOn = this.askBeforeOpeningLink;
        this.toggleAskBeforeLink.onValueChanged.AddListener(this.OnToggle_AskBeforeLink);

        if(Vibrator.HasVibrator() == true)
        {
            //      SETUP HAPTIC KEY SLIDER
            //////////////////////////////////////////////////
            this.sliderHapticKeys.minValue = 0.0f;
            this.sliderHapticKeys.maxValue = Application.MaxHapticAmount/ haptQuantFactor;
            this.sliderHapticKeys.wholeNumbers = true;
            this.sliderHapticKeys.value = this.App.GetVibrateKeysMS() / haptQuantFactor;
            int hkey = PlayerPrefs.GetInt(prefHapt_Key, this.App.GetVibrateKeysMS());
            this.App.SetVibrateKeysMS(hkey);
            this.sliderHapticKeys.onValueChanged.AddListener(this.OnSlider_HapticKeys);

            //      SETUP HAPTIC BUTTON SLIDER
            //////////////////////////////////////////////////
       
            this.sliderHapticButton.minValue = 0.0f;
            this.sliderHapticButton.maxValue = Application.MaxHapticAmount/ haptQuantFactor;
            this.sliderHapticButton.wholeNumbers = true;
            this.sliderHapticButton.value = this.App.GetVibrateButtonsMS() / haptQuantFactor;
            int hbtn = PlayerPrefs.GetInt(prefHapt_Btn, this.App.GetVibrateButtonsMS());
            this.App.SetVibrateButtonsMS(hbtn);
            this.sliderHapticButton.onValueChanged.AddListener(this.OnSlider_HapticButtons);
        }

        //      SETUP TAB HEIGHT PERCENT
        //////////////////////////////////////////////////
        this.sliderTabHeight.value = this.App.GetTabHeightPercent();
        this.sliderTabHeight.onValueChanged.AddListener( this.OnSlider_TabHeight);

        this.InitializeExercises();

        int filter = PlayerPrefs.GetInt(prefOpt_ContentFilter, (int)ExerciseFilter.ShowAll);
        switch(filter)
        { 
            default:
            case (int)ExerciseFilter.ShowAll:
                this.SetExerciseFilter(ExerciseFilter.ShowAll, true);
                break;

            case (int)ExerciseFilter.ShowExercises:
                this.SetExerciseFilter(ExerciseFilter.ShowExercises, true);
                break;

            case (int)ExerciseFilter.ShowNotes:
                this.SetExerciseFilter(ExerciseFilter.ShowNotes, true);
                break;

            case (int)ExerciseFilter.EarTraining:
                this.SetExerciseFilter(ExerciseFilter.EarTraining, true);
                break;

            case (int)ExerciseFilter.Keyboard:
                this.SetExerciseFilter(ExerciseFilter.Keyboard, true);
                break;
        }

        this.App.StartCoroutine(this.exerciseScrollRgn.SetVertScrollLater(1.0f));

        this.initialized = true;
    }

    static PxPre.UIL.EleGenButton<PushdownButton> AddOptionsButtion(PxPre.UIL.UILStack uiStack, Sprite icon, string label, UnityEngine.Events.UnityAction action)
    { 
        PxPre.UIL.PadRect optBtnPadding = new PxPre.UIL.PadRect(10.0f, 10.0f, 10.0f, 15.0f);

        PxPre.UIL.EleGenButton<PushdownButton> btn = uiStack.PushButton<PushdownButton>("", 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        btn.border.Set(0);
        uiStack.PushHorizSizer().border = optBtnPadding;
            uiStack.AddImage(icon, 0.0f, PxPre.UIL.LFlag.AlignCenter);
            uiStack.AddSpace(10.0f, 0.0f, 0);
            uiStack.AddText(label, true, 1.0f, PxPre.UIL.LFlag.Grow).Chn_TextAlignment(TextAnchor.MiddleCenter);
        uiStack.Pop();
        uiStack.Pop();
        btn.Button.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);

        if(action != null)
        {
            btn.Button.onClick.AddListener(action);
        }

        return btn;
    }

    public void OnToggle_AskBeforeLink(bool b)
    { 
        PlayerPrefs.SetInt(prefOpt_AskLink, b == true ? 1 : 0);
        this.askBeforeOpeningLink = b;
        this.toggleAskBeforeLink.isOn = b;
    }

    public void TickFPS(float deltaTime)
    {
        this.fpsaccum *= 0.5f;
        this.fpswt *= 0.5f;
        this.fpsaccum += 1.0f / deltaTime;
        this.fpswt += 1.0f;
        double fps = fpsaccum / this.fpswt;

        this.fpsCounter.text = fps.ToString("0.000");
    }

    public void SetHapticSliderButtons(int value)
    { 
        PlayerPrefs.SetInt(prefHapt_Btn, value);
        this.sliderHapticButton.value = value / haptQuantFactor;
    }

    public void SetHapticSliderKeys(int value)
    {
        PlayerPrefs.SetInt(prefHapt_Key, value);
        this.sliderHapticKeys.value = value / haptQuantFactor;
    }

    public void OnSlider_HapticButtons(float value)
    { 
        if(this.App.SetVibrateButtonsMS((int)value * haptQuantFactor) == true)
            this.App.DoVibrateButton();
    }

    public void OnSlider_HapticKeys(float value)
    {
        if (this.App.SetVibrateKeysMS((int)value * haptQuantFactor) == true)
            this.App.DoVibrateKeyboard();
    }

  
    public void OnSlider_FPSScale(float value)
    {
        this.SetFPSPercent(this.sliderFPSScale.value);
    
        if (initialized == true)
        {
            bool fpsShown = this.ShowingFPSMeter();
            this.SetFPSMeter(true, true, !fpsShown);
        }
    }

    /// <summary>
    /// The text display for the FPS meter.
    /// </summary>
    public UnityEngine.UI.Text fpsCounter;

    double fpsaccum = 0.0;
    double fpswt = 0.0f;
    
    public void OnSlider_TabHeight(float v)
    {
        if (this.sliderTabHeight == null)
            return;

        //////////////////////////////////////////////////
        // See SetUIScale() for more information in this
        //
        float initialScroll = 0.0f;
        bool showingScroll = true;
        if(this.optionsScrollRect != null)
        {
            if(this.optionsScrollRect.viewport.rect.height >= this.optionsScrollRect.content.rect.height)
                showingScroll = false;

            initialScroll = this.optionsScrollRect.verticalNormalizedPosition;
        }
        //////////////////////////////////////////////////

    
        // The actual meat of the function instead of 2/3rd of this function to preserve scrollbar values
        this.App.SetTabHeightPercent(v);

        //////////////////////////////////////////////////
        // Continuing from before
        if(this.optionsScrollRect != null)
        {
            //It's got a bad habit of starting the scroll at the bottom
            // if it' just poped in.
            if(showingScroll == false)
                initialScroll = 1.0f;

            this.App.StartCoroutine(
                PxPre.UIL.EleVertScrollRgn.SetVertScrollLater(
                    this.optionsScrollRect, 
                    initialScroll));
        }
    }

    void OnToggle_ToggleFPS(bool b)
    {
        this.SetFPSMeter(b, true, true);
    }
    
    public bool ShowingFPSMeter()
    {
        return this.toggleFPSMeter.IsActive();
    }

    public float GetFPSFontSize()
    {
        return this.fpsCounter.fontSize;
    }

    public float GetFPSPercent()
    {
        return Mathf.InverseLerp(minFPFont, maxFPFont, this.GetFPSFontSize());
    }

    public void SetFPSPercent(float per)
    {
        float newSize = Mathf.Lerp(minFPFont, maxFPFont, per);
        this.fpsCounter.fontSize = (int)newSize;
        PlayerPrefs.SetFloat(prefKey_FPSScale, per);
    }

    public void SetFPSMeter(bool value, bool savePref, bool strobe)
    {
        this.toggleFPSMeter.isOn = value;
        this.fpsCounter.gameObject.SetActive(value);
    
        if (savePref == true)
            PlayerPrefs.SetInt(prefKey_FPSMeter, value ? 1 : 0);
    
        if (value == true && strobe == true)
            this.fpsCounter.StartCoroutine(AnimateFPSOnEnum(this.fpsCounter));
    }

    IEnumerator AnimateFPSOnEnum(UnityEngine.UI.Graphic g)
    {
        const float time = 1.0f;
        const float ping = 10.0f;
        Color cAlert = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        Color cFinal = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        for (float f = 0.0f; f < time; f += Time.deltaTime)
        {
            float sz = Mathf.Cos(f * ping);
            float lam = 1.0f - f / time;

            g.color = Color.Lerp(cAlert, cFinal, sz * 0.5f + 0.5f);
            float finSz = 1.0f + sz * lam;
            g.transform.localScale = Vector3.one * finSz;

            yield return null;
        }
        g.transform.localScale = Vector3.one;
        g.color = cFinal;
    }

    public void InitSharedExerciseParameters()
    {
        ExerciseAssets assets = this.App.exerciseAssets;

        this.paramNumber =
            new ExerciseEnumParam(
                "Drill Ct",
                "How many questions do you want to quiz yourself?",
                "EXR_Gen_ParamNum",
                true,
                new ExerciseEnumParam.Entry(0, assets.quantityLow, "20 Questions", "The exercise will have twenty questions."),
                new ExerciseEnumParam.Entry(1, assets.quantityMed, "40 Questions", "The exercise will have fourty questions."),
                new ExerciseEnumParam.Entry(2, assets.quantityHigh, "100 Questions", "The exercise will have a \"whoppin' hundo\" questions.")
#if PREMIUM
                ,new ExerciseEnumParam.Entry(-1, assets.infinity, "Endless", "Non-stop quiz.\n<i>Drill, baby; Drill!</i>")
#endif
                );

        this.paramTimed =
            new ExerciseEnumParam(
                "Timed",
                "Do you want to limit the amount of time to answer the question to challenge yourself?",
                "EXR_Gen_ParamTime",
                false,
                new ExerciseEnumParam.Entry(-1, assets.clockOff, "Untimed", "No sweat! Take all the time you need."),
                new ExerciseEnumParam.Entry(0, assets.clockLong, "20 Seconds", "Thirty seconds to answer."),
                new ExerciseEnumParam.Entry(1, assets.clockMed, "10 Seconds", "Twenty seconds to answer."),
                new ExerciseEnumParam.Entry(2, assets.clockShort, "5 Seconds", "Five seconds to answer."));

        this.paramShowAnswers =
            new ExerciseEnumParam(
                "Reveal",
                "Do you want to see the correct answer if you get a question wrong?",
                "EXR_Gen_ShowAnswer",
                true,
                new ExerciseEnumParam.Entry(1, assets.showAnswerOn, "Show", "If you answer the question incorrectly, the correct answer will be shown"),
                new ExerciseEnumParam.Entry(0, assets.showAnswerOff, "Don't Show", "If you answer the question incorrectly, DON'T show the correct answer."));

        this.paramIntervalLabel =
            new ExerciseEnumParam(
                "Label",
                "What kind of interval labeling do you want to use?",
                "EXR_Gen_Label",
                true,
                new ExerciseEnumParam.Entry(0, assets.intervalNum, "Number", "The number difference."),
                new ExerciseEnumParam.Entry(1, assets.intervalMinMaj, "Major Minor", "The interval quality in terms of perfect, major and minor."),
                new ExerciseEnumParam.Entry(2, assets.intervalAugDim, "Aug Dim", "The interval quality in terms of augmented and dimininished."),
                new ExerciseEnumParam.Entry(3, assets.intervalWildcard, "Wildcard", "Any type of interval label. Includes shorthand notations."));

        this.paramBreak =
            new ExerciseEnumParam(
                "Break",
                "How are the multiple notes played?",
                "EXR_Gen_Break",
                true,
                new ExerciseEnumParam.Entry(0, assets.breakBroken, "Broken", "The notes are played one at a time in quick succession."),
                new ExerciseEnumParam.Entry(1, assets.breakOverlap, "Overlap", "The notes are played on top of each other."));

        this.paramDir =
            new ExerciseEnumParam(
                "Direction",
                "Direction of the scale",
                "EXR_Gen_ScaleDir",
                true,
                new ExerciseEnumParam.Entry(0, assets.ascending, "Ascending", "Up direction."),
                new ExerciseEnumParam.Entry(1, assets.descending, "Descending", "Down direction."),
                new ExerciseEnumParam.Entry(2, assets.bidirectional, "Random", "Either direction."));
    }

    public void InitializeExercises()
    {
        this.InitSharedExerciseParameters();

        // Populate the exercise library
        this.introContent = new NoteContentResource("Introduction", "introduction", "Introduction to the application.", false);
        this.exercises.Add(this.introContent);
        //
        //this.exercises.Add(new NoteContentResource("Keyboard Tab", "keyboardnotes", "Information about the keyboard tab.", false));
        //this.exercises.Add(new NoteContentResource("Wiring Tab", "wiringnotes", "Information about the wiring tab.", false));
        //this.exercises.Add(new NoteContentResource("Options Tab", "optionsnotes", "Information about the options tab.", false));
        this.exercises.Add(new NoteContentHelpdoc<HelpdocKeyboard>( this.App,   "Keyboard Tab", "Information about the keyboard tab.", true));
        this.exercises.Add(new NoteContentHelpdoc<HelpdocWiring>(   this.App,   "Wiring Tab", "Information about the wiring tab.", true));
        this.exercises.Add(new NoteContentHelpdoc<HelpdocOptions>(  this.App,   "Options Tab", "Information about the options tab.", true));
        //
        this.exercises.Add(new ExerciseInputKey());
        this.exercises.Add(new ExerciseIdentifyInterval());
        this.exercises.Add(new ExerciseInputInterval());
        this.exercises.Add(new ExerciseIdentifyChord());
        this.exercises.Add(new ExerciseInputChord());
        this.exercises.Add(new ExerciseIdentifyScale());
        this.exercises.Add(new ExerciseInputScale());
        this.exercises.Add(new ExerciseInputAbsolute());
        //
        this.exercises.Add(new NoteContentResource("Performance", "performancenotes", "Information about application and audio performance.", true));
        this.exercises.Add(new NoteContentResource("FAQ", "faq", "Misc information in Q&A form.", true));

        foreach (BaseContent ba in this.exercises)
        {
            ba.Initialize(this.App, this.App.exerciseAssets, this);
        }

        // LOAD PREVIOUSLY SAVED VALUES
        HashSet<ExerciseEnumParam> hsEEP = new HashSet<ExerciseEnumParam>();
        foreach(BaseContent bc in this.exercises)
        {
            BaseExercise be = bc as BaseExercise;
            if(be == null)
                continue;

            foreach(ExerciseEnumParam eep in be.ExerciseParams)
                hsEEP.Add(eep);
        }
        foreach(ExerciseEnumParam eep in hsEEP)
            eep.Load();
    }

    public void SetExerciseFilter(ExerciseFilter ef, bool force = false)
    { 
        if(ef == this.exerciseFilter && force == false)
            return;

        this.exerciseFilter = ef;

        Sprite newMicroIcon = null;
        switch (ef)
        { 
            case ExerciseFilter.Keyboard:
                newMicroIcon = this.quizMicroInput;
                this.exerciseFilterText.text = "Keyboard Exercises";
                break;

            case ExerciseFilter.EarTraining:
                newMicroIcon = this.quizMicroIdentify;
                this.exerciseFilterText.text = "Ear Training Exercises";
                break;

            case ExerciseFilter.ShowNotes:
                newMicroIcon = this.quizMicroNotes;
                this.exerciseFilterText.text = "Show Notes";
                break;

            case ExerciseFilter.ShowExercises:
                newMicroIcon = this.quizMicroWildcard;
                this.exerciseFilterText.text = "Show Exercises";
                break;

            case ExerciseFilter.ShowAll:
                newMicroIcon = this.quizMicroWildcard;
                this.exerciseFilterText.text = "Show All Exercises";
                break;
        }

        this.App.DoDropdownTextUpdate(this.exerciseFilterText);

        PlayerPrefs.SetInt(prefOpt_ContentFilter, (int)ef);


        this.UpdateExerciseFilter();

        this.exerciseFilterIcon.rectTransform.sizeDelta = newMicroIcon.rect.size;
        this.App.DoDropdownIconUpdate(this.exerciseFilterIcon, newMicroIcon);
    }

    public void OnButton_ExerciseFilter()
    { 
        PxPre.DropMenu.StackUtil menuStack = new PxPre.DropMenu.StackUtil();

        PxPre.DropMenu.Props dropProp = 
            PxPre.DropMenu.DropMenuSingleton.MenuInst.props;

        Color cS = dropProp.selectedColor;
        Color cU = dropProp.unselectedColor;

        menuStack.AddAction(
            this.quizMicroWildcard,
            (this.exerciseFilter == ExerciseFilter.ShowAll) ? cS : cU,
            "Show All",
            () => { this.SetExerciseFilter(ExerciseFilter.ShowAll); });

        menuStack.AddAction(
            this.quizMicroWildcard,
            (this.exerciseFilter == ExerciseFilter.ShowExercises) ? cS : cU,
            "Show All Exercises",
            () => { this.SetExerciseFilter(ExerciseFilter.ShowExercises); });

        menuStack.AddAction(
            this.quizMicroIdentify,
            (this.exerciseFilter == ExerciseFilter.EarTraining) ? cS : cU,
            "Ear Training Exercises",
            () => { this.SetExerciseFilter(ExerciseFilter.EarTraining); });

        menuStack.AddAction(
            this.quizMicroInput,
            (this.exerciseFilter == ExerciseFilter.Keyboard) ? cS : cU,
            "Keyboard Exercises",
            () => { this.SetExerciseFilter(ExerciseFilter.Keyboard); });

        menuStack.AddAction(
            this.quizMicroNotes,
            (this.exerciseFilter == ExerciseFilter.ShowNotes) ? cS : cU,
            "Notes",
            () => { this.SetExerciseFilter(ExerciseFilter.ShowNotes); });

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            menuStack.Root,
            this.rtExerciseFilter);

        this.App.DoVibrateButton();
    }

    public void UpdateExerciseFilter()
    { 
        if(this.exerciseHost == null)
        {
            this.exerciseHost = new PxPre.UIL.EleHost(this.exerciseRegion, true);
            PxPre.UIL.EleBoxSizer ebs = this.App.uiFactory.VerticalSizer(this.exerciseHost);

            this.exerciseScrollRgn = this.App.uiFactory.CreateGenVerticalScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(this.exerciseHost);
            ebs.Add(this.exerciseScrollRgn, 1.0f, PxPre.UIL.LFlag.Grow);
        }

        this.exerciseScrollRgn.SetSizer( null);
        PxPre.UIL.UILStack uiStack =  new PxPre.UIL.UILStack(this.App.uiFactory, this.exerciseScrollRgn);
        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.GrowHoriz);

        foreach (Transform t in this.exerciseScrollRgn.GetContentRect())
            GameObject.Destroy(t.gameObject);

        foreach(BaseContent bc in this.exercises)
        {
            BaseExercise.ContentType et = bc.GetExerciseType();
            if(this.exerciseFilter != ExerciseFilter.ShowAll)
            { 
                switch(this.exerciseFilter)
                { 
                    case ExerciseFilter.EarTraining:
                        if(et != BaseExercise.ContentType.EarTraining)
                            continue;
                        break;

                    case ExerciseFilter.Keyboard:
                        if(et != BaseExercise.ContentType.KeyboardQuiz)
                            continue;
                        break;

                    case ExerciseFilter.ShowExercises:
                        if (et != BaseExercise.ContentType.KeyboardQuiz && et != BaseExercise.ContentType.EarTraining)
                            continue;
                        break;

                    case ExerciseFilter.ShowNotes:
                        if( et != BaseContent.ContentType.Document)
                            continue;
                        break;
                }
            }

            PxPre.UIL.EleGenButton<PushdownButton> exbtn = uiStack.PushButton<PushdownButton>("", 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            exbtn.border = new PxPre.UIL.PadRect(0);
            exbtn.Button.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);
            PxPre.UIL.EleBoxSizer btnSzr = uiStack.PushHorizSizer();
            btnSzr.border = new PxPre.UIL.PadRect(10.0f, 10.0f, 10.0f, 15.0f);
                
                BaseExercise.ContentIcon icoType = bc.GetExerciseIcon();
                switch(icoType)
                { 
                    case BaseExercise.ContentIcon.Quiz:
                        uiStack.AddImage(this.quizIcoIdentify, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                        break;

                    case BaseExercise.ContentIcon.Keyboard:
                        uiStack.AddImage(this.quizIcoInput, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                        break;

                    case BaseExercise.ContentIcon.Notepad:
                        uiStack.AddImage(this.noteIco, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                        break;
            }

                uiStack.AddSpace(10.0f, 0.0f, 0);

                uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.AlignCenter|PxPre.UIL.LFlag.GrowHoriz);
                    uiStack.AddSpace(0.0f, 1.0f, 0);
                    uiStack.AddText(bc.title, 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                    uiStack.AddSpace(10.0f, 0.0f, 0);
                    uiStack.AddText(bc.description, true, 1.0f, PxPre.UIL.LFlag.AlignCenter|PxPre.UIL.LFlag.GrowHoriz);
                    uiStack.AddSpace(0.0f, 1.0f, 0);
                uiStack.Pop();

            uiStack.Pop();
            uiStack.Pop();

            BaseExercise baCpy = bc as BaseExercise;
            if(baCpy != null)
            {
                exbtn.Button.onClick.AddListener(
                    ()=>
                    { 
                        PxPre.UIL.Dialog dlg = baCpy.CreateDialog(this.App.exerciseAssets); 
                        dlg.onClose += ()=>{ baCpy.SaveParams(); };
            
                        this.App.DoVibrateButton();
                        this.App.SetupDialogForTransition(dlg, exbtn.RT);
                    });
            }
            else
            {
                BaseContent bcCpy = bc;

                exbtn.Button.onClick.AddListener(
                    () =>
                    {
                        PxPre.UIL.Dialog dlg = bcCpy.CreateDialog(this.App.exerciseAssets);

                        this.App.DoVibrateButton();
                        this.App.SetupDialogForTransition(dlg, exbtn.RT);
                    });
            }
        }
        

        this.exerciseHost.LayoutInRT(true);

        this.exerciseScrollRgn.ScrollRect.verticalNormalizedPosition = 1.0f;
        //this.StartCoroutine( PxPre.UIL.EleVertScrollRgn.SetVertScrollLater(
        //    this.exerciseScrollRgn.ScrollRect,
        //    1.0f);
    }

    public void OnSlider_CanvasScalerChanged()
    { 
        float canvasVal = Mathf.Lerp(maxScale, minScale, this.sliderUIScale.value);
        this.SetUIScale(canvasVal);
        
    }

    void SetUIScale(float scale)
    {
        // Unity likes to have a mind of its own with the scrolling region if it's modified...
        // it'd rather it not.
        float initialScroll = 0.0f;
        bool showingScroll = true;
        if(this.optionsScrollRect != null)
        {
            if(this.optionsScrollRect.viewport.rect.height >= this.optionsScrollRect.content.rect.height)
                showingScroll = false;

            initialScroll = this.optionsScrollRect.verticalNormalizedPosition;
        }


        // The the scale
        this.canvasScaler.scaleFactor = scale;

        // Save for next time the app is restarted
        PlayerPrefs.SetFloat(prefKey_UIScale, scale);

        if(this.optionsScrollRect != null)
        {
            //It's got a bad habit of starting the scroll at the bottom
            // if it' just poped in.
            if(showingScroll == false)
                initialScroll = 1.0f;

            this.App.StartCoroutine(
                PxPre.UIL.EleVertScrollRgn.SetVertScrollLater(
                    this.optionsScrollRect, 
                    initialScroll));
        }
    }

    public void OnSlider_MasterVolume()
    { 
        // This can trigger before initialization
        if(this.App == null)
            return;

        this.App.SetMasterVolume(this.sliderUIMaster.value);
    }

    public override void OnMasterVolumeChange(float newVal)
    {
        this.sliderUIMaster.value = newVal;
    }

    public override void OnMetronomeVolumeChange(float newVal)
    {
        if(this.sliderMetronomeVolume != null)
            this.sliderMetronomeVolume.value = newVal;
    }

    ExerciseEnumParam IExerciseParamProvider.paramNumber 
    { get {return this.paramNumber; } }

    ExerciseEnumParam IExerciseParamProvider.paramTimed 
    { get { return this.paramTimed; } }

    ExerciseEnumParam IExerciseParamProvider.paramShowAnswers 
    { get { return this.paramShowAnswers; } }

    ExerciseEnumParam IExerciseParamProvider.paramIntervalLabel 
    { get {return this.paramIntervalLabel; } }

    ExerciseEnumParam IExerciseParamProvider.paramBreak 
    { get {return this.paramBreak; } }

    ExerciseEnumParam IExerciseParamProvider.paramDirection
    { get {return this.paramDir; } }

    public void CreateInfoScrollDialog(out PxPre.UIL.Dialog dlg, out PxPre.UIL.UILStack scrolluis, string title)
    {
        dlg =
            this.App.dlgSpawner.CreateDialogTemplate(
                new Vector2(650.0f, 0.0f),
                PxPre.UIL.LFlag.AlignCenter,
                0.0f);

        dlg.AddDialogTemplateTitle(title);

        PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.App.uiFactory, dlg.rootParent, dlg.contentSizer);
        uiStack.PushImage(this.App.exerciseAssets.scrollRectBorder, 1.0f, PxPre.UIL.LFlag.Grow).Chn_SetImgFillCenter(false).Chn_SetImgSliced();
        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);
        PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> scrollRgn = 
            uiStack.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(1.0f, PxPre.UIL.LFlag.GrowVertOnCollapse | PxPre.UIL.LFlag.GrowHoriz);
        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(10.0f, 0.0f, 10.0f, 0.0f);

        scrolluis = uiStack.GetSnapshot();

        uiStack.Pop();
        uiStack.Pop();
        uiStack.Pop();
        uiStack.Pop();
        uiStack.AddSpace(5.0f, 0.0f, 0);

        dlg.AddDialogTemplateSeparator();
        dlg.AddDialogTemplateButtons(new PxPre.UIL.DlgButtonPair("Close", null));

        DoNothing dn = dlg.host.RT.gameObject.AddComponent<DoNothing>();
        dn.StartCoroutine( scrollRgn.SetVertScrollLater(1.0f));
    }

    public void ShowCreditsButton(RectTransform invoker)
    { 
        PxPre.UIL.Dialog dlg;
        PxPre.UIL.UILStack uiStack;
        this.CreateInfoScrollDialog(out dlg, out uiStack, "Credits");
        RectTransform scrollRectTrans = uiStack.GetHeadRectTransform();

        uiStack.AddText(
            "<b>Precision Keyboard</b> is a product of <b>Pixel Precision LLC</b>\n\n" +
            "<b>William Leu</b>\n\tDesign, assets, technology and app development.\n\n" +
            "<b>Ian Owerbach</b>\n\tTester.", 
            true, 
            0.0f, 
            PxPre.UIL.LFlag.GrowHoriz);

        uiStack.AddSpace(40.0f, 0.0f, 0);

        uiStack.AddText("<b>Attributions</b>", 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
        uiStack.AddHorizontalSeparator(0.0f);
        uiStack.AddText("<b>Daniel Simion</b>\n\tAnalog Watch Alarm Sound", true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        uiStack.AddText("<b>Yannick Lemieux</b>\n\tSmall Crowd Applause Sound", true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        uiStack.AddText("<b>Benboncan</b>\n\tBoxing Bell Sound", true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        uiStack.AddText("<b>SilentSin26</b>\n\tReddit post on Unity Haptics", true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        uiStack.AddText("<b>Unity Technologies</b>\n\tUnity Game Engine", true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

        uiStack.AddSpace(40.0f, 0.0f, 0);

        uiStack.AddText("<b>Shout-outs</b>", 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
        uiStack.AddHorizontalSeparator(0.0f);
        uiStack.AddText(
            "", 
            true, 
            0.0f, 
            PxPre.UIL.LFlag.GrowHoriz);

        dlg.host.LayoutInRTSmartFit();
        this.App.SetupDialogForTransition(dlg, invoker);

        // The transitioning logo
        GameObject goLogo = new GameObject("Logo");
        goLogo.transform.SetParent(scrollRectTrans, false);
        UnityEngine.UI.Image imgLogo = goLogo.AddComponent<UnityEngine.UI.Image>();
        imgLogo.sprite = this.creditsLogo;
        imgLogo.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        RectTransform rtLogo = imgLogo.rectTransform;
        rtLogo.pivot = new Vector2(0.5f, 0.5f);
        rtLogo.anchorMin = new Vector2(0.5f, 0.5f);
        rtLogo.anchorMax = new Vector2(0.5f, 0.5f);
        rtLogo.sizeDelta = this.creditsLogo.rect.size;
        const float logoLSc = 0.75f;
        rtLogo.localScale = new Vector3(logoLSc, logoLSc, logoLSc);
        rtLogo.SetAsFirstSibling();

        DoNothing dn = goLogo.AddComponent<DoNothing>();
        dn.StartCoroutine(CreditsLogoFillinEffect(imgLogo, 4.0f));
    }

    public void ShowClearPrefsQuestion(RectTransform invoker)
    { 

        PxPre.UIL.Dialog dlg = this.App.dlgSpawner.CreateDialogTemplate(true,false);
        dlg.AddDialogTemplateTitle("Clear Save Preferences?");

        PxPre.UIL.UILStack uiStack = dlg.CreateContentStack(this.App.uiFactory);

        uiStack.AddSpace(0.0f, 1.0f, 0);
        uiStack.AddText("Are you sure you want to clear the saved preferences?", true, 0.0f, PxPre.UIL.LFlag.AlignVertCenter|PxPre.UIL.LFlag.GrowHoriz);
        uiStack.AddSpace(0.0f, 1.0f, 0);
        uiStack.AddText("<i>Keyboard settings are saved so that when the app is closed and reopened the layout of the keyboard will be similar to last time. By removing these saved setting they will be reset to default values; so next time the application is run the keyboard interface will be created with default values.</i>", 14, true, 0.0f, PxPre.UIL.LFlag.AlignVertCenter|PxPre.UIL.LFlag.GrowHoriz);
        uiStack.AddSpace(0.0f, 1.0f, 0);


        dlg.AddDialogTemplateSeparator();

        PxPre.UIL.Dialog.OptionsButton [] btns = 
            dlg.AddDialogTemplateButtons(
                new PxPre.UIL.DlgButtonPair("Cancel", null), 
                new PxPre.UIL.DlgButtonPair("Clear", (x)=>{ PlayerPrefs.DeleteAll(); return true; }));

        btns[1].button.targetGraphic.color = Color.red;

        dlg.host.LayoutInRTSmartFit();
        this.App.SetupDialogForTransition(dlg, invoker);
    }

    static IEnumerator CreditsLogoFillinEffect(UnityEngine.UI.Image img, float timeOver)
    {
        img.type = Type.Filled;
        img.fillMethod = FillMethod.Radial360;
        img.fillAmount = 0.0f;

        float startTime = Time.time;
        yield return null;

        for(float f = Time.time; f < startTime + timeOver; f = Time.time)
        { 
            float lambda = (f - startTime)/timeOver;
            img.fillAmount = lambda;
            yield return null;
        }

        img.fillAmount = 1.0f;
    }

    public void ShowExplainOptionsButton(RectTransform invoker)
    {
        PxPre.UIL.Dialog dlg;
        PxPre.UIL.UILStack uiStack;
        this.CreateInfoScrollDialog(out dlg, out uiStack, "Explaination of Options");

        uiStack.AddText(
            "On the left of the options tab are exercises and text content.\n\n"+
            "On the right are the actual options and controls of the application and a more thorough explanation of all of them are listed below.\n\n",
            true,
            0.0f,
            PxPre.UIL.LFlag.GrowHoriz);

        float indention = 10.0f;
        float spaceBetweenEntries = 20.0f;

        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>UI Scale</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0.0f, 0);
                uiStack.AddText(
                    "The scaling factor for the entire users interface, including the other tabs."+
                    " Adjust this value to whatever is comfortable to use. It's not uncommon for cell phones " +
                    " to have this slider turned all the way up."
                    , true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>Master Vol</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "The Master control. This controls the volume of the keys pressed, the same as the Master slider in the keyboard tab.", 
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>FPS Meter</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "A toggle for the FPS meter on the top right. The FPS shows how fast the application's \"application loop\" is running which is a rough indicator "+
                    "of how fast the graphics are drawing and how fast the input is being applied, both for UI interaction and for the keyboard. This is also a very rough" +
                    " indicator of the performance of the synthesizer - if the synthesizer is asked to generate audio faster than it can compute, this number will start to go down.",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>FPS Scale</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "A slider to adjust the scale of the FPS meter on the top right of the application.",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>Tab Height</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "The height of the tabs on the top left of the application. Depending on your preference or your device, " +
                    "you may want it smaller to save space, or taller to make it easier to press on touch screens.",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>Exer. Vol</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "The volume of the sounds played during exercises. This does not include the sounds of wiring played by " +
                    "the exercise or by keyboard key presses - which is still controlled by the Master volume.",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>Ask Link</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "If the app opens up a link, the Ask Link option will bring up a dialog with the full URL And ask for " +
                    "confirmation before opening the link. This can be turned off to turn off the warning dialog and instantly " +
                    "open links in a browser.",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>Key Hapt</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "A slider to adjust haptic feedback for piano keys. Haptics can make the virtual keyboard feel more tangible " +
                    "and responsive because it will give you a tactile feedback to notifiy you exactly when it registered your key press." +
                    "Depending on your preference it may not be preffered, or you may want to turn it off to save battery power on your device. Set " +
                    "the slider to the very left to disable haptics.",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>Btn Hapt</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "A slider to adjust haptic feedback when interacting with user interface elements. Set the slider to the very left if you "+
                    " want to disable haptics.",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>Explaination of Options</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "I um,... well,... How did you think you got to this dialog?",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>Credits</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "A button that opens a dialog with the credits and a few shout-outs to muh peeps!",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(spaceBetweenEntries, 0.0f, 0);
        uiStack.PushVertSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddText("<b>Clear Prefs</b>", false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddSpace(indention, 0, 0);
                uiStack.AddText(
                    "Clear saved preferences information. This information recreates the keyboard to be configured like the last time the application was run. If the preferences are cleared and the application is restarted, the default configuration will be used.",
                    true, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.Pop();
        uiStack.Pop();

        uiStack.AddSpace(0.0f, 1.0f, 0);

        dlg.host.LayoutInRTSmartFit();
        this.App.SetupDialogForTransition(dlg, invoker);
    }

    public void SetupNewPushdownToggle(PushdownToggle pt)
    {
        pt.pushdown = new Vector2(0.0f, -5.0f);

        pt.downSprite = this.pushdownToggleDown;
        pt.downColor = Color.white;
        //
        UnityEngine.UI.ColorBlock cbWhite = new UnityEngine.UI.ColorBlock();
        cbWhite.colorMultiplier = 0.0f;
        cbWhite.fadeDuration = 0.0f;
        cbWhite.normalColor = Color.white;
        cbWhite.highlightedColor = Color.white;
        cbWhite.pressedColor = Color.white;
        cbWhite.disabledColor = Color.white;
        pt.downColorBlock = cbWhite;

        UnityEngine.UI.SpriteState ssUp = new UnityEngine.UI.SpriteState();
        ssUp.highlightedSprite = this.pushdownToggleUp;
        ssUp.pressedSprite = this.pushdownToggleTrans;
        pt.spriteState = ssUp;

        UnityEngine.UI.SpriteState ssDown = new UnityEngine.UI.SpriteState();
        ssDown.highlightedSprite = this.pushdownToggleDown; 
        ssDown.pressedSprite = this.pushdownToggleTrans;
        pt.downSpriteState = ssDown;

        UnityEngine.UI.Graphic oldG = pt.graphic;
        pt.graphic = null;
        oldG.color = new Color(0.0f, 0.0f, 0.0f, 0.1f);
        pt.graphicsToActivate.Add(new PushdownToggle.OnActivateGraphic(Color.black, oldG));

        UnityEngine.UI.Image img = pt.GetComponent<UnityEngine.UI.Image>();
        img.sprite = this.pushdownToggleUp;
        pt.plate = img;

        pt.Initialize(true);
        pt.SetSpriteDown(pt.isOn);

        this.SetupToggleToVibrate(pt);
    }

    public void SetupToggleToVibrate(UnityEngine.UI.Toggle toggle)
    {
        toggle.onValueChanged.AddListener( 
            (x)=>
            { 
                // If we're not initialized yet, we're still setting stuff
                // up wich can cause false vibrations from toggles.
                if(this.initialized == false)
                    return;

                this.App.DoVibrateButton(); 
            });
    }

    public void SetupSlider(UnityEngine.UI.Slider slider)
    { 
    }

    public void SetupSliderCreatedFromFactory(UnityEngine.UI.Slider slider)
    { 
        if(slider.direction == UnityEngine.UI.Slider.Direction.BottomToTop || 
            slider.direction == UnityEngine.UI.Slider.Direction.TopToBottom)
        { 
            return;
        }

        PushdownSlider ps = slider as PushdownSlider;
        if(ps == null)
            return;

        ps.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);

        GameObject goArrow = new GameObject("Arrow");
        goArrow.transform.SetParent(slider.handleRect);
        UnityEngine.UI.Image img = goArrow.AddComponent<UnityEngine.UI.Image>();
        img.sprite = this.horizontalThumbArrows;
        img.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

        RectTransform rtImg = img.rectTransform;
        rtImg.pivot = new Vector2(0.5f, 0.5f);
        rtImg.anchorMin = new Vector2(0.5f, 0.5f);
        rtImg.anchorMax = new Vector2(0.5f, 0.5f);
        rtImg.sizeDelta = this.horizontalThumbArrows.rect.size;
        rtImg.anchoredPosition = new Vector2(0.0f, 3.0f);
    }

    float PredictBestScale()
    { 
        float dpi = Screen.dpi;
        if( dpi <= 0.0f)
            dpi = 96.0f;

        Resolution r = Screen.currentResolution;

        float retPx = PredictBestScalePix(dpi, r.width, r.height);
        float retDm = PredictBestScaleDim(dpi, r.width / dpi, r.height / dpi);

        float accum = 0.0f;
        float wt = 0.0f;

        // Not sure this would ever happen, a defensive check
        if(float.IsNaN(retPx) == false)
        {
            accum += retPx;
            wt += 1.0f;
        }
        // Not sure this would ever happen, a defensive check
        if(float.IsNaN(retDm) == false)
        { 
            accum += retDm;
            wt += 1.0f;
        }

        float val = 0.5f;
        if(wt != 0.0f)
            val = accum / wt;

        // Quantize to a tenth
        val = Mathf.Round(val * 10.0f) / 10.0f; 
        val = Mathf.Clamp01(val);

        return val;
    }

    float PredictBestScalePix(float dpi, float pixelWidth, float pixelHeight)
    { 
        // This performs a polynomial with data entered into the excel in the
        // project called PreferedZoom.xlsx using the 
        // - dpi (x1)
        // - pixel width of the screen (x2)
        // - and pixel height of the screen (x3) 
        //
        // Regressed through http://www.xuru.org/rt/MPR.asp
        
        // Convert the names, this will shorten things and make it way more similar to
        // the output given to us on xuru.org - which will make things way more sane.
        float x1 = dpi;
        float x2 = pixelWidth;
        float x3 = pixelHeight;

        // If we copy-pasta the variables from 
        float x12 = x1 * x1;
        float x22 = x2 * x2;
        float x32 = x3 * x3;

        float y = 
            -1.0774651e-5f * x12 + 
            1.900050399e-6f * x1 * x2 + 
            3.410982293e-6f * x1 * x3 - 
            4.077532381e-8f * x22 + 
            3.525590897e-7f * x2 * x3 - 
            6.282656759e-6f * x32 - 
            3.773517925e-4f * x1 - 
            4.127394987e-4f * x2 + 
            1.054122432e-2f * x3 - 
            3.891956033f;

        return y;
    }

    float PredictBestScaleDim(float dpi, float inchWidth, float inchHeight)
    { 
        float x1 = dpi;
        float x2 = inchWidth;
        float x3 = inchHeight;

        // If we copy-pasta the variables from 
        float x12 = x1 * x1;
        float x22 = x2 * x2;
        float x32 = x3 * x3;

        // It may seem weird to have a seperate regression method instead of combining
        // it all into one regression process, but sadly we don't have the dataset big
        // enough for that.

        float y = 
            3.095611629e-6f * x12 - 
            6.10422306e-4f * x1 * x2 + 
            1.325943791e-3f * x1 * x3 - 
            1.157727648e-6f * x22 - 
            1.791986666e-2f * x2 * x3 + 
            1.856343509e-2f * x32 + 
            8.673916813e-4f * x1 + 
            2.557573545e-1f * x2 - 
            2.328007907e-1f * x3 - 
            6.976408129e-1f;

        return y;
    }
}
