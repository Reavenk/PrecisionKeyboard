// <copyright file="ExerciseIdentifyInterval.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The exercise to identify intervals.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseIdentifyInterval : BaseExercise
{
    public override string title => "Identify Intervals";
    public override string description => "Ear training for identifying note intervals.";

    ExerciseEnumParam paramDifficulty;
    ExerciseEnumParam paramIntervalLabel;

    public struct Interval
    {
        public struct IntName
        { 
            public string name;
            public string shorthand;

            public IntName(string name, string shorthand)
            { 
                this.name = name;
                this.shorthand = shorthand;
            }
        }

        public int offset;
        public IntName name;
        public IntName[] names;

        public Interval(int offset, IntName name)
        {
            this.offset = offset;
            this.name = name;
            this.names = null;
        }

        public Interval(int offset, IntName name, params IntName[] names)
        {
            this.offset = offset;
            this.name = name;
            this.names = names;
        }
    }

    public struct CondensedInterval
    { 
        public readonly string name;
        public readonly int offset;

        public CondensedInterval(string name, int offset)
        { 
            this.name = name;
            this.offset = offset;
        }
    }

    public override bool Initialize(Application app, ExerciseAssets assets, IExerciseParamProvider sharedParams)
    {
        if (base.Initialize(app, assets, sharedParams) == false)
            return false;

        this.paramDifficulty =
            new ExerciseEnumParam(
                "Difficulty",
                "How much of a challenge do you want?",
                "EXR_Int_Difficulty",
                true,
                new ExerciseEnumParam.Entry(0, assets.diffEasy, "Easy", "For beginners. Four options. Octave, Unison, Mjr Second, Mnr Third."),
                new ExerciseEnumParam.Entry(1, assets.diffMedium, "Medium", "For experienced users. Four options. All intervals."),
                new ExerciseEnumParam.Entry(2, assets.diffHard, "Hard", "For experts! Thirteen options. All intervals."));

        this.paramIntervalLabel = sharedParams.paramIntervalLabel;

        this.exerciseParams.Add(this.paramDifficulty);
        this.exerciseParams.Add(this.paramIntervalLabel);
        this.exerciseParams.Add(this.paramNumber);
        this.exerciseParams.Add(this.paramShowAnswers);
        this.exerciseParams.Add(this.paramTimed);

        return true;
    }

    List<CondensedInterval> GetLabels()
    { 
        return GetLabels(this.paramIntervalLabel.GetInt());
    }

    public static List<CondensedInterval> GetLabels(int id)
    {
        List<Interval> lstI = null;
        bool randShort = false;

        switch(id)
        { 
            case 0:
            default:
                lstI = GetValueInterval();
                break;

            case 1:
                lstI = GetMajMinQualityInterval();
                break;

            case 2:
                lstI = GetAugDimQualityInterval();
                break;

            case 3:
                randShort = (Random.Range(0, 2) == 0);

                switch(Random.Range(0, 3))
                { 
                    default:
                    case 0:
                        lstI = GetValueInterval();
                        break;

                    case 1:
                        lstI = GetMajMinQualityInterval();
                        break;

                    case 2:
                        lstI = GetAugDimQualityInterval();
                        break;

                }
                break;
        }

        List<CondensedInterval> ret = 
            new List<CondensedInterval>();

        foreach(Interval i in lstI)
        { 
            if(randShort == false)
                ret.Add(new CondensedInterval(i.name.name, i.offset));
            else
                ret.Add(new CondensedInterval(i.name.shorthand, i.offset));
        }

        return ret;
    }

    protected override IEnumerator ExerciseCoroutineImpl(int questions, bool showAnswers, float timer, TweenUtil tweener, ExerciseUtil exu, OctaveRange? range)
    {        
        bool allOptions = this.paramDifficulty.GetInt() == 2;

        int minRange = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 4);
        int maxRange = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 5);

        List<int> playingSamples = new List<int>();

        bool contQuiz = true;
        while (contQuiz == true)
        {
            List<CondensedInterval> lstci = GetLabels();

            PxPre.UIL.EleHost host = CreateQuizRegion();
            PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, host);
            uiStack.AddVertSizer();
            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.AlignCenter|PxPre.UIL.LFlag.GrowHoriz|PxPre.UIL.LFlag.GrowVertOnCollapse);
            PxPre.UIL.EleText eleTitle = uiStack.AddText("Identify the interval.", 50, true, 0.0f, PxPre.UIL.LFlag.AlignCenter|PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);
            PxPre.UIL.EleText eleSub = uiStack.AddText("Select what you hear.", true, 0.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);
            uiStack.AddSpace(10.0f, 0.0f, 0);

            DoNothing dn = host.RT.gameObject.AddComponent<DoNothing>();
            TweenUtil correctionTween = 
                new TweenUtil(dn);

            CondensedInterval [] rintUse = null;
            if (this.paramDifficulty.GetInt() == 0)
            {
                rintUse = 
                    new CondensedInterval[] 
                    {
                        lstci[0],
                        lstci[1],
                        lstci[2],
                        lstci[3],
                        lstci[4],
                        lstci[lstci.Count - 1],
                    };
            }
            else if(this.paramDifficulty.GetInt() == 1)
            {

                List<int> lstidx = GetRandomValues(0, 12, 4);
                rintUse = new CondensedInterval [4];
                for(int i = 0; i < lstidx.Count; ++i)
                    rintUse[i] = lstci[lstidx[i]];
            }
            else if(this.paramDifficulty.GetInt() == 2)
            { 
                rintUse = lstci.ToArray();
            }

            // Get a random note
            int firstIdx = Random.Range(minRange, maxRange);
            int corIdx = Random.Range(0, rintUse.Length);
            int secondIdx = firstIdx + rintUse[corIdx].offset;
            UnityEngine.UI.Button correct = null;

            exu.SetCorrectBuffer(false, firstIdx, secondIdx);

            BaseFloatingQuestion fq = 
                new BaseFloatingQuestion(
                    this.app, 
                    null, 
                    tutorialRegionObjs.replayButton,
                    exu,
                    true);

            List<UnityEngine.UI.Button> questionButtons = new List<UnityEngine.UI.Button>();

            PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> vertscroll = 
                uiStack.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(1.0f, PxPre.UIL.LFlag.GrowOnCollapse|PxPre.UIL.LFlag.AlignCenter|PxPre.UIL.LFlag.Grow);

            dn.StartCoroutine( ResetScroll(vertscroll.ScrollRect, 1.0f) );
            uiStack.PushGridSizer(2);
            for(int i = 0; i < rintUse.Length; ++i)
            {
                const float buttonWidth = 300.0f;
                    
                PxPre.UIL.EleGenButton<PushdownButton> btn = 
                    uiStack.PushButton<PushdownButton>(rintUse[i].name, 0.0f, 0);

                questionButtons.Add(btn.Button);
                btn.minSize = new Vector2(buttonWidth, 0.0f);
                UnityEngine.UI.Button btnObj = btn.Button;
                btn.Button.onClick.AddListener(()=>{ fq.SetStateFromButton(btnObj); });

                uiStack.Pop();

                if(i == corIdx)
                    correct = btn.Button;
            }
            uiStack.Pop();

            foreach(UnityEngine.UI.Button btn in questionButtons)
                btn.interactable = false;

            fq.correctBtn = correct;

            tutorialRegionObjs.replayButton.onClick.RemoveAllListeners();
            tutorialRegionObjs.replayButton.onClick.AddListener( 
                ()=>
                { 
                    this.app.DoVibrateButton();
                    dn.StartCoroutine(fq.PlaySample(playingSamples)); 
                });

            host.LayoutInRTSmartFit();

            correctionTween.SlidingAnchorFade(
                eleTitle.GetContentRect(),
                eleTitle.text,
                new Vector2(messageTweenDist, 0.0f),
                true,
                true,
                0.25f,
                TweenUtil.RestoreMode.RestoreLocal);

            correctionTween.SlidingAnchorFade(
                eleSub.GetContentRect(),
                eleSub.text,
                new Vector2(-messageTweenDist, 0.0f),
                true,
                true,
                0.25f,
                TweenUtil.RestoreMode.RestoreLocal);

            yield return new WaitForSeconds(0.2f);
            yield return dn.StartCoroutine(fq.PlaySample(playingSamples));

            foreach (UnityEngine.UI.Button btn in questionButtons)
                btn.interactable = true;

            exu.StartQuestionTimer();
            tutorialRegionObjs.replayButton.gameObject.SetActive(true);

            while (contQuiz == true)
            {
                bool contQuestion = true;
                ExerciseUtil.Timer timerStatus = exu.TickClock();

                if( fq.answerState != BaseFloatingQuestion.AnswerState.Unknown || timerStatus == ExerciseUtil.Timer.OutOfTime)
                {
                    float timeLen = exu.EndClock();

                    contQuestion = false;

                    bool outofTime = timerStatus == ExerciseUtil.Timer.OutOfTime;
                    bool answerCorrect = fq.answerState == BaseFloatingQuestion.AnswerState.Correct;

                    if (answerCorrect)
                        contQuiz = exu.IncrementAnswer(true);
                    else
                        contQuiz = exu.IncrementAnswer(false, outofTime);

                    string incorrectWhy =
                        (timerStatus == ExerciseUtil.Timer.OutOfTime) ?
                            "Sorry! You ran out of time." :
                            "Sorry! Incorrect answer.";

                    if (answerCorrect == false && showAnswers == true)
                    {
                        eleTitle.text.text = "Showing Correct Answer:";
                        eleSub.text.text = "Press correct answer to continue";

                        correctionTween.SlidingAnchorFade(
                            fq.correctBtn.targetGraphic.rectTransform,
                            null,
                            new Vector2(messageTweenDist, 0.0f),
                            false,
                            true,
                            0.25f,
                            TweenUtil.RestoreMode.RestoreLocal);

                        while (fq.answerState != BaseFloatingQuestion.AnswerState.Correct)
                        {
                            float strobeLambda = Mathf.Sin(Time.time * 2.0f * 2.0f) * 0.5f + 0.5f;

                            Color strobCol = Color.Lerp(Color.red, Color.white, strobeLambda);
                            fq.correctBtn.targetGraphic.color = strobCol;

                            yield return null;
                        }
                    }
                    break;
                }
                else if(contQuestion == true)
                    yield return null;
                else
                    break;

            }   
            GameObject.Destroy(host.RT.gameObject);
        }
    }

    public override PaneKeyboard.ExerciseDisplayMode GetDisplayMode()
    {
        return PaneKeyboard.ExerciseDisplayMode.PlainExercise;
    }

    public override ContentType GetExerciseType() => ContentType.EarTraining;

    public static List<Interval> GetValueInterval()
    {
        return new List<Interval>
        {
            new Interval(0,     new Interval.IntName("Unison",  "Unison")),
            new Interval(1,     new Interval.IntName("One",     "One")),
            new Interval(2,     new Interval.IntName("Two",     "Two")),
            new Interval(3,     new Interval.IntName("Three",   "Three")),
            new Interval(4,     new Interval.IntName("Four",    "Four")),
            new Interval(5,     new Interval.IntName("Five",    "Five")),
            new Interval(6,     new Interval.IntName("Six",     "Six")),
            new Interval(7,     new Interval.IntName("Seven",   "Seven")),
            new Interval(8,     new Interval.IntName("Eight",   "Eight")),
            new Interval(9,     new Interval.IntName("Nine",    "Nine")),
            new Interval(10,    new Interval.IntName("Ten",     "Ten")),
            new Interval(11,    new Interval.IntName("Eleven",  "Eleven")),
            new Interval(12,    new Interval.IntName("Octave",  "P8"))
        };
    }

    public static List<Interval> GetMajMinQualityInterval()
    { 
        return new List<Interval>
        { 
            new Interval(0,     new Interval.IntName("Unison",          "P1")),
            new Interval(1,     new Interval.IntName("Minor Second",    "m2")),
            new Interval(2,     new Interval.IntName("Major Second",    "M2")),
            new Interval(3,     new Interval.IntName("Minor Third",     "m3")),
            new Interval(4,     new Interval.IntName("Major Third",     "M3")),
            new Interval(5,     new Interval.IntName("Perfect Fourth",  "P4")),
            new Interval(7,     new Interval.IntName("Perfect Fifth",   "P5")),
            new Interval(8,     new Interval.IntName("Minor Sixth",     "m6")),
            new Interval(9,     new Interval.IntName("Major Sixth",     "M6")),
            new Interval(10,    new Interval.IntName("Minor Seventh",   "m7")),
            new Interval(11,    new Interval.IntName("Major Seventh",   "M7")),
            new Interval(12,    new Interval.IntName("Octave",          "P8")),
        };
    }

    public static List<Interval> GetAugDimQualityInterval()
    {
        return new List<Interval>
        {
            new Interval(0,     new Interval.IntName("Diminished Second",   "d2")),
            new Interval(1,     new Interval.IntName("Augmented Unison",    "A1")),
            new Interval(2,     new Interval.IntName("Diminished Third",    "d3")),
            new Interval(3,     new Interval.IntName("Augmented Second",    "d2")),
            new Interval(4,     new Interval.IntName("Major Third",         "d4")),
            new Interval(5,     new Interval.IntName("Diminished Fourth",   "d4")),
            new Interval(6,     new Interval.IntName("Diminished Fifth",    "d5"), new Interval.IntName("Augmented Fourth", "A4")),
            new Interval(7,     new Interval.IntName("Diminished Sixth",    "d6")),
            new Interval(8,     new Interval.IntName("Augmented Fifth",     "A5")),
            new Interval(9,     new Interval.IntName("Diminished Seventh",  "d7")),
            new Interval(10,    new Interval.IntName("Augmented Sixth",     "A6")),
            new Interval(11,    new Interval.IntName("Diminished Octave",   "d8")),
            new Interval(12,    new Interval.IntName("Augmented Seventh",   "A7")),
        };
    }
}
