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

public class LLDNSmear : LLDNBase
{
    ParamConnection input = null;
    ParamFloat factor = null;

    public LLDNSmear()
        : base()
    { }

    public LLDNSmear(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal that will be smeared.";
        this.genParams.Add(this.input);

        this.factor = new ParamFloat("Factor", "Unknown", 0.5f, 0.0f, 1.0f);
        this.factor.description = "A setting that control how much to \"smear\" the audio, with a higher value adding more smear.";
        this.genParams.Add(this.factor);
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
            return null;

        PxPre.Phonics.GenBase gb = 
            this.input.Reference.SpawnGenerator(
                freq, 
                beatsPerSec, 
                samplesPerSec, 
                amp,
                spawnFrom,
                collection);

        float factor = this.factor.value;

        float integrateFactor = Mathf.Lerp(1.0f, 10.0f, factor);
        float decayFactor = Mathf.Lerp(0.9f, 0.999f, factor);

        return new PxPre.Phonics.GenSmear(gb, integrateFactor, decayFactor);
    }

    public override NodeType nodeType => NodeType.Smear;

    public override LLDNBase CloneType()
    {
        return new LLDNSmear();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "Smears a audio signal.\n\n" +
        "A smoothing operation that's rounds out an audio signal, but often at the cost of amplitude. An amplify may be needed after using to counteract any quieting that happens.";


}