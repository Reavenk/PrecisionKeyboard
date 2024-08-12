// <copyright file="MIDIInputPollScope.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>
// A token that increments the input counter of a IMIDIPollCountering during
// its lifetime. 
// </summary>

public class MIDIInputPollScope : System.IDisposable
{
    private IMIDIPollCountering counter; 

    public MIDIInputPollScope(IMIDIPollCountering counter)
    { 
        if(counter == null)
            return;

        this.counter = counter;
        this.counter.IncrementInputPollCounter();
    }

    void System.IDisposable.Dispose()
    { 
        if(this.counter == null)
            this.counter.DecrementInputPollCounter();

        this.counter = null;
    }
}
