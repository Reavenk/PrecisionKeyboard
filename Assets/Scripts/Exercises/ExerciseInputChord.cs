// <copyright file="ExerciseInputChord.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The exercise to press music chords.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseInputChord : BaseExercise
{
    public override string title => "Input Chords";
    public override string description => "Identify chords on a keyboard.";

    ExerciseEnumParam paramDifficulty;
    ExerciseEnumParam paramBreak;

    public struct ChordInfo
    { 
        public string name;
        public int [] offsets;

        public ChordInfo(string name, params int [] offsets)
        { 
            this.name = name;
            this.offsets = offsets;
        }
    }

    public override bool Initialize(Application app, ExerciseAssets assets, IExerciseParamProvider sharedParams)
    {
        base.Initialize(app, assets, sharedParams);

        this.paramBreak = sharedParams.paramBreak;

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

        this.exerciseParams.Add(this.paramTimed);
        this.exerciseParams.Add(this.paramNumber);
        this.exerciseParams.Add(this.paramShowAnswers);

        return true;
    }

    public override ContentType GetExerciseType() => ContentType.KeyboardQuiz;

    protected override OctaveRange? GetExerciseOctaveRange()
    {
        if(this.paramDifficulty.GetInt() == 0)
            return new OctaveRange(4, 4);
        else
            return new OctaveRange(4, 5);
    }

    public static List<ChordInfo> Chords = 
        new List<ChordInfo>()
        {
            new ChordInfo("Major", 0, 4, 7),
            new ChordInfo("Minor", 0, 3, 7),
            new ChordInfo("Augmented", 0, 4, 8),
            new ChordInfo("Diminished", 0, 3, 6)
        };

    protected override IEnumerator ExerciseCoroutineImpl(
        int questions, 
        bool showAnswers, 
        float timer, 
        TweenUtil tweener, 
        ExerciseUtil exu, 
        OctaveRange? range)
    {
        exu.highlightPressed = true;

        

        List<int> playingKeys = new List<int>();

        int startIdx = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 4);
        int endIdx = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 5);

        string rootMax = "MMM";
        int maxChordName = 0;
        foreach(ChordInfo ci in Chords)
            maxChordName = Mathf.Max(maxChordName, ci.name.Length);
        string chordMax = new string('M', maxChordName);

        while (true)
        {
            Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfo =
                this.app.keyboardPane.GetKeyCreationsInfo();

            this.StopKeys(playingKeys);
            this.app.keyboardPane.EnableAllKeys();

            ChordInfo ci = Chords[Random.Range(0, Chords.Count)];

            int idx = startIdx;
            if(this.paramDifficulty.GetInt() == 2)
                idx = Random.Range(startIdx, endIdx);

            PxPre.Phonics.WesternFreqUtils.Key k;
            int octave;
            PxPre.Phonics.WesternFreqUtils.GetNoteInfo(idx, out k, out octave);
            string keyName = keyCreationsInfo[k].noteName;

            List<int> choffs = new List<int>();
            foreach(int i in ci.offsets)
                choffs.Add(idx + i);
            //
            exu.SetCorrectBuffer(false, choffs.ToArray());

            if(this.paramDifficulty.GetInt() != 2)
                exu.AddKeyHighlight(idx, Color.yellow);

            PxPre.UIL.EleHost host = CreateQuizRegion(120.0f);
            PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, host);
            uiStack.PushVertSizer();
            //uiStack.AddText($"Press the chord  {ci.name}.", questionFontSz, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);

            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.AlignCenter);
                uiStack.AddText($"Press the chord ", questionFontSz, false, 0.0f, PxPre.UIL.LFlag.AlignBot);

                uiStack.PushImage(this.app.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.AlignBot).Chn_SetImgSliced();
                uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);
                    PxPre.UIL.EleText etxtRoot = uiStack.AddText(rootMax, questionFontSz, false, 0.0f, 0).Chn_TextAlignment(TextAnchor.MiddleCenter);
                uiStack.Pop();
                uiStack.Pop();

                uiStack.PushImage(this.app.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.AlignBot).Chn_SetImgSliced();
                uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);
                    PxPre.UIL.EleText etxtChord = uiStack.AddText(chordMax, questionFontSz, false, 0.0f, 0).Chn_TextAlignment(TextAnchor.MiddleCenter);
                uiStack.Pop();
                uiStack.Pop();

                uiStack.AddText($".", questionFontSz, false, 0.0f, PxPre.UIL.LFlag.AlignBot);
            uiStack.Pop();

            uiStack.AddText("Press all the keys of the scale.", false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
            uiStack.Pop();
            host.LayoutInRT();
            etxtRoot.text.text = $"{keyName}{octave}";
            etxtChord.text.text = ci.name;
            DoNothing dn = host.RT.gameObject.AddComponent<DoNothing>();
            TweenUtil insideTweener = new TweenUtil(dn);

            insideTweener.SlidingAnchorFade(etxtRoot.text.rectTransform, etxtRoot.text, new Vector2(-messageTweenDist, 0.0f), true, true, 0.25f );
            insideTweener.SlidingAnchorFade(etxtChord.text.rectTransform, etxtChord.text, new Vector2(-messageTweenDist, 0.0f), true, true, 0.25f );

            exu.UpdateQuestionsRegion(true);
            exu.StartQuestionTimer();
            pressedNoteCache.Clear();
            bool contQuiz = true;
            while (true)
            {
                exu.DoKeyHighlights();
                ExerciseUtil.Timer timeout = exu.TickClock();
                ExerciseUtil.KeyCheck kc = exu.CheckInputBuffer();

                if (timeout == ExerciseUtil.Timer.OutOfTime || kc == ExerciseUtil.KeyCheck.Incorrect)
                {
                    GameObject.Destroy(host.RT.gameObject);

                    bool outofTime =
                        (timeout == ExerciseUtil.Timer.OutOfTime);

                    string incorrectWhy =
                        (outofTime) ?
                            "Sorry! You ran out of time." :
                            "Sorry! Incorrect answer.";

                    GameObject.Destroy(host.RT.gameObject);
                    contQuiz = exu.IncrementAnswer(false, outofTime);
                    exu.EndClock();

                    if (showAnswers == true)
                    {
                        GameObject.Destroy(host.RT.gameObject);
                        host = CreateQuizRegion();
                        uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, host);
                        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
                        uiStack.AddSpace(10.0f, 1.0f, 0);
                        uiStack.AddText(incorrectWhy, 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                        uiStack.AddSpace(10.0f, 0.0f, 0);
                        uiStack.AddText($"Showing Correct Answer for, <b>{keyName}{octave} {ci.name}</b>:", 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                        uiStack.AddText("Press correct answer to continue", false, 1.0f, PxPre.UIL.LFlag.AlignCenter);
                        uiStack.AddSpace(10.0f, 1.0f, 0);
                        uiStack.Pop();
                        host.LayoutInRT(false);

                        exu.ResetCorrects(true);
                        while (exu.CheckInputBuffer() != ExerciseUtil.KeyCheck.Correct)
                        {
                            exu.DoKeyHighlights();
                            yield return null;
                        }

                        GameObject.Destroy(host.RT.gameObject);

                        yield return this.ReadySetGo(tweener, tutorialRegionObjs.hostRightOffset);
                    }
                }
                else if (kc == ExerciseUtil.KeyCheck.Correct)
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
