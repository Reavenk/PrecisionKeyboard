// MIT License
//
// Copyright(c) 2020 Pixel Precision LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;

public class LLDNStutter : LLDNBase
{
    ParamConnection input = null;
    ParamTimeLen stutterTime = null;
    ParamFloat stutterPercent = null;
    ParamBool pass = null;
    ParamBool start = null;

    public LLDNStutter()
        : base()
    { }

    public LLDNStutter(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal that will stuttered.";
        this.genParams.Add(this.input);

        this.stutterTime = new ParamTimeLen("Window", 1.0f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.stutterTime.description = "The rate at which stutters happen";
        this.genParams.Add(this.stutterTime);

        this.stutterPercent = new ParamFloat("Percent", "Percent", 0.5f, 0.0f, 1.0f);
        this.stutterPercent.description = "The percent of the window between playing the audio and stuttering.";
        this.genParams.Add(this.stutterPercent);

        this.pass = new ParamBool("Pass", "Stutter Behaviour", false);
        this.pass.trueString = "Pass";
        this.pass.falseString = "Hold";
        this.pass.description = "If true, when the stutter is silent, it will still pass the audio through instead of holding it - resulting in a gate effect.";
        this.genParams.Add(this.pass);

        this.start = new ParamBool("Start", "Play on start?", false);
        this.start.trueString = "Play";
        this.start.falseString = "Stut";
        this.start.description = "If true, the node starts by playing; if false the node starts by stuttering.";
        this.genParams.Add(this.start);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection)
    { 
        if(this.input.IsConnected() == false)
            return ZeroGen();

        PxPre.Phonics.GenBase gb = 
            this.input.Reference.SpawnGenerator(
                freq, 
                beatsPerSec, 
                samplesPerSec, 
                amp,
                spawnFrom,
                collection);

        bool passOnStut = this.pass.value;
        bool startOnPlay = this.start.value;

        float stutterTime = this.stutterTime.GetWavelength(freq, beatsPerSec);
        float stutterPer = this.stutterPercent.value;

        // If the parameter is set to something ineffective,
        // just bypass the stutter and optimize it out.
        if(stutterPer == 1.0f)
        { 
            if(startOnPlay == true)
                return gb;
            else
                return ZeroGen();
        }
        if(stutterPer == 0.0f)
        {
            if(startOnPlay == true)
                return ZeroGen();
            else
                return gb;
        }

        int windowSamples = (int)(stutterTime * samplesPerSec);
        windowSamples = Mathf.Max(windowSamples, 2);
        //
        int playSamples = (int)(stutterPer * windowSamples);
        playSamples = Mathf.Max(playSamples, 1);
        //
        int stutterSamples = windowSamples - playSamples;


        if(passOnStut == true)
        {
            return 
                new PxPre.Phonics.GenStutterPass(
                    gb, 
                    playSamples, 
                    stutterSamples, 
                    startOnPlay);
        }
        else
        { 
            return 
                new PxPre.Phonics.GenStutterHold(
                    gb, 
                    playSamples, 
                    stutterSamples, 
                    startOnPlay);
        }
    }

    public override NodeType nodeType => NodeType.Stutter;

    public override LLDNBase CloneType()
    {
        return new LLDNStutter();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "Stutters and audio signal.\n\n" +
        "Chops up an audio signal at regular intervals.";
}
