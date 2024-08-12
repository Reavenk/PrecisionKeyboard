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

public class LLDNCombSub : LLDNBase
{
    ParamConnection connectionA = null;
    ParamConnection connectionB = null;

    public LLDNCombSub()
    {
    }

    public LLDNCombSub(string guid)
        : base( guid)
    { }

    protected override void _Init()
    {
        this.connectionA = new ParamConnection("A", null);
        this.connectionA.description = "The first audio signal.";
        //
        this.connectionB = new ParamConnection("B", null);
        this.connectionB.description = "The second audio signal.";

        this.genParams.Add(this.connectionA);
        this.genParams.Add(this.connectionB);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp, 
        WiringDocument spawnFrom,
        WiringCollection collection)
    {
        if (this.connectionA.IsConnected() == false && this.connectionA.IsConnected() == false)
            return ZeroGen();

        if (this.connectionA.IsConnected() == false)
        {
            return 
                this.connectionB.Reference.SpawnGenerator(
                    freq, 
                    beatsPerSec, 
                    samplesPerSec, 
                    amp,
                    spawnFrom,
                    collection);
        }

        if(this.connectionB.IsConnected() == false)
        {
            return 
                this.connectionA.Reference.SpawnGenerator(
                    freq,
                    beatsPerSec,
                    samplesPerSec,
                    amp,
                    spawnFrom,
                    collection);
        }

        PxPre.Phonics.GenBase gba = 
            this.connectionA.Reference.SpawnGenerator(
                freq, 
                beatsPerSec, 
                samplesPerSec, 
                amp,
                spawnFrom,
                collection);

        PxPre.Phonics.GenBase gbb = this.connectionB.Reference.SpawnGenerator(
            freq, 
            beatsPerSec, 
            samplesPerSec, 
            amp,
            spawnFrom,
            collection);

        return
            new PxPre.Phonics.GenSub(
                gba,
                gbb);
    }

    public override NodeType nodeType 
    { 
        get 
        {
            return NodeType.CombSub;
        } 
    }

    public override LLDNBase CloneType()
    { 
        return new LLDNCombSub();
    }

    public override Category GetCategory() => Category.Combines;

    public override string description => 
        "Given two audio signals, create a new audio signal from subtracting one signal from the other.\n\n" +
        "If this is used to overlap audio, this is similar to adding audio signals and often sounds the same - quality-wise, but can be used for phase cancellation effects.";
}
