using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseUtil
{
    TweenUtil tweener;
    Application app;
    BaseExercise exercise;

    public enum Grade
    {
        APlus,
        A,
        B,
        C,
        D,
        F
    }

    struct HighlightKeyRecord
    {
        public Key key;
        public Color colorA;
        public Color colorB;
        public bool strobe;
    }

    public enum KeyCheck
    { 
        Void,
        Correct,
        Increment,
        Incorrect
    }

    int totalQuestions;
    public int TotalQuestions {get{return this.totalQuestions; } }

    int curQuestion = 0;
    public int CurrentQuestion {get{return this.curQuestion; } }

    int correct = 0;
    public int CorrectAnswered {get{return this.correct; } }

    int incorrect = 0;
    public int IncorrectAnswered {get{return this.incorrect; } }

    int timeout = 0;
    public int Timeouts {get{return this.timeout; } }

    float timePerQuestion = 0.0f;
    float timeQuestionStarted = 0.0f;
    float timeEllapsed = 0.0f;

    float timeExerciseStarted = -1.0f;

    Dictionary<KeyPair, HighlightKeyRecord> highlightedKeys = 
        new Dictionary<KeyPair, HighlightKeyRecord>();

    List<KeyPair> correctAnswers = new List<KeyPair>();
    List<KeyPair> leftAnswers = new List<KeyPair>();
    bool requireInOrder = true;
    bool turnOffHightlights = false;

    public Color highlightColor = Color.green;
    public Color correctionColor = Color.red;
    public Color rootColor = Color.yellow;
    public Color pressedColor = new Color(1.0f, 0.5f, 0.25f);

    public bool highlightPressed = false;

    public enum Timer
    { 
        Invalid,
        Valid,
        OutOfTime
    }

    public ExerciseUtil(Application app, BaseExercise exercise, TweenUtil tweener, int questions, float timePerQuestion)
    { 
        this.app = app;
        this.exercise = exercise;
        this.tweener = tweener;
        this.totalQuestions = questions;
        this.timePerQuestion = timePerQuestion;

        this.UpdateQuestionsRegion(false);

        this.app.keyboardPane.quizRecordCorrect.text.text = this.correct.ToString();
        this.app.keyboardPane.quizRecordIncorrect.text.text = this.incorrect.ToString();

        this.NullTimer();
    }

    public void UpdateQuestionsRegion(bool pulse)
    {
        string questionTotalStr = "∞";
        if(totalQuestions != int.MaxValue)
            questionTotalStr = totalQuestions.ToString();

        BaseExercise.titlebarObjs.progressText.text = $"{this.curQuestion + 1}/{questionTotalStr}";

        if (pulse == true)
        {
            this.tweener.WobbleScale(
                BaseExercise.titlebarObjs.progressText.rectTransform, 
                0.8f, 
                1.5f, 
                0.5f, 
                4.0f);
        }
    }

    public void StartQuestionTimer()
    { 
        this.timeQuestionStarted = Time.time;

        if(this.timeExerciseStarted <= 0.0f)
            this.timeExerciseStarted = Time.time;

        if(BaseExercise.titlebarObjs.timerText != null)
        {
            this.tweener.WobbleScale(
                BaseExercise.titlebarObjs.timerText.rectTransform,
                0.8f,
                1.5f,
                0.5f,
                4.0f);
        }

        if(this.IsTimed() == true)
        {
            this.app.keyboardPane.EnsureQuizTimerTicksReady();

            this.app.keyboardPane.timedAudioSlow.volume = 1.0f;
            this.app.keyboardPane.timedAudioSlow.Stop();
            this.app.keyboardPane.timedAudioSlow.Play();
            //
            this.app.keyboardPane.timedAudioMed.volume = 0.0f;
            this.app.keyboardPane.timedAudioMed.Stop();
            this.app.keyboardPane.timedAudioMed.Play();
            //
            this.app.keyboardPane.timedAudioFast.volume = 0.0f;
            this.app.keyboardPane.timedAudioFast.Stop();
            this.app.keyboardPane.timedAudioFast.Play();
        }
    }

    bool IsTimed()
    { 
        return this.timePerQuestion > 0.0f && this.timePerQuestion != float.PositiveInfinity;
    }

    public Timer TickClock()
    { 
        bool inited = this.app.keyboardPane.EnsureQuizTimerTicksReady();

        if (
            this.IsTimed() == false ||
            BaseExercise.titlebarObjs.timerText == null)
        {
            return Timer.Invalid;
        }

        float timeEllapsed = Time.time - this.timeQuestionStarted;
        float timeLeft = this.timePerQuestion - timeEllapsed;

        if(timeLeft <= 0.0f)
        { 
            this.NullTimer();
            return Timer.OutOfTime;
        }

        bool cananim = timeEllapsed > 2.0f;
        RectTransform rtTimerTxt = BaseExercise.titlebarObjs.timerText.rectTransform;

        float secPor = timeLeft % 1.0f;
        float s = 1.0f;
        if (secPor < 0.3f)
            s += (1.0f - secPor / 0.3f) * 0.2f;

        if(inited == true)
        {
            this.app.keyboardPane.timedAudioFast.Play();
            this.app.keyboardPane.timedAudioMed.Play();
            this.app.keyboardPane.timedAudioFast.Play();

            float retime = 1.0f - (timeLeft % 1.0f);
            this.app.keyboardPane.timedAudioFast.time = retime;
            this.app.keyboardPane.timedAudioMed.time = retime;
            this.app.keyboardPane.timedAudioFast.time = retime;
        }

        if (timeLeft < 2.0f)
        { 
            float lambda = 1.0f - (timeLeft / 2.0f);
            this.app.keyboardPane.timedAudioFast.volume = lambda;
            this.app.keyboardPane.timedAudioMed.volume = 1.0f - lambda;
            this.app.keyboardPane.timedAudioSlow.volume = 0.0f;

            s += Random.Range(-0.1f, 0.1f) * (1.0f - lambda);
            s += Mathf.Sin(timeLeft * Mathf.PI * 4.0f) * lambda * 0.1f;
        }
        else if(timeLeft < 5.0f)
        {
            float lambda = 1.0f - Mathf.InverseLerp(2.0f, 5.0f, timeLeft);

            this.app.keyboardPane.timedAudioFast.volume = 0.0f;
            this.app.keyboardPane.timedAudioMed.volume = lambda;
            this.app.keyboardPane.timedAudioSlow.volume = 1.0f - lambda;

            s += Random.Range(-0.1f, 0.1f) * lambda;
        }
        else
        {
            this.app.keyboardPane.timedAudioFast.volume = 0.0f;
            this.app.keyboardPane.timedAudioMed.volume = 0.0f;
            this.app.keyboardPane.timedAudioSlow.volume = 1.0f;
        }

        if(cananim == true)
            rtTimerTxt.localScale =  new Vector3(s, s, s);

        BaseExercise.titlebarObjs.timerText.text = Mathf.Ceil(timeLeft).ToString();
        return Timer.Valid;
    }

    public float EndClock()
    { 
        this.timeEllapsed = Time.time - this.timeExerciseStarted;

        return this.timeEllapsed;
    }

    public void NullTimer()
    { 
        if(BaseExercise.titlebarObjs.timerText == null)
            return;

        BaseExercise.titlebarObjs.timerText.text = "---";
    }


    public bool AddCorrect()
    { 
        return IncrementAnswer(true);
    }

    public bool AddIncorrect()
    {
        return IncrementAnswer(false);
    }

    public bool IncrementAnswer(bool correct, bool timeout = false)
    {
        this.app.keyboardPane.timedAudioSlow.Stop();
        this.app.keyboardPane.timedAudioMed.Stop();
        this.app.keyboardPane.timedAudioFast.Stop();

        if (correct == true)
        {
            ++this.correct;
            this.IncrementTally(this.app.keyboardPane.quizRecordCorrect);
            this.app.keyboardPane.quizRecordCorrect.text.text = this.correct.ToString();

            this.app.keyboardPane.PlayAudio_Correct();
        }
        else
        {
            ++this.incorrect;

            if(timeout == true)
            {
                this.app.keyboardPane.PlayAudio_Timeout();
                ++this.timeout;
            }
            else
                this.app.keyboardPane.PlayAudio_Incorrect();

            this.IncrementTally(this.app.keyboardPane.quizRecordIncorrect);
            this.app.keyboardPane.quizRecordIncorrect.text.text = this.incorrect.ToString();
        }

        ++curQuestion;


        //      DO ANIMATED CHECK OR X
        ////////////////////////////////////////////////////////////////////////////////
        GameObject largeIco = new GameObject("ValidIco");
        largeIco.transform.SetParent(CanvasSingleton.canvas.transform);
        UnityEngine.UI.Image imgLIco = largeIco.AddComponent<UnityEngine.UI.Image>();

        imgLIco.sprite = correct ? 
            this.app.exerciseAssets.largeCheck : 
            this.app.exerciseAssets.largeCross;

        RectTransform rt = imgLIco.rectTransform;
        rt.sizeDelta = imgLIco.sprite.rect.size;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        this.tweener.SlidingAnchorFade(
            rt, 
            imgLIco, 
            new Vector2(0.0f, 100.0f), 
            false, 
            false, 
            0.5f, 
            TweenUtil.RestoreMode.Delete);


        return curQuestion < totalQuestions;
    }

    public void IncrementTally(QuizProgressRecord qpr)
    {
        this.tweener.WobbleScale(qpr.icon.rectTransform, 0.8f, 1.5f, 1.0f, 4.0f);

        this.tweener.SlidingAnchorFade(
            qpr.incrementAnimText.rectTransform, 
            qpr.incrementAnimText,
            new Vector2(50.0f, 0.0f),
            false, 
            true, 
            2.0f, 
            TweenUtil.RestoreMode.RestoreLocal);
    }

    public void DoResultsDialog()
    {
        PxPre.UIL.Factory uif = this.app.uiFactory;

        PxPre.UIL.Dialog dlg = 
            this.app.dlgSpawner.CreateDialogTemplate(
                new Vector2(650.0f, -1.0f), 
                PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowVert, 
                1.0f);

        dlg.dialogSizer.border = new PxPre.UIL.PadRect(0.0f, 20.0f, 0.0f, 20.0f);

        dlg.AddDialogTemplateTitle( "Exercise Results");

        PxPre.UIL.EleImg scrollContainerPlate = uif.CreateImage(dlg.rootParent, this.app.exerciseAssets.scrollRectBorder);
        scrollContainerPlate.Img.type = UnityEngine.UI.Image.Type.Sliced;
        scrollContainerPlate.Img.fillCenter = false;
        dlg.contentSizer.Add(scrollContainerPlate, 1.0f, PxPre.UIL.LFlag.Grow);
        PxPre.UIL.EleBoxSizer scrollContainerPlateSzr = uif.VerticalSizer(scrollContainerPlate);
        const float scrollRectPadding = 5.0f;
        scrollContainerPlateSzr.border = new PxPre.UIL.PadRect(scrollRectPadding);


        PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> vertScroll = 
            uif.CreateGenVerticalScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(scrollContainerPlate);

        vertScroll.ScrollRect.StartCoroutine(SetScrollbarVert(vertScroll.ScrollRect, 1.0f));

        scrollContainerPlateSzr.Add(vertScroll, 1.0f, PxPre.UIL.LFlag.Grow);
        PxPre.UIL.EleBoxSizer scrollRootPad = uif.HorizontalSizer(vertScroll);
        scrollRootPad.AddHorizontalSpace(10.0f);
        PxPre.UIL.EleBoxSizer scrollBoxSizer = uif.VerticalSizer(scrollRootPad, 1.0f, PxPre.UIL.LFlag.Grow);
        scrollRootPad.AddHorizontalSpace(10.0f);

        scrollBoxSizer.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);

        string summaryText = $"You just finished the exercise <b>{this.exercise.title}</b> with a score of {this.CorrectAnswered}/{this.TotalQuestions}.";
        if (this.CorrectAnswered == this.TotalQuestions)
        {
            summaryText += "\n\nThat's a <b>PERFECT</b> score! Congratulations!";
            
            DoNothing dnPlayPerfect = dlg.host.RT.gameObject.AddComponent<DoNothing>();
            dnPlayPerfect.StartCoroutine(this.PlayPerfectGradeAudio(1.0f));
        }



        PxPre.UIL.EleBoxSizer horizSummary = uif.HorizontalSizer(scrollBoxSizer, 1.0f, PxPre.UIL.LFlag.GrowHoriz);
        PxPre.UIL.EleImg imgMedallion = uif.CreateImage(vertScroll, this.app.exerciseAssets.medal);
        PxPre.UIL.EleText ebrief = uif.CreateText(vertScroll, summaryText, 20, true);
        horizSummary.Add(imgMedallion, 0.0f, PxPre.UIL.LFlag.AlignVertCenter);
        horizSummary.AddHorizontalSpace(10.0f);
        horizSummary.Add(ebrief, 1.0f, PxPre.UIL.LFlag.GrowHoriz | PxPre.UIL.LFlag.AlignVertCenter);

        scrollBoxSizer.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
        //////////////////////////////////////////////////

        AddDialogSectionHeader(null, uif, vertScroll, scrollBoxSizer, "Summary");

        // Correct answers
        //////////////////////////////////////////////////
        scrollBoxSizer.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
        PxPre.UIL.EleBoxSizer ebsCorrectHoriz = uif.HorizontalSizer(scrollBoxSizer, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        scrollBoxSizer.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
        //
        PxPre.UIL.EleImg imgCorrect = uif.CreateImage(vertScroll, this.app.keyboardPane.quizRecordCorrect.icon.sprite);
        PxPre.UIL.EleText txtCorrect = uif.CreateText(vertScroll, $"You got {this.CorrectAnswered} answers correct.", 20, true);
        ebsCorrectHoriz.Add(imgCorrect, 0.0f, PxPre.UIL.LFlag.AlignCenter);
        ebsCorrectHoriz.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
        ebsCorrectHoriz.Add(txtCorrect, 1.0f, PxPre.UIL.LFlag.Grow);

        //////////////////////////////////////////////////

        scrollBoxSizer.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
        PxPre.UIL.EleBoxSizer ebsIncorrectHoriz = uif.HorizontalSizer(scrollBoxSizer, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        scrollBoxSizer.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
        //
        PxPre.UIL.EleImg imgIncorrect = uif.CreateImage(vertScroll, this.app.keyboardPane.quizRecordIncorrect.icon.sprite);
        string wrongAnswers = $"You got {this.IncorrectAnswered} incorrect.";
        if (this.Timeouts != 0)
        {
            string quantVert = (this.Timeouts == 1) ? "was" : "were";
            wrongAnswers += $"\n{this.Timeouts} of them {quantVert} from taking too long.";
        }
        PxPre.UIL.EleText txtIncorrect = uif.CreateText(vertScroll, wrongAnswers, 20, true);
        ebsIncorrectHoriz.Add(imgIncorrect, 0.0f, PxPre.UIL.LFlag.AlignCenter);
        ebsIncorrectHoriz.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
        ebsIncorrectHoriz.Add(txtIncorrect, 1.0f, PxPre.UIL.LFlag.Grow);

        //////////////////////////////////////////////////

        AddDialogSectionHeader(null, uif, vertScroll, scrollBoxSizer, "Exercise Description");

        scrollBoxSizer.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);

        foreach (ExerciseEnumParam eep in this.exercise.ExerciseParams)
        {
            PxPre.UIL.EleBoxSizer paramBoxSizer = uif.HorizontalSizer(scrollBoxSizer, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

            PxPre.UIL.EleImg paramImg = uif.CreateImage(vertScroll, eep.GetSprite());
            paramBoxSizer.Add(paramImg, 0.0f, PxPre.UIL.LFlag.AlignCenter);

            paramBoxSizer.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
            PxPre.UIL.EleBoxSizer paramTextsSizers = uif.VerticalSizer(paramBoxSizer, 1.0f, PxPre.UIL.LFlag.Grow);

            PxPre.UIL.EleText paramTitle = uif.CreateText(vertScroll, $"<b>{eep.paramName}: {eep.GetLabel()}</b>", 20, false);
            PxPre.UIL.EleBoxSizer bsTitle = uif.HorizontalSizer(paramTextsSizers, 0.0f, 0);
            bsTitle.AddHorizontalSpace(20.0f);
            bsTitle.Add(paramTitle, 0.0f, 0);
            paramTextsSizers.Add(bsTitle, 0.0f, 0);

            paramTextsSizers.AddSpace(5.0f);

            PxPre.UIL.EleText paramDescr = uif.CreateText(vertScroll, eep.GetDescription(), 20, true);
            paramTextsSizers.Add(paramDescr, 1.0f, PxPre.UIL.LFlag.Grow);


            //////////////////////////////////////////////////
            scrollBoxSizer.AddSpace(10.0f);

            PxPre.UIL.EleImg propSep = uif.CreateImage(vertScroll, null, new Vector2(2.0f, 2.0f));
            propSep.Img.color = Color.black;
            scrollBoxSizer.Add(propSep, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

            scrollBoxSizer.AddSpace(10.0f);
        }

        PxPre.UIL.EleGenButton<PushdownButton> pbModifyExer = this.app.uiFactory.CreateButton<PushdownButton>(vertScroll, "Modify Exercise");
        scrollBoxSizer.Add(pbModifyExer, 0.0f, PxPre.UIL.LFlag.Grow);
        pbModifyExer.Button.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);
        pbModifyExer.Button.onClick.AddListener(
            () =>
            {
                PxPre.UIL.Dialog modDlg = this.exercise.CreateDialog(this.app.exerciseAssets);
                this.app.SetupDialogForTransition(modDlg, pbModifyExer.RT, false, false);

                dlg.DestroyDialog();
            });

        scrollBoxSizer.AddVerticalSpace(20.0f);

        //////////////////////////////////////////////////

        AddDialogSectionHeader(null, this.app.uiFactory, vertScroll, scrollBoxSizer, "Stats");

        PxPre.UIL.ElePropGrid propStats = this.app.uiFactory.CreatePropertyGrid(vertScroll, 20);
        propStats.labelPushdown = 0.0f;
        scrollBoxSizer.Add(propStats, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        propStats.Add(this.app.uiFactory.CreateText(vertScroll, ((float)this.correct/(float)this.totalQuestions * 100.0f).ToString("0.0") + "%", false), 0.0f, 0, "Percent");
        propStats.Add(this.app.uiFactory.CreateText(vertScroll, this.timeEllapsed.ToString("0.00") + " Secs", 20, false), 0.0f, 0, "Total Time");
        propStats.Add(this.app.uiFactory.CreateText(vertScroll, this.TotalQuestions.ToString(), 20, false), 0.0f, 0, "Questions");
        propStats.Add(this.app.uiFactory.CreateText(vertScroll, (this.timeEllapsed / this.TotalQuestions).ToString("0.00") + " Secs/Q", 20, false), 0.0f, 0, "Avg Time/Q");

        //////////////////////////////////////////////////
        
        dlg.contentSizer.AddSpace(8.0f);
        PxPre.UIL.EleImg propLowerSep = this.app.uiFactory.CreateImage(dlg.rootParent, null, new Vector2(4.0f, 4.0f));
        propLowerSep.Img.color = Color.black;
        dlg.contentSizer.Add(propLowerSep, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        dlg.contentSizer.AddSpace(5.0f);

        PxPre.UIL.EleText eleContText = this.app.uiFactory.CreateText(dlg.rootParent, "Do you wish to stop or repeat the exercise?", 20, true);
        dlg.contentSizer.Add(eleContText, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

        dlg.AddDialogTemplateSeparator();
        dlg.AddDialogTemplateButtons(
            new PxPre.UIL.DlgButtonPair("Close", (x) => { return true; }),
            new PxPre.UIL.DlgButtonPair("Repeat",
            (x) =>
            {
                this.app.StartExercise(this.exercise, this.exercise.GetDisplayMode());
                return true;
            }));

        dlg.host.LayoutInRT();

        // Medal Animation
        DoNothing dn = dlg.host.RT.gameObject.AddComponent<DoNothing>();
        TweenUtil tu = new TweenUtil(dn);
        dn.StartCoroutine(AnimateInGrade(imgMedallion.RT, tweener, this.GetGrade() ));
    }

    public Grade GetGrade()
    { 
        float percent = (float)this.correct / (float)this.totalQuestions;

        if(percent >= 1.0f)
            return Grade.APlus;
        else if(percent >= 0.9f)
            return Grade.A;
        else if(percent >= 0.8f)
            return Grade.B;
        else if(percent >= 0.7f)
            return Grade.C;
        else if(percent >= 0.6f)
            return Grade.D;
        else
            return Grade.F;
    }

    IEnumerator PlayPerfectGradeAudio(float time)
    { 
        yield return new WaitForSeconds(time);
        this.app.keyboardPane.PlayAudio_PerfectApplause();
    }

    IEnumerator AnimateInGrade(RectTransform imgRT, TweenUtil tweener, Grade grade)
    {
        Sprite gradeSprite = null;
        Color medalColor;
        switch(grade)
        {
            case Grade.APlus:
            case Grade.A:
                gradeSprite = this.app.exerciseAssets.gradeA;
                medalColor = new Color(0.9f, 0.7f, 0.22f);
                break;

            case Grade.B:
                gradeSprite = this.app.exerciseAssets.gradeB;
                medalColor = Color.green;
                break;

            case Grade.C:
                gradeSprite = this.app.exerciseAssets.gradeD;
                medalColor = Color.yellow;
                break;

            case Grade.D:
                gradeSprite = this.app.exerciseAssets.gradeD;
                medalColor = Color.gray;
                break;

            default:
            case Grade.F:
                gradeSprite = this.app.exerciseAssets.gradeF;
                medalColor = Color.red;
                break;
        }

        GameObject goGrade = new GameObject("Grade");
        goGrade.transform.SetParent(imgRT, false);
        UnityEngine.UI.Image imgGrade = goGrade.AddComponent<UnityEngine.UI.Image>();
        imgGrade.sprite = gradeSprite;
        imgGrade.rectTransform.sizeDelta = imgGrade.sprite.rect.size;
        imgGrade.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        float medalYAnchor = 0.65f;
        imgGrade.rectTransform.anchorMin = new Vector2(0.5f, medalYAnchor);
        imgGrade.rectTransform.anchorMax = new Vector2(0.5f, medalYAnchor);
        imgGrade.rectTransform.anchoredPosition = Vector2.zero;
        
        tweener.SlidingAnchorFade(imgGrade.rectTransform, imgGrade, new Vector2(50.0f, 0.0f), true, true, 0.5f);
        tweener.Host.StartCoroutine( this.TweenColor(imgRT.GetComponent<UnityEngine.UI.Graphic>(), Color.white, medalColor, 0.5f));
        
        yield return new WaitForSeconds(0.5f);
        tweener.WobbleScale(imgGrade.rectTransform, 0.8f, 1.5f, 0.5f, 2.0f);

        if(grade == Grade.APlus)
        {
            GameObject goPlus = new GameObject("Grade");
            goPlus.transform.SetParent(imgRT, false);
            const float plusOffX = 0.8f;
            const float plusOffY = 0.8f;
            UnityEngine.UI.Image imgPlus = goPlus.AddComponent<UnityEngine.UI.Image>();
            imgPlus.sprite = this.app.exerciseAssets.gradeplus;
            imgPlus.rectTransform.sizeDelta = imgPlus.sprite.rect.size;
            imgPlus.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            imgPlus.rectTransform.anchorMin = new Vector2(plusOffX, plusOffY);
            imgPlus.rectTransform.anchorMax = new Vector2(plusOffX, plusOffY);
            imgPlus.rectTransform.anchoredPosition = Vector2.zero;

            tweener.SlidingAnchorFade(imgPlus.rectTransform, imgPlus, new Vector2(50.0f, 0.0f), true, true, 0.5f);
        }
    }

    IEnumerator TweenColor(UnityEngine.UI.Graphic graphic, Color start, Color end, float duration)
    { 
        float startTime = Time.time;

        while(Time.time < startTime + duration)
        {
            float ellapsed = Time.time - startTime;
            float lambda = ellapsed / duration;

            graphic.color = Color.Lerp(start, end, lambda);

            yield return null;
        }

        graphic.color = end;
    }


    private IEnumerator SetScrollbarVert(UnityEngine.UI.ScrollRect sr, float value)
    {
        yield return new WaitForEndOfFrame();
        sr.verticalNormalizedPosition = value;
    }

    private PxPre.UIL.EleBoxSizer AddDialogSectionHeader(
        Sprite sp, 
        PxPre.UIL.Factory factory, 
        PxPre.UIL.EleBaseRect parent, 
        PxPre.UIL.EleBaseSizer szr, 
        string text)
    {
        PxPre.UIL.EleImg plate = factory.CreateImage(parent, factory.headerSprite);
        plate.Img.color = new Color(0.95f, 0.95f, 1.0f);
        plate.Img.type = UnityEngine.UI.Image.Type.Sliced;
        PxPre.UIL.EleBoxSizer bsHoriz = factory.HorizontalSizer(plate);
        bsHoriz.border = new PxPre.UIL.PadRect(0.0f, 20.0f, 0.0f, 25.0f);

        PxPre.UIL.EleText sectionTitle = factory.CreateText(plate, text, 30, false);
        bsHoriz.Add(sectionTitle, 1.0f, PxPre.UIL.LFlag.AlignCenter);

        szr.Add(plate, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        return bsHoriz;
    }

    public bool AddKeyHighlight(int id, Color a)
    { 
        int octave;
        PxPre.Phonics.WesternFreqUtils.Key k;
        PxPre.Phonics.WesternFreqUtils.GetNoteInfo(id, out k, out octave);

        return this.AddKeyHighlight(new KeyPair(k, octave), a);
    }

    public bool AddKeyHighlight(int id, Color a, Color b)
    {
        int octave;
        PxPre.Phonics.WesternFreqUtils.Key k;
        PxPre.Phonics.WesternFreqUtils.GetNoteInfo(id, out k, out octave);

        return this.AddKeyHighlight(new KeyPair(k, octave), a, b);
    }

    public bool AddKeyHighlight(KeyPair kp, Color a)
    { 
        Key k = this.app.keyboardPane.GetKey(kp);
        if(k == null)
            return false;

        HighlightKeyRecord hr = new HighlightKeyRecord();
        hr.colorA = a;
        hr.colorB = a;
        hr.key = k;
        hr.strobe = false;

        if (this.highlightedKeys.ContainsKey(kp) == true)
            this.highlightedKeys[kp] = hr;
        else
            this.highlightedKeys.Add(kp, hr);

        return true;
    }

    public bool AddKeyHighlight(KeyPair kp, Color a, Color b)
    {
        Key k = this.app.keyboardPane.GetKey(kp);
        if (k == null)
            return false;

        HighlightKeyRecord hr = new HighlightKeyRecord();
        hr.colorA = a;
        hr.colorB = a;
        hr.key = k;
        hr.strobe = false;

        if(this.highlightedKeys.ContainsKey(kp) == true)
            this.highlightedKeys[kp] = hr;
        else
            this.highlightedKeys.Add(kp, hr);

        return true;
    }

    public bool AddKeyHighlight(PxPre.Phonics.WesternFreqUtils.Key k, int octave, Color a)
    { 
        return this.AddKeyHighlight(new KeyPair(k, octave), a);
    }

    public bool AddKeyHighlight(PxPre.Phonics.WesternFreqUtils.Key k, int octave, Color a, Color b)
    { 
        return this.AddKeyHighlight(new KeyPair(k, octave), a, b);
    }

    public bool UnhighlightKey(PxPre.Phonics.WesternFreqUtils.Key k, int octave)
    { 
        return this.UnhighlightKey(new KeyPair(k, octave));
    }

    public bool UnhighlightKey(int id)
    {
        int octave;
        PxPre.Phonics.WesternFreqUtils.Key k;
        PxPre.Phonics.WesternFreqUtils.GetNoteInfo(id, out k, out octave);

        return this.UnhighlightKey(new KeyPair(k, octave));
    }

    public bool UnhighlightKey(KeyPair kp)
    {
        HighlightKeyRecord hkr;
        if(this.highlightedKeys.TryGetValue(kp, out hkr) == false)
            return false;

        hkr.key.ResetColor();
        this.highlightedKeys.Remove(kp);
        return true;
    }

    public void DoKeyHighlights()
    { 
        foreach(KeyValuePair<KeyPair, HighlightKeyRecord> kvp in this.highlightedKeys)
        {
            UnityEngine.UI.Graphic g = kvp.Value.key.targetGraphic;

            if (kvp.Value.strobe == true)
            {
                g.color = 
                    Color.Lerp( 
                        kvp.Value.colorA,
                        kvp.Value.colorB,
                        Mathf.Sin(Time.time * 2.0f * 2.0f) * 0.5f + 0.5f);
            }
            else
                g.color = kvp.Value.colorA;
        }
    }

    public void ResetKeyHighlights()
    {
        foreach (KeyValuePair<KeyPair, HighlightKeyRecord> kvp in this.highlightedKeys)
            kvp.Value.key.ResetColor();

        this.highlightedKeys.Clear();
    }

    public void SetCorrectBuffer( bool inOrder, params KeyPair [] rkp)
    { 
        this.correctAnswers = new List<KeyPair>(rkp);
        this.leftAnswers = new List<KeyPair>(rkp);
        this.requireInOrder = inOrder;
    }

    public void SetCorrectBuffer(bool inOrder, params int [] ids)
    { 
        List<KeyPair> lst = new List<KeyPair>();

        foreach(int i in ids)
        {
            PxPre.Phonics.WesternFreqUtils.Key k;
            int octave;

            PxPre.Phonics.WesternFreqUtils.GetNoteInfo(i, out k, out octave);
            lst.Add(new KeyPair(k, octave));
        }

        this.SetCorrectBuffer(inOrder, lst.ToArray());
    }

    public IEnumerator PlayCorrectKeys(float duration, float padding, bool overlay, List<int> playingRecord)
    { 
        if(this.correctAnswers.Count == 0)
            yield break;

        if(this.correctAnswers.Count == 1)
            overlay = true;

        List<KeyPair> ans = this.correctAnswers;
        if (overlay == true)
        { 
            List<int> rec = new List<int>();

            foreach(KeyPair kp in ans)
            {
                int id = this.app.PressKey(kp, Application.NoteStartEvent.Started);
                rec.Add(id);
                playingRecord.Add(id);
            }

            yield return new WaitForSeconds(duration);

            foreach(int id in rec)
                this.app.EndNote(id);
        }
        else
        {
            for(int i = 0; i < ans.Count; ++i)
            {
                int id = this.app.PressKey(ans[i], Application.NoteStartEvent.Started);
                playingRecord.Add(id);
                yield return new WaitForSeconds(duration);
                this.app.EndNote(id);

                if(i != ans.Count - 1)
                    yield return new WaitForSeconds(padding);
            }
        }
    }

    public KeyCheck CheckInputBuffer(bool clear = true)
    { 
        if(BaseExercise.pressedNoteCache.Count == 0)
            return KeyCheck.Void;

        if(leftAnswers.Count > 0)
        {
            foreach(KeyPair kp in BaseExercise.pressedNoteCache)
            { 

                if(this.requireInOrder == true)
                { 
                    if(KeyPair.Matches(kp, this.leftAnswers[0]) == false)
                    {
                        if(clear == true)
                            BaseExercise.pressedNoteCache.Clear();

                        return KeyCheck.Incorrect;
                    }
                
                    if(this.turnOffHightlights == true)
                        this.UnhighlightKey(kp);

                    if(this.highlightPressed == true)
                        this.AddKeyHighlight(kp, this.pressedColor);

                    this.leftAnswers.RemoveAt(0);
                }
                else
                { 
                    bool found = false;

                    for(int i = 0; i < this.leftAnswers.Count; ++i)
                    { 
                        if(KeyPair.Matches(kp, this.leftAnswers[i]) == true)
                        {
                            if (this.turnOffHightlights == true)
                                this.UnhighlightKey(kp);

                            if (this.highlightPressed == true)
                                this.AddKeyHighlight(kp, this.pressedColor);

                            this.leftAnswers.RemoveAt(i);
                            found = true;
                        }
                    }

                    if(found == false)
                    {
                        if(clear == true)
                            BaseExercise.pressedNoteCache.Clear();

                        return KeyCheck.Incorrect;
                    }
                }
            }
        }

        if (clear == true)
            BaseExercise.pressedNoteCache.Clear();

        if(this.leftAnswers.Count == 0)
            return KeyCheck.Correct;

        return KeyCheck.Increment;
    }

    public void ResetCorrects(bool showAnswerMode)
    { 
        this.SetCorrectBuffer(this.requireInOrder, this.correctAnswers.ToArray());

        if(showAnswerMode == true)
        {
            this.requireInOrder = true;
            this.turnOffHightlights = true;

            this.HighlightCorrects();
        }
    }

    public void HighlightCorrects()
    {
        foreach (KeyPair kpc in this.correctAnswers)
            this.AddKeyHighlight(kpc, Color.red);
    }
}
