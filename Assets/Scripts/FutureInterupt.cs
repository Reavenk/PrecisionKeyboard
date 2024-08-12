using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FutureInterupt
{
    public bool interrupted = false;

    public void Interrupt()
    { 
        this.interrupted = true;
    }

    public bool IsInterrupted()
    { 
        return this.interrupted;
    }
}

public class FutureInteruptHold
{ 
    public FutureInterupt interrupt = null;

    public void Interrupt()
    { 
        if(this.interrupt != null)
            this.interrupt.Interrupt();
    }

    public bool IsInterrupted()
    { 
        if(this.interrupt == null)
            return false;

        return this.interrupt.IsInterrupted();
    }

    public void SetInterrupt(FutureInterupt fi)
    { 
        this.interrupt = fi;
    }
}
