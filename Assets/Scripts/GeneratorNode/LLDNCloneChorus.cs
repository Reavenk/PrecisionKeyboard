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

public class LLDNCloneChorus : LLDNBase
{
    ParamInt phonicCt;
    ParamFloat dampen;
    ParamFloat freqOff;
    ParamConnection input;

    public LLDNCloneChorus()
        : base()
    { }

    public LLDNCloneChorus(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The input audio signal to apply a chorus effect to.";
        //
        this.phonicCt = new ParamInt("Voices", "Voices", 2, 2, 8);
        this.phonicCt.description = "The number of copies to create.";
        //
        this.dampen = new ParamFloat("Dampen", "Attenuation", 1.0f, 0.0f, 1.0f);
        this.dampen.description = "A compounding volume multiplication factor for each voice generated.";
        //
        this.freqOff = new ParamFloat("Freq Offset", "Offset", -0.2f, -1.0f, 1.0f);
        this.freqOff.description = "A compounding frequency multiplier applied to the frequency of each voice generated.";

        this.genParams.Add(this.input);
        this.genParams.Add(this.phonicCt);
        this.genParams.Add(this.dampen);
        this.genParams.Add(this.freqOff);
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

        float f = freq;
        float fmod = Mathf.Pow(2.0f, this.freqOff.value);

        List<PxPre.Phonics.GenBase> gens = new List<PxPre.Phonics.GenBase>();
        for(int i = 0 ; i < phonicCt.value; ++i)
        {
            PxPre.Phonics.GenBase gb = 
                this.input.Reference.SpawnGenerator(
                    f, 
                    beatsPerSec, 
                    samplesPerSec, 
                    amp,
                    spawnFrom,
                    collection);

            gens.Add(gb);

            amp *= this.dampen.value;
            f *= fmod;
        }

        return new PxPre.Phonics.GenBatch(gens.ToArray());
    }

    public override NodeType nodeType => NodeType.Chorus;

    public override LLDNBase CloneType()
    {
        return new LLDNCloneChorus();
    }

    public override Category GetCategory() => Category.Voices;

    public override string description => 
        "Apply a chorus effect to a wiring node's heirarchy.\n\n" +
        "When playing a note, the node will make multiple duplicate copies of the node's input graph with slight different frequency values, overlapped over each other.\n\n" +
        "Note that because this is duplicating node heirarchies in the wiring instead of operating on audio signals, the more complex the graph going into a Chorus' input node, the more expensive the node will be to process.";
}
