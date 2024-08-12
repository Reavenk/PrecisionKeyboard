// <copyright file="ExerciseInputInterval.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The exercise to press intervals.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseInputInterval : BaseExercise
{
    public override string title => "Input Intervals";
    public override string description => "Training to identify intervals on a keyboard.";

    ExerciseEnumParam paramDifficulty;
    ExerciseEnumParam paramIntervalLabel;

    public override bool Initialize(Application app, ExerciseAssets assets, IExerciseParamProvider sharedParams)
    {
        base.Initialize(app, assets, sharedParams);

        this.paramIntervalLabel = sharedParams.paramIntervalLabel;

        this.paramDifficulty =
            new ExerciseEnumParam(
                "Difficulty",
                "How much of a challenge do you want?",
                "EXR_PressInt_Difficulty",
                true,
                new ExerciseEnumParam.Entry(0, assets.diffEasy, "Easy", "For beginners. Four options. Root note labeled, offset given."),
                new ExerciseEnumParam.Entry(1, assets.diffMedium, "Medium", "For experienced users. All options. Root note labeled."),
                new ExerciseEnumParam.Entry(2, assets.diffHard, "Hard", "For experts! Thirteen options. All options. Root note unlabeled."));

        this.exerciseParams.Add(this.paramDifficulty);
        this.exerciseParams.Add(this.paramIntervalLabel);

        this.exerciseParams.Add(this.paramTimed);
        this.exerciseParams.Add(this.paramNumber);
        this.exerciseParams.Add(this.paramShowAnswers);

        return true;
    }

    protected override OctaveRange? GetExerciseOctaveRange()
    {
        return new OctaveRange(4, 5);
    }

    protected override IEnumerator ExerciseCoroutineImpl(
        int questions, 
        bool showAnswers, 
        float timer, 
        TweenUtil tweener, 
        ExerciseUtil exu,
         OctaveRange? range)
    {
        int minRange = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 4);
        int maxRange = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.B, 4);

        List<ExerciseIdentifyInterval.CondensedInterval> lstci = 
            ExerciseIdentifyInterval.GetLabels(this.paramIntervalLabel.GetInt());

        bool contQuiz = true;
        while (contQuiz == true)
        {
            exu.StartQuestionTimer();
            this.app.keyboardPane.DisableAllKeys();

            ExerciseIdentifyInterval.CondensedInterval[] rintUse = null;
            if (this.paramDifficulty.GetInt() == 0)
            {
                rintUse =
                    new ExerciseIdentifyInterval.CondensedInterval[]
                    {
                        lstci[0],
                        lstci[1],
                        lstci[2],
                        lstci[3],
                        lstci[4],
                        lstci[lstci.Count - 1],
                    };
            }
            else
                rintUse = lstci.ToArray();

            Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfo = 
                this.app.keyboardPane.GetKeyCreationsInfo();

            ExerciseIdentifyInterval.CondensedInterval qInt = rintUse[Random.Range(0, rintUse.Length)];
            //
            int baseNote = Random.Range(minRange, maxRange);
            int bnOctave;
            PxPre.Phonics.WesternFreqUtils.Key bnKey;
            PxPre.Phonics.WesternFreqUtils.GetNoteInfo(baseNote, out bnKey, out bnOctave);
            string bnName = keyCreationsInfo[bnKey].noteName;
            //
            int answer = baseNote + qInt.offset;
            int ansOctave;
            PxPre.Phonics.WesternFreqUtils.Key ansKey;
            PxPre.Phonics.WesternFreqUtils.GetNoteInfo(answer, out ansKey, out ansOctave);
            string ansName = keyCreationsInfo[ansKey].noteName;

            bool highlight = this.paramDifficulty.GetInt() <= 1;
            if(highlight == true)
                exu.AddKeyHighlight(baseNote, Color.green);

            exu.SetCorrectBuffer(true, new KeyPair(ansKey, ansOctave));

            PxPre.UIL.EleHost host = CreateQuizRegion();
            PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, host);
            uiStack.AddVertSizer();
            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz | PxPre.UIL.LFlag.GrowVertOnCollapse);

            exu.UpdateQuestionsRegion(true);

            DoNothing dn = host.RT.gameObject.AddComponent<DoNothing>();
            TweenUtil insideTweener = new TweenUtil(dn);

            // FIGURE OUT THE MAX SIZE OF AD-LIB PLATES
            string noteMaxStr = "MMM";
            int intervalMaxCt = 0;
            foreach(ExerciseIdentifyInterval.CondensedInterval ci in rintUse)
                intervalMaxCt = Mathf.Max(intervalMaxCt, ci.name.Length);
            string intervalMaxStr = new string('M', intervalMaxCt);
            
            const int questionFontSize = 50;

            // ADD 
            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.AlignHorizCenter);
                //PxPre.UIL.EleText eleTitle = uiStack.AddText($"Interval <b>{qInt.name}</b> of {bnName}{bnOctave}.", 50, true, 0.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);

                PxPre.UIL.EleText eleTitle = uiStack.AddText($"Interval ", questionFontSize, false, 0.0f,  PxPre.UIL.LFlag.AlignBot);

                uiStack.AddSpace(0.0f, 1.0f, 0);
                uiStack.PushImage( this.app.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.AlignBot).Chn_SetImgSliced();
                uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);
                    PxPre.UIL.EleText eleTxtInt = uiStack.AddText(intervalMaxStr, questionFontSize, false, 1.0f, 0 ).Chn_TextAlignment(TextAnchor.MiddleCenter);
                uiStack.Pop();
                uiStack.Pop();

                uiStack.AddText(" of ", questionFontSize, false, 1.0f, PxPre.UIL.LFlag.AlignBot );

                uiStack.PushImage( this.app.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.AlignBot).Chn_SetImgSliced();
                uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);
                    PxPre.UIL.EleText eleTxtRoot = uiStack.AddText(noteMaxStr, questionFontSize, false, 1.0f, 0 ).Chn_TextAlignment(TextAnchor.MiddleCenter);
                uiStack.Pop();
                uiStack.Pop();

                uiStack.AddText(".", questionFontSize, false, 1.0f, PxPre.UIL.LFlag.AlignBot );
                uiStack.AddSpace(0.0f, 1.0f, 0);

            uiStack.Pop();

            PxPre.UIL.EleText eleSub = uiStack.AddText("Press the interval.", true, 0.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);

            host.LayoutInRT(false);
            insideTweener.SlidingAnchorFade(eleTxtInt.text.rectTransform, eleTxtInt.text, new Vector2(-messageTweenDist, 0.0f), true, true, 0.25f );
            insideTweener.SlidingAnchorFade(eleTxtRoot.text.rectTransform, eleTxtRoot.text, new Vector2(-messageTweenDist, 0.0f), true, true, 0.25f );
            insideTweener.SlidingAnchorFade(eleSub.text.rectTransform, eleSub.text, new Vector2(messageTweenDist, 0.0f), true, true, 0.25f);

            eleTxtInt.text.text = qInt.name;
            eleTxtRoot.text.text = $"{bnName}{bnOctave}";

            this.app.keyboardPane.EnableAllKeys();
            BaseExercise.pressedNoteCache.Clear();
            while (true)
            {
                exu.DoKeyHighlights();
                ExerciseUtil.Timer timeout = exu.TickClock();
                ExerciseUtil.KeyCheck kc = exu.CheckInputBuffer();

                if(timeout == ExerciseUtil.Timer.OutOfTime || kc == ExerciseUtil.KeyCheck.Incorrect)
                {

                    bool outofTime = 
                        (timeout == ExerciseUtil.Timer.OutOfTime);

                    GameObject.Destroy(host.RT.gameObject);
                    contQuiz = exu.IncrementAnswer(false, outofTime);
                    exu.EndClock();

                    if(showAnswers == true)
                    {
                        string incorrectWhy = 
                            (timeout == ExerciseUtil.Timer.OutOfTime) ? 
                                "Sorry! You ran out of time." :
                                "Sorry! Incorrect answer.";

                        host = CreateQuizRegion();
                        uiStack =  new PxPre.UIL.UILStack(this.app.uiFactory, host);
                        uiStack.PushVertSizer(0.0f, 0.0f);
                            uiStack.AddSpace(10.0f, 1.0f, 0);
                            uiStack.AddText(incorrectWhy, 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                            uiStack.AddSpace(10.0f, 0.0f, 0);
                            uiStack.AddText($"Showing Correct Answer for interval <b>{qInt.name}</b> of <b>{bnName}{bnOctave}</b>:", 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                            uiStack.AddText("Press correct answer to continue", false, 1.0f, PxPre.UIL.LFlag.AlignCenter);
                            uiStack.AddSpace(10.0f, 1.0f, 0);
                        uiStack.Pop();
                        host.LayoutInRT(false);


                        exu.ResetCorrects(true);
                        while(exu.CheckInputBuffer() != ExerciseUtil.KeyCheck.Correct)
                        {
                            exu.DoKeyHighlights();
                            yield return null;
                        }

                        GameObject.Destroy(host.RT.gameObject);
                        yield return this.ReadySetGo(tweener, tutorialRegionObjs.hostRightOffset);
                    }
                }
                else if(kc == ExerciseUtil.KeyCheck.Correct)
                {
                    GameObject.Destroy(host.RT.gameObject);
                    contQuiz = exu.IncrementAnswer(true);
                    exu.EndClock();
                }
                else
                {
                    yield return null;
                    continue;
                }

                exu.ResetKeyHighlights();
                break;
            }
        }
    }

    public override ContentType GetExerciseType() => ContentType.KeyboardQuiz;
}
