using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepHoldMgr : 
    MonoBehaviour, 
    ISleepTokenCounter
{
    enum Mode
    { 
        Inactive,

    }

    SleepHoldMgr instance;

    public SleepHoldMgr Instance 
    {get{ return instance; } }

    HashSet<SleepHoldToken> activeTokens = 
        new HashSet<SleepHoldToken>();

    float sleepAfter = 0.0f;

    private void Awake()
    {
        instance = this;
    }

    public void Update()
    {
        if(Time.time > this.sleepAfter && this.activeTokens.Count == 0)
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        else
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void StayAwakeFor(float seconds)
    { 
        if(seconds <= 0.0f)
            return;

        this.sleepAfter = Time.time + seconds;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public SleepHoldToken CreateHoldToken()
    { 
        return new SleepHoldToken(this);
    }

    bool ISleepTokenCounter.IncrementSleepHoldCounter(SleepHoldToken tok)
    { 
        if(this.activeTokens.Add(tok) == false)
            return false;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        return true;
    }

    bool ISleepTokenCounter.DecrementSleepHoldCounter(SleepHoldToken tok)
    { 
        if(this.activeTokens.Remove(tok) == false)
            return false;

        return true;
    }
}
