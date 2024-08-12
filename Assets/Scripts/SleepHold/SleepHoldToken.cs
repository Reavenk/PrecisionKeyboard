using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepHoldToken : System.IDisposable
{
    private ISleepTokenCounter counter;

    public SleepHoldToken(ISleepTokenCounter counter)
    { 
        this.counter = counter;
        this.counter.IncrementSleepHoldCounter(this);
    }

    void System.IDisposable.Dispose()
    { 
        if(this.counter == null)
            return;

        this.counter.DecrementSleepHoldCounter(this);
        this.counter = null;
    }
}
