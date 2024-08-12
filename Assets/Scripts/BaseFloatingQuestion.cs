using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFloatingQuestion
{
    public enum AnswerState
    {
        Unknown,
        Correct,
        Incorrect
    }

    protected Application app;

    public UnityEngine.UI.Button correctBtn;
    protected UnityEngine.UI.Button replayBtn;
    protected bool stepping;
    protected ExerciseUtil exu;

    public AnswerState answerState = AnswerState.Unknown;

    public BaseFloatingQuestion(Application app, UnityEngine.UI.Button correctButton, UnityEngine.UI.Button replayBtn, ExerciseUtil exu, bool stepping)
    { 
        this.app        = app;
        this.correctBtn = correctButton;
        this.replayBtn  = replayBtn;
        this.stepping   = stepping;
        this.exu        = exu;
    }

    public void SetStateFromButton(UnityEngine.UI.Button btn)
    {
        if (btn == this.correctBtn)
            answerState = AnswerState.Correct;
        else
            answerState = AnswerState.Incorrect;
    }

    public IEnumerator PlaySample(List<int> playIdxs, bool disableBtn = true)
    { 
        const float playDur = 0.3f;
        const float stepDur = 0.2f;
        const float overlapCur = 0.5f;

        if(disableBtn == true && this.replayBtn != null)
            this.replayBtn.interactable = false;

        if(this.stepping == true)
        { 
            yield return this.exu.PlayCorrectKeys(playDur, stepDur, false, playIdxs);
        }
        else
        { 
            yield return this.exu.PlayCorrectKeys(overlapCur, 0.0f, true, playIdxs);
        }

        if(disableBtn == true && this.replayBtn != null)
            this.replayBtn.interactable = true;
    }

    public void StopSamples(List<int> playIdxs)
    { 
        foreach(int i in playIdxs)
            this.app.EndNote(i);

        playIdxs.Clear();
    }
}
