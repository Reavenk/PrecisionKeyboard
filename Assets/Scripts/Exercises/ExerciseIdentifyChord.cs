// <copyright file="ExerciseIdentifyChord.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>Exercise to identify chords.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseIdentifyChord : BaseExercise
{
    public override string title => "Identify Chords";
    public override string description => "Ear training for identifying chords.";

    ExerciseEnumParam paramDifficulty;
    ExerciseEnumParam paramIntervalLabel;
    ExerciseEnumParam paramBreak;

    public override ContentType GetExerciseType() => ContentType.EarTraining;

    public override PaneKeyboard.ExerciseDisplayMode GetDisplayMode()
    {
        return PaneKeyboard.ExerciseDisplayMode.PlainExercise;
    }

    public override bool UsesReplayButton(PaneKeyboard.ExerciseDisplayMode dm)
    {
        return true;
    }

    public override bool Initialize(Application app, ExerciseAssets assets, IExerciseParamProvider sharedParams)
    {
        if (base.Initialize(app, assets, sharedParams) == false)
            return false;

        this.paramDifficulty =
            new ExerciseEnumParam(
                "Difficulty",
                "How much of a challenge do you want?",
                "EXR_IdCh_InputDifficulty",
                true,
                new ExerciseEnumParam.Entry(0, assets.diffEasy, "Easy", "For beginners. One octave, all keys are labeled."),
                new ExerciseEnumParam.Entry(1, assets.diffMedium, "Medium", "For experienced users. Three octaves, all keys have octaves labeled."),
                new ExerciseEnumParam.Entry(2, assets.diffHard, "Hard", "For experts! Three octaves, only the octave of middle C is labeled."));

        this.paramIntervalLabel = sharedParams.paramIntervalLabel;

        this.paramBreak = sharedParams.paramBreak;

        this.exerciseParams.Add(this.paramDifficulty);
        this.exerciseParams.Add(this.paramIntervalLabel);
        this.exerciseParams.Add(this.paramBreak);
        this.exerciseParams.Add(this.paramNumber);
        this.exerciseParams.Add(this.paramShowAnswers);
        this.exerciseParams.Add(this.paramTimed);

        return true;
    }

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

        string lastChordName = string.Empty;

        bool contQuiz = true;
        while(contQuiz == true)
        {
            PxPre.UIL.EleHost host = CreateQuizRegion();
            DoNothing dn = host.RT.gameObject.AddComponent<DoNothing>();
            PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, host);
            uiStack.AddVertSizer();
            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz | PxPre.UIL.LFlag.GrowVertOnCollapse);
            PxPre.UIL.EleText eleTitle = uiStack.AddText("Identify the triad chord.", 50, true, 0.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);
            PxPre.UIL.EleText eleSub = uiStack.AddText("Select what you hear.", true, 0.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);
            uiStack.AddSpace(10.0f, 0.0f, 0);

            TweenUtil correctionTween = 
                new TweenUtil(dn);

            List<ExerciseInputChord.ChordInfo> rci = ExerciseInputChord.Chords;
            int corIdx = Random.Range(0, rci.Count);

            // If it's the same chord, let's reroll (at least) once to allow back-to-back duplicates,
            // but to lower their chances of occuring.
            if(lastChordName == rci[corIdx].name)
                corIdx = Random.Range(0, rci.Count);

            UnityEngine.UI.Button correctBtn = null;

            PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> vertscroll = 
                uiStack.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(1.0f, PxPre.UIL.LFlag.GrowOnCollapse | PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.Grow);

            uiStack.PushGridSizer(2);

            // TODO: Vary reoot note if needed.
            int rootNote = minRange;

            BaseFloatingQuestion fq = 
                new BaseFloatingQuestion(
                    this.app, 
                    null, 
                    tutorialRegionObjs.replayButton,
                    exu,
                    this.paramBreak.GetInt() == 0);

            fq.StopSamples(playingSamples);

            List<UnityEngine.UI.Button> questionButtons = new List<UnityEngine.UI.Button>();

            for (int i = 0; i < rci.Count; ++i)
            {
                const float buttonWidth = 300.0f;

                PxPre.UIL.EleGenButton<PushdownButton> btn = 
                    uiStack.PushButton<PushdownButton>(rci[i].name, 0.0f, 0);

                questionButtons.Add(btn.Button);
                btn.minSize = new Vector2(buttonWidth, 0.0f);
                UnityEngine.UI.Button btnObj = btn.Button;
                btn.Button.onClick.AddListener(() => { fq.SetStateFromButton(btnObj); });

                uiStack.Pop();

                if (i == corIdx)
                    correctBtn = btn.Button;
            }
            dn.StartCoroutine(ResetScroll(vertscroll.ScrollRect, 1.0f));

            List<int> correctNotes = new List<int>();
            ExerciseInputChord.ChordInfo corCh = rci[corIdx];
            foreach(int i in corCh.offsets)
                correctNotes.Add(rootNote + i);
            exu.SetCorrectBuffer(true, correctNotes.ToArray());

            foreach (UnityEngine.UI.Button btn in questionButtons)
                btn.interactable = false;

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

            exu.UpdateQuestionsRegion(true);
            yield return new WaitForSeconds(0.2f);
            yield return dn.StartCoroutine(fq.PlaySample(playingSamples));

            foreach (UnityEngine.UI.Button btn in questionButtons)
                btn.interactable = true;

            exu.StartQuestionTimer();
            tutorialRegionObjs.replayButton.gameObject.SetActive(true);

            const float nextInstQuestionDelay = 0.5f;
            while (contQuiz == true)
            {
                ExerciseUtil.Timer timeout = exu.TickClock();

                if (timeout == ExerciseUtil.Timer.OutOfTime || fq.answerState != BaseFloatingQuestion.AnswerState.Unknown)
                {
                    exu.EndClock();

                    if (fq.answerState == BaseFloatingQuestion.AnswerState.Correct)
                    {
                        contQuiz = exu.IncrementAnswer(true);
                        fq.StopSamples(playingSamples);
                        yield return new WaitForSeconds(nextInstQuestionDelay);
                    }
                    else
                    {
                        bool outoftime = (timeout == ExerciseUtil.Timer.OutOfTime);
                        contQuiz = exu.IncrementAnswer(false, outoftime);
                        
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
                        else
                        {
                            fq.StopSamples(playingSamples);
                            yield return new WaitForSeconds(nextInstQuestionDelay);
                        }
                    }
                    GameObject.Destroy(host.RT.gameObject);
                }
                else
                { 
                    yield return null;
                    continue;
                }

                fq.StopSamples(playingSamples);
                break;
            }
            

        }

        yield break;
    }
}
