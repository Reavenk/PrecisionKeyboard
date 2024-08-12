using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISleepTokenCounter
{
    bool IncrementSleepHoldCounter(SleepHoldToken tok);
    bool DecrementSleepHoldCounter(SleepHoldToken tok);
}
