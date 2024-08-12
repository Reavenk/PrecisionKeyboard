// <copyright file="ExerciseInputScale.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The exercise to input scales.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseInputScale : BaseExercise
{
    public override string title => "Input Scales";
    public override string description => "Enter scales on the keyboard.";

    ExerciseEnumParam paramDifficulty;
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
                "EXR_InputScale_Difficulty",
                true,
                new ExerciseEnumParam.Entry(0, assets.diffEasy,     "Easy",     "C4 always used as the root. Highlight keys and list intervals."),
                new ExerciseEnumParam.Entry(1, assets.diffMedium,   "Medium",   "Random roots. Intervals will be listed for reference."),
                new ExerciseEnumParam.Entry(2, assets.diffHard,     "Hard",     "For experts! Random roots, hints."));

        this.paramDir = sharedParams.paramDirection;

        this.exerciseParams.Add(this.paramDifficulty);
        this.exerciseParams.Add(this.paramDir);

        this.exerciseParams.Add(this.paramTimed);
        this.exerciseParams.Add(this.paramNumber);
        this.exerciseParams.Add(this.paramShowAnswers);

        return true;
    }

    protected override OctaveRange? GetExerciseOctaveRange()
    {
        return new OctaveRange(4, 5);
    }

    public override ContentType GetExerciseType() => ContentType.KeyboardQuiz;

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

        List<MusicScale> scales = 
            new List<MusicScale>()
            {
                MusicScale.ScaleMajor,
                MusicScale.ScaleMinor,
                MusicScale.ScaleHungarianMinor,
                MusicScale.ScaleArabic,
                MusicScale.ScalePersian
            };

        string rootMax = "MMM";
        int maxScaleLen = 0;
        foreach(MusicScale ms in scales)
            maxScaleLen = Mathf.Max(maxScaleLen, ms.name.Length);
        string scaleMax = new string('M', maxScaleLen);

        bool highlight = this.paramDifficulty.GetInt() < 2;
        bool showOffs = this.paramDifficulty.GetInt() == 0;

        bool contQuiz = true;
        while(contQuiz == true)
        {
            Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfo =
                    this.app.keyboardPane.GetKeyCreationsInfo();

            exu.StartQuestionTimer();
            this.app.keyboardPane.DisableAllKeys();

            MusicScale curScale = scales[Random.Range(0, scales.Count)];

            int baseNote = minRange;
            if(this.paramDifficulty.GetInt() != 0)
                baseNote = Random.Range(minRange, maxRange);

            List<int> correctNotes = new List<int>(curScale.offsets);
            correctNotes.Add(12);
            for(int i = 0; i < correctNotes.Count; ++i)
                correctNotes[i] += baseNote;
            //
            int dirInt = paramDir.GetInt();
            string dirName = "Ascending";
            if (dirInt == 1 || (dirInt == 2 && Random.Range(0,2) == 0))
            {
                correctNotes.Reverse();
                dirName = "Descending";
            }

            PxPre.Phonics.WesternFreqUtils.Key k;
            int octave;
            PxPre.Phonics.WesternFreqUtils.GetNoteInfo(baseNote, out k, out octave);
            string kName = keyCreationsInfo[k].noteName;

            exu.SetCorrectBuffer(true, correctNotes.ToArray());

            baseNote = correctNotes[0];
            exu.UpdateQuestionsRegion(true);

            PxPre.UIL.EleHost host = CreateQuizRegion();
            PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, host);
            uiStack.AddVertSizer();
            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz | PxPre.UIL.LFlag.GrowVertOnCollapse);

            int questionFontSz = 50;
            //PxPre.UIL.EleText eleTitle = uiStack.AddText($"{dirName} scale <b>{curScale.name}</b> of {kName}{octave}.", questionFontSz, true, 0.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);


            uiStack.PushHorizSizer(0,  PxPre.UIL.LFlag.AlignHorizCenter);
                PxPre.UIL.EleText eleTitle = uiStack.AddText($"{dirName} scale ", questionFontSz, false, 0.0f, PxPre.UIL.LFlag.AlignBot);

                uiStack.PushImage(this.app.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.AlignBot).Chn_SetImgSliced();
                uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);
                    PxPre.UIL.EleText etxtRoot = uiStack.AddText(rootMax, questionFontSz, false, 0.0f, PxPre.UIL.LFlag.Grow).Chn_TextAlignment(TextAnchor.MiddleCenter);
                uiStack.Pop();
                uiStack.Pop();

                uiStack.PushImage(this.app.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.AlignBot).Chn_SetImgSliced();
                uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);
                    PxPre.UIL.EleText etxtScale = uiStack.AddText(scaleMax, questionFontSz, false, 0.0f, PxPre.UIL.LFlag.Grow).Chn_TextAlignment(TextAnchor.MiddleCenter);
                uiStack.Pop();
                uiStack.Pop();
            uiStack.Pop();


            PxPre.UIL.EleText eleSub = uiStack.AddText("Enter the scale <b>in order</b>.", true, 0.0f, PxPre.UIL.LFlag.AlignCenter | PxPre.UIL.LFlag.GrowHoriz).Chn_TextAlignment(TextAnchor.MiddleCenter);
            host.LayoutInRT(false);

            etxtRoot.text.text = $"{kName}{octave}";
            etxtScale.text.text = curScale.name;

            DoNothing dn = host.RT.gameObject.AddComponent<DoNothing>();
            TweenUtil insideTweener = new TweenUtil(dn);
            insideTweener.SlidingAnchorFade(etxtRoot.text.rectTransform, etxtRoot.text, new Vector2(-messageTweenDist, 0.0f), true, true, 0.25f );
            insideTweener.SlidingAnchorFade(etxtScale.text.rectTransform, etxtScale.text, new Vector2(-messageTweenDist, 0.0f), true, true, 0.25f );


            this.app.keyboardPane.EnableAllKeys();
            exu.UpdateQuestionsRegion(true);

            while (true)
            { 
                exu.DoKeyHighlights();
                ExerciseUtil.Timer timeout = exu.TickClock();
                ExerciseUtil.KeyCheck kc = exu.CheckInputBuffer();

                if(timeout == ExerciseUtil.Timer.OutOfTime || kc == ExerciseUtil.KeyCheck.Incorrect)
                {
                    bool outOfTime =
                        (timeout == ExerciseUtil.Timer.OutOfTime);

                    GameObject.Destroy(host.RT.gameObject);
                    contQuiz = exu.IncrementAnswer(false, outOfTime);
                    exu.EndClock();

                    if(showAnswers == true)
                    { 
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
}
