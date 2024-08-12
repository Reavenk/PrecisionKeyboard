// <copyright file="ExerciseIdentifyScales.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The exercise to identify scales.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseIdentifyScale : BaseExercise
{
    public override string title => "Identify Scales";
    public override string description => "Ear training to identify scales.";

    ExerciseEnumParam paramDifficulty;
    ExerciseEnumParam paramBreak;
    ExerciseEnumParam paramDir;

    public override bool Initialize(
        Application app,
        ExerciseAssets assets,
        IExerciseParamProvider sharedParams)
    {
        base.Initialize(app, assets, sharedParams);

        this.paramDifficulty =
            new ExerciseEnumParam(
                "Difficulty",
                "How much of a challenedge do you want?",
                "EXR_IdenScale_Difficulty",
                true,
                new ExerciseEnumParam.Entry(0, assets.diffEasy, "Easy", "C4 always used as the root. Only 4 pre-chosen scales will be used."),
                new ExerciseEnumParam.Entry(1, assets.diffMedium, "Medium", "C4 always used as the root. Any scale, but only 4 options."),
                new ExerciseEnumParam.Entry(2, assets.diffHard, "Hard", "Random root, all scales."));

        this.paramDir = sharedParams.paramDirection;
        this.paramBreak = sharedParams.paramBreak;

        this.exerciseParams.Add(this.paramDifficulty);
        this.exerciseParams.Add(this.paramDir);

        this.exerciseParams.Add(this.paramTimed);
        this.exerciseParams.Add(this.paramNumber);
        this.exerciseParams.Add(this.paramShowAnswers);

        return true;
    }

    public override ContentType GetExerciseType() => ContentType.EarTraining;

    public override PaneKeyboard.ExerciseDisplayMode GetDisplayMode() => PaneKeyboard.ExerciseDisplayMode.PlainExercise;

    protected override IEnumerator ExerciseCoroutineImpl(
        int questions, 
        bool showAnswers, 
        float timer, 
        TweenUtil tweener, 
        ExerciseUtil exu, 
        OctaveRange? range)
    {
        List<int> playingSamples = new List<int>();
        int minRange = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 4);
        int maxRange = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 5);

        List<MusicScale> scales =
            new List<MusicScale>()
            {
                MusicScale.ScaleMajor,
                MusicScale.ScaleMinor,
                MusicScale.ScaleHungarianMinor,
                MusicScale.ScaleArabic,
                MusicScale.ScalePersian
            };

        int numScales = 4;
        switch(this.paramDifficulty.GetInt())
        { 
            case 0:
                while(scales.Count < numScales)
                    scales.RemoveAt(Random.Range(0, scales.Count));
                break;

            case 1:
                break;

            case 2:
                numScales = scales.Count;
                break;
        }

        bool contQuiz = true;
        while(contQuiz == true)
        {
            exu.UpdateQuestionsRegion(true);

            int rootNote = minRange;

            BaseFloatingQuestion fq =
                new BaseFloatingQuestion(
                    this.app,
                    null,
                    tutorialRegionObjs.replayButton,
                    exu,
                    this.paramBreak.GetInt() == 0);

            PxPre.UIL.EleHost host = CreateQuizRegion();
            DoNothing dn = host.RT.gameObject.AddComponent<DoNothing>();
            PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, host);
            uiStack.AddVertSizer();
            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz | PxPre.UIL.LFlag.GrowVertOnCollapse);
            PxPre.UIL.EleText eleTitle = uiStack.AddText("Identify the scale.", 50, true, 0.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);
            PxPre.UIL.EleText eleSub = uiStack.AddText("Select what you hear.", true, 0.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);
            uiStack.AddSpace(10.0f, 0.0f, 0);

            TweenUtil correctionTween = 
                new TweenUtil(dn);

            List<UnityEngine.UI.Button> questionButtons = new List<UnityEngine.UI.Button>();
            PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> vertscroll = 
                uiStack.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(1.0f, PxPre.UIL.LFlag.GrowOnCollapse | PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.Grow);
            uiStack.PushGridSizer(2);

            UnityEngine.UI.Button correctBtn = null;

            List<MusicScale> scaleQuiz = new List<MusicScale>(scales);
            while(scaleQuiz.Count < numScales)
                scaleQuiz.RemoveAt(Random.Range(0, scaleQuiz.Count));

            int corIdx = Random.Range(0, scaleQuiz.Count);

            for(int i = 0; i < scaleQuiz.Count; ++i)
            {
                MusicScale ms = scaleQuiz[i];
                const float buttonWidth = 300.0f;

                PxPre.UIL.EleGenButton<PushdownButton> btn = 
                    uiStack.PushButton<PushdownButton>(ms.name, 0.0f, 0);

                questionButtons.Add(btn.Button);
                btn.minSize = new Vector2(buttonWidth, 0.0f);
                UnityEngine.UI.Button btnObj = btn.Button;
                btn.Button.onClick.AddListener(()=>{ fq.SetStateFromButton(btnObj); });

                uiStack.Pop();

                if(i == corIdx)
                    correctBtn = btn.Button;
            }
            uiStack.Pop();

            foreach (UnityEngine.UI.Button btn in questionButtons)
                btn.interactable = false;

            if(this.paramDifficulty.GetInt() == 2)
                rootNote = Random.Range(minRange, maxRange);

            List<int> correctids = new List<int>();
            foreach(int i in scaleQuiz[corIdx].offsets)
                correctids.Add(rootNote + i);
            //
            correctids.Add(rootNote + 12);

            if(this.paramDir.GetInt() == 1 || (this.paramDir.GetInt() == 2 && Random.Range(0, 2) == 0))
                correctids.Reverse();

            exu.SetCorrectBuffer(true, correctids.ToArray());
            fq.correctBtn = correctBtn;
            tutorialRegionObjs.replayButton.onClick.RemoveAllListeners();
            tutorialRegionObjs.replayButton.onClick.AddListener(
                () => 
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

            dn.StartCoroutine( ResetScroll( vertscroll.ScrollRect, 1.0f));
            yield return dn.StartCoroutine(fq.PlaySample(playingSamples));
            
            foreach (UnityEngine.UI.Button btn in questionButtons)
                btn.interactable = true;
            
            exu.StartQuestionTimer();
            tutorialRegionObjs.replayButton.gameObject.SetActive(true);

            while (contQuiz == true)
            {
                ExerciseUtil.Timer timeout = exu.TickClock();
            
                if (timeout == ExerciseUtil.Timer.OutOfTime || fq.answerState != BaseFloatingQuestion.AnswerState.Unknown)
                {
                    exu.EndClock();
            
                    if (fq.answerState == BaseFloatingQuestion.AnswerState.Correct)
                    {
                        exu.IncrementAnswer(true);
                        exu.NullTimer();
                    }
                    else
                    {
                        bool outoftime = (timeout == ExerciseUtil.Timer.OutOfTime);
                        exu.IncrementAnswer(false, outoftime);
                        exu.NullTimer();
            
                        string incorrectWhy =
                            (outoftime) ?
                                "Sorry! You ran out of time." :
                                "Sorry! Incorrect answer.";
            
                        if (showAnswers == true)
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
                    }
                }
                else
                {
                    yield return null;
                    continue;
                }
            
                fq.StopSamples(playingSamples);
                break;
            }

            GameObject.Destroy(host.RT.gameObject);
        }

        yield break;
    }
}
