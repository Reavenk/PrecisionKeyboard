// <copyright file="ExerciseIdentifyAbsolute.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The exercise to identify perfect pitch.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseInputAbsolute : BaseExercise
{
    public override string title => "Identify Absolute Notes";
    public override string description => "Ear training for identifying absolute note pitches. Aka, perfect pitch.";

    ExerciseEnumParam paramDifficulty;

    public override bool Initialize(Application app, ExerciseAssets assets, IExerciseParamProvider sharedParams)
    {
        if (base.Initialize(app, assets, sharedParams) == false)
            return false;

        this.paramDifficulty =
            new ExerciseEnumParam(
                "Difficulty",
                "How much of a challenge do you want?",
                "EXR_Int_InputDifficulty",
                true,
                new ExerciseEnumParam.Entry(0, assets.diffEasy, "Easy", "For beginners. One octave, all keys are labeled."),
                new ExerciseEnumParam.Entry(1, assets.diffMedium, "Medium", "For experienced users. Three octaves, all keys have octaves labeled."),
                new ExerciseEnumParam.Entry(2, assets.diffHard, "Hard", "For experts! Three octaves, only the octave of middle C is labeled."));

        this.exerciseParams.Add(this.paramDifficulty);
        this.exerciseParams.Add(this.paramNumber);
        this.exerciseParams.Add(this.paramShowAnswers);
        this.exerciseParams.Add(this.paramTimed);

        return true;
    }

    protected override OctaveRange? GetExerciseOctaveRange()
    {
        int minOctave = 4;
        int maxOctave = 4;
        switch (this.paramDifficulty.GetInt())
        {
            case 0:
                break;

            case 1:
                if (Random.Range(0, 2) == 1)
                {
                    minOctave = 3;
                    maxOctave = 4;
                }
                else
                {
                    minOctave = 4;
                    maxOctave = 5;
                }
                break;

            case 2:
                minOctave = 3;
                maxOctave = 5;
                break;
        }

        return new OctaveRange(minOctave, maxOctave);
    }

    protected override IEnumerator ExerciseCoroutineImpl(int questions, bool showAnswers, float timer, TweenUtil tweener, ExerciseUtil exu, OctaveRange? range)
    {
        OctaveRange or = range.Value;

        this.app.keyboardPane.SetKeyLabels(true, true, true, true);

        const float playDuration = 0.5f;
        List<int> playingKeys = new List<int>();

        while (true)
        {
            this.StopKeys(playingKeys);
            this.app.keyboardPane.DisableAllKeys();

            int octave = Random.Range(or.min, or.max + 1);
            PxPre.Phonics.WesternFreqUtils.Key k = PxPre.Phonics.WesternFreqUtils.RandomKey();

            PxPre.UIL.EleHost host = CreateQuizRegion(120.0f);
            PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, host);
            uiStack.PushVertSizer();
            PxPre.UIL.EleText etxtIdNote = uiStack.AddText("Identify the note.", questionFontSz, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
            PxPre.UIL.EleText etxtSub = uiStack.AddText("Press the key of the note you hear.", false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
            uiStack.Pop();
            host.LayoutInRT();
            DoNothing dn = host.RT.gameObject.AddComponent<DoNothing>();
            TweenUtil internalTween = new TweenUtil(dn);
            internalTween.SlidingAnchorFade(
                etxtIdNote.GetContentRect(),
                etxtIdNote.text,
                new Vector2(messageTweenDist, 0.0f),
                true,
                true,
                0.25f,
                TweenUtil.RestoreMode.RestoreLocal);

            internalTween.SlidingAnchorFade(
                etxtSub.GetContentRect(),
                etxtSub.text,
                new Vector2(-messageTweenDist, 0.0f),
                true,
                true,
                0.25f,
                TweenUtil.RestoreMode.RestoreLocal);

            tutorialRegionObjs.replayButton.onClick.RemoveAllListeners();
            tutorialRegionObjs.replayButton.onClick.AddListener(
                ()=>
                {
                    this.app.DoVibrateButton();
                    dn.StartCoroutine(exu.PlayCorrectKeys(playDuration, 0.0f, true, playingKeys));
                });

            this.app.keyboardPane.DisableAllKeys();
            tutorialRegionObjs.replayButton.interactable = false;

            yield return dn.StartCoroutine(this.PlayKeyEnum(k, octave, playDuration, playingKeys));
            this.app.keyboardPane.EnableAllKeys();

            tutorialRegionObjs.replayButton.interactable = true;
            exu.UpdateQuestionsRegion(true);
            exu.StartQuestionTimer();
            pressedNoteCache.Clear();
            bool contQuiz = true;
            while (true)
            {
                Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfo =
                    this.app.keyboardPane.GetKeyCreationsInfo();

                ExerciseUtil.Timer timerStatus = exu.TickClock();
        
                if (pressedNoteCache.Count > 0 || timerStatus == ExerciseUtil.Timer.OutOfTime)
                {
                    this.StopKeys(playingKeys);
                    exu.NullTimer();
        
                    bool correct = false;
                    if (timerStatus != ExerciseUtil.Timer.OutOfTime)
                    {
                        KeyPair kp = pressedNoteCache[0];
        
                        correct =
                            kp.key == k &&
                            kp.octave == octave;
                    }
        
        
                    if (correct == true)
                    {
                        contQuiz = exu.IncrementAnswer(true);
                    }
                    else
                    {
                        contQuiz = exu.IncrementAnswer(false, timerStatus == ExerciseUtil.Timer.OutOfTime);
                    }
        
                    pressedNoteCache.Clear();
                    GameObject.Destroy(host.RT.gameObject);
        
                    bool rsgTimeout = false;
                    string readySetTitle = "Out of Time!";
                    string getReadyString = "Get Ready!";
                    if (timerStatus == ExerciseUtil.Timer.OutOfTime)
                    {
                        if (exu.CurrentQuestion == exu.TotalQuestions)
                            getReadyString = "One moment...";
        
                        rsgTimeout = true;
                    }
        
                    if (correct == false && showAnswers == true)
                    {
                        // TODO: Set text to instruction of pressing key
                        this.app.keyboardPane.CenterToNoteRange(new KeyPair(k, octave));
                        rsgTimeout = true;
        
                        string answerString = keyCreationsInfo[k].noteName;
                        answerString += octave.ToString();
        
                        string incorrectWhy =
                            (timerStatus == ExerciseUtil.Timer.OutOfTime) ?
                                "Sorry! You ran out of time." :
                                "Sorry! Incorrect answer.";
        
                        GameObject.Destroy(host.RT.gameObject);
                        host = CreateQuizRegion();
                        uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, host);
                        uiStack.PushVertSizer(0.0f, 0.0f);
                        uiStack.AddSpace(10.0f, 1.0f, 0);
                        uiStack.AddText(incorrectWhy, 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                        uiStack.AddSpace(10.0f, 0.0f, 0);
                        uiStack.AddText($"Showing Correct Answer, <b>{answerString}</b>:", 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                        uiStack.AddText("Press correct answer to continue", false, 1.0f, PxPre.UIL.LFlag.AlignCenter);
                        uiStack.AddSpace(10.0f, 1.0f, 0);
                        uiStack.Pop();
                        host.LayoutInRT(false);
        
                        HashSet<Key> strobeKeys = new HashSet<Key>();
                        strobeKeys.Add(this.app.keyboardPane.GetKey(k, octave));
        
                        readySetTitle = "Next Question";
                        while (true)
                        {
                            float strobeLambda = Mathf.Sin(Time.time * 2.0f * 2.0f) * 0.5f + 0.5f;
                            foreach (Key key in strobeKeys)
                            {
                                Color strobCol = Color.Lerp(Color.red, key.colors.normalColor, strobeLambda);
                                key.targetGraphic.color = strobCol;
                            }
        
                            bool cont = HasCachedNote(new KeyPair(k, octave), false, true);
                            if (cont == true)
                                break;
        
                            yield return null;
                        }
        
                        foreach (Key key in strobeKeys)
                            key.ResetColor();
        
                        GameObject.Destroy(host.RT.gameObject);
                    }

                    tutorialRegionObjs.replayButton.interactable = false;
                    this.app.keyboardPane.DisableAllKeys();

                    if (rsgTimeout == true && contQuiz == true)
                    {
                        this.app.keyboardPane.DisableAllKeys();
                        yield return this.ReadySetGo(tweener, readySetTitle, getReadyString, tutorialRegionObjs.hostRightOffset);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.5f);
                        this.app.EStop(false);
                    }

                    break;
                }
                yield return null;
            }
        
            if (contQuiz == false)
                break;
        
            while (true)
            {
                int newOctave = Random.Range(or.min, or.max + 1);
                PxPre.Phonics.WesternFreqUtils.Key newK = PxPre.Phonics.WesternFreqUtils.RandomKey();
        
                if (newOctave != octave || newK != k)
                {
                    octave = newOctave;
                    k = newK;
                    break;
                }
            }
        }

        float timeLen = exu.EndClock();
    }

    protected override void EndOfDialogScroll(
        PxPre.UIL.Factory uifactory, 
        PxPre.UIL.EleBaseRect rect, 
        PxPre.UIL.EleBaseSizer sizer, 
        ExerciseAssets assets)
    { 
        base.EndOfDialogScroll(uifactory, rect, sizer, assets);

        this.AddWarningHeader(
            uifactory, 
            assets, 
            rect, 
            sizer, 
            "The effetivness of training to identify absolute pitch is contentious. It is recommended to focus on learning relative pitch (intervals) instead.");
    }

    public override bool UsesReplayButton(PaneKeyboard.ExerciseDisplayMode dm)
    {
        return true;
    }

    public override ContentType GetExerciseType() => ContentType.EarTraining;

    public override ContentIcon GetExerciseIcon()
    {
        return ContentIcon.Keyboard;
    }
}
