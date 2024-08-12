// <copyright file="ExerciseInputKey.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The exercise to identify and press keys.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseInputKey : BaseExercise
{
    public override string title => "Input Key";
    public override string description => "Training for identifying keyboard keys by note name.";

    ExerciseEnumParam paramDifficulty;

    public override bool Initialize(
        Application app, 
        ExerciseAssets assets, 
        IExerciseParamProvider sharedParams)
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

    protected override void EndOfDialogScroll(
        PxPre.UIL.Factory uifactory, 
        PxPre.UIL.EleBaseRect rect, 
        PxPre.UIL.EleBaseSizer sizer, 
        ExerciseAssets assets)
    {
        this.AddAccidentalHeader(uifactory, assets, rect, sizer);
        base.EndOfDialogScroll(uifactory, rect, sizer, assets);
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

    protected override IEnumerator ExerciseCoroutineImpl(int questions, bool showAnswers, float timer, TweenUtil tweener, ExerciseUtil exu, OctaveRange ? range)
    {
        OctaveRange or = range.Value;
        this.app.keyboardPane.DisableAllKeys();
        bool showOctave = or.min != or.max;

        switch (this.paramDifficulty.GetInt())
        {
            case 0:
                this.app.keyboardPane.SetKeyLabels(true, true, true, true);
                break;

            case 1:
                this.app.keyboardPane.SetKeyLabels(false, true, true, true);
                break;

            case 2:
                this.app.keyboardPane.SetKeyLabels(false, false, false, false);
                this.app.keyboardPane.SetKeyLabel(4, PxPre.Phonics.WesternFreqUtils.Key.C, false, true);
                break;
        }


        int octave = Random.Range(or.min, or.max + 1);
        PxPre.Phonics.WesternFreqUtils.Key k = PxPre.Phonics.WesternFreqUtils.RandomKey();

        // Exercise loop
        while (true)
        {
            UnityEngine.UI.Text txtNote;
            UnityEngine.UI.Text txtOctave;
            PxPre.UIL.EleHost host = CreateQuizRegion(showOctave, out txtNote, out txtOctave);
            host.LayoutInRT();

            DoNothing dn = host.RT.gameObject.AddComponent<DoNothing>();
            TweenUtil insideTweener = new TweenUtil(dn);
            if(txtNote != null)
                insideTweener.SlidingAnchorFade(txtNote.rectTransform, txtNote, new Vector2(-messageTweenDist, 0.0f), true, true, 0.25f );

            if(txtOctave != null)
                insideTweener.SlidingAnchorFade(txtOctave.rectTransform, txtOctave, new Vector2(-messageTweenDist, 0.0f), true, true, 0.25f );

            Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfo =
                this.app.keyboardPane.GetKeyCreationsInfo();

            txtNote.text = 
                keyCreationsInfo[k].noteName;

            exu.UpdateQuestionsRegion(true);

            if(txtOctave != null)
                txtOctave.text = octave.ToString();

            exu.StartQuestionTimer();
            pressedNoteCache.Clear();
            bool contQuiz = true;

            this.app.keyboardPane.EnableAllKeys();

            // Wait for answer and responde loop
            while (true)
            {
                ExerciseUtil.Timer timerStatus = exu.TickClock();

                if (pressedNoteCache.Count > 0 || timerStatus == ExerciseUtil.Timer.OutOfTime)
                {
                    exu.NullTimer();

                    bool correct = false;
                    if(timerStatus != ExerciseUtil.Timer.OutOfTime)
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
                    if(timerStatus == ExerciseUtil.Timer.OutOfTime)
                    { 
                        if(exu.CurrentQuestion == exu.TotalQuestions)
                            getReadyString = "One moment...";

                        rsgTimeout = true;
                    }

                    if(correct == false && showAnswers == true)
                    { 
                        // TODO: Set text to instruction of pressing key
                        this.app.keyboardPane.CenterToNoteRange(new KeyPair(k, octave));
                        rsgTimeout = true;

                        string answerString = keyCreationsInfo[k].noteName;
                        if(or.min != or.max)
                            answerString += octave.ToString();

                        string incorrectWhy = 
                            (timerStatus == ExerciseUtil.Timer.OutOfTime) ? 
                                "Sorry! You ran out of time." :
                                "Sorry! Incorrect answer.";

                        GameObject.Destroy(host.RT.gameObject);
                        host = CreateQuizRegion();
                        PxPre.UIL.UILStack uiStack =  new PxPre.UIL.UILStack(this.app.uiFactory, host);
                        uiStack.PushVertSizer(0.0f, 0.0f);
                            uiStack.AddSpace(10.0f, 1.0f, 0);
                            uiStack.AddText(incorrectWhy, 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                            uiStack.AddSpace(10.0f, 0.0f, 0);
                            uiStack.AddText($"Showing Correct Answer for pressing <b>{answerString}</b>:", 30, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
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
                            foreach(Key key in strobeKeys)
                            {
                                Color strobCol = Color.Lerp(Color.red, key.colors.normalColor, strobeLambda);
                                key.targetGraphic.color = strobCol;
                            }

                            bool cont = HasCachedNote(new KeyPair(k, octave), false, true);
                            if(cont == true)
                                break;

                            yield return null;
                        }

                        foreach(Key key in strobeKeys)
                            key.ResetColor();

                        GameObject.Destroy(host.RT.gameObject);
                    }

                    if(rsgTimeout == true)
                    {
                        this.app.keyboardPane.DisableAllKeys();

                        yield return this.ReadySetGo(
                            tweener, 
                            readySetTitle, 
                            getReadyString, 
                            tutorialRegionObjs.hostRightOffset);
                    }

                    break;
                }
                yield return null;
            }

            if (contQuiz == false)
                break;

            while(true)
            {
                int newOctave = Random.Range(or.min, or.max + 1);
                PxPre.Phonics.WesternFreqUtils.Key newK = PxPre.Phonics.WesternFreqUtils.RandomKey();

                if(newOctave != octave || newK != k)
                { 
                    octave = newOctave;
                    k = newK;
                    break;
                }
            }
        }

        float timeLen = exu.EndClock();
    }

    PxPre.UIL.EleHost CreateQuizRegion(bool showOctave, out UnityEngine.UI.Text keyTxt, out UnityEngine.UI.Text octTxt)
    {
        PxPre.UIL.EleHost ehost = CreateQuizRegion();

        PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, ehost);
        uiStack.PushVertSizer();
        uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.AlignCenter);
            uiStack.AddSpace(0.0f, 1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.AddText("Press the note: ", questionFontSz, false, 0.0f, PxPre.UIL.LFlag.AlignBot);

            uiStack.PushImage(this.app.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.AlignCenter).Chn_SetImgSliced();
                uiStack.PushHorizSizer().Chn_Border(10.0f);
                    PxPre.UIL.EleText txtNote = uiStack.AddText("M#", questionEntryFontSz, false, 1.0f, PxPre.UIL.LFlag.AlignCenter).Chn_TextAlignment(TextAnchor.MiddleCenter);
                uiStack.Pop();
            uiStack.Pop();

            keyTxt = txtNote.text;

        if (showOctave == true)
        {
            uiStack.PushImage(this.app.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.AlignCenter).Chn_SetImgSliced();
                uiStack.PushHorizSizer().Chn_Border(10.0f);
                    PxPre.UIL.EleText txtOct = uiStack.AddText("M", questionEntryFontSz, false, 1.0f, PxPre.UIL.LFlag.AlignCenter).Chn_TextAlignment(TextAnchor.MiddleCenter);
                uiStack.Pop();
            uiStack.Pop();

            octTxt = txtOct.text;
        }
        else
            octTxt = null;

            uiStack.AddSpace(0.0f, 1.0f, PxPre.UIL.LFlag.Grow);

        return ehost;
    }

    public override Sprite GetTimeIcon()
    { 
        if(this.paramTimed.GetInt() == -1)
            return null;

        return this.paramTimed.GetSprite();
    }

    public override ContentType GetExerciseType() => ContentType.KeyboardQuiz;
}
