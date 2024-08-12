// <copyright file="MIDIOutputPollScope.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>
// A token that increments the output counter of a IMDIPollCountering during
// its lifetime.
// </summary>

public class MIDIOutputPollScope : System.IDisposable
{
    private IMIDIPollCountering counter = null;

    public MIDIOutputPollScope(IMIDIPollCountering counter)
    { 
        if(counter == null)
            return;

        this.counter = counter;
        this.counter.IncrementOutputPollCounter();
    }

    void System.IDisposable.Dispose()
    { 
        if(this.counter != null)
            this.counter.DecrementOutputPollCounter();

        this.counter = null;
    }
}
