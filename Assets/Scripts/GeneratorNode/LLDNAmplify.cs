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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LLDNAmplify : LLDNBase
{
    ParamConnection input;
    ParamFloat gain;

    public LLDNAmplify()
        : base()
    { }

    public LLDNAmplify(string guid)
        : base(guid)
    { }

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

        return new PxPre.Phonics.GenAmplify(gb, this.gain.value);
    }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal to multiply.";
        this.genParams.Add(this.input);

        this.gain = new ParamFloat("Gain", "Multiple", 1.0f, -4.0f, 4.0f);
        this.gain.description = "The constant value to scale the audio signal by.";
        this.genParams.Add(this.gain);
    }

    public override NodeType nodeType => NodeType.Amplify;

    public override LLDNBase CloneType()
    {
        return new LLDNAmplify();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "Multiply an audio signal by a constant value.\n\n"+
        "A gain of 1.0 will result in no change. A value between 0.0 and 1.0 will make the audio quieter, and a value above 1.0 will make the audio louder.";
}
