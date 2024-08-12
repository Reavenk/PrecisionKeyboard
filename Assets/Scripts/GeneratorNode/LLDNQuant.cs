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

public class LLDNQuant : LLDNBase
{
    ParamConnection input = null;
    ParamFloat levels = null;

    public LLDNQuant()
        : base()
    { }

    public LLDNQuant(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal to quantize.";
        this.genParams.Add(this.input);

        this.levels = new ParamFloat("Levels", "Divisions", 4.0f, 1.0f, 50.0f);
        this.levels.description = "The number of quantum levels to divide the audio signal by.";
        this.genParams.Add(this.levels);
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

        return new PxPre.Phonics.GenQuant(gb, this.levels.value);
    }

    public override NodeType nodeType => NodeType.Quant;

    public override LLDNBase CloneType()
    {
        return new LLDNQuant();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "Quantize the audio stream.\n\n" +
        "Also refered to a 'posterizing' or 'bit crushing'. The audio signals will be forced to stairstepped values.\n\n" +
        "Note that the node expects to work on a full range between -1.0 to 1.0. If a wave generator feeding into the Quant node is not in that range, it is recommended to set it to, and later change the volume with an 'Amplify' node.";
}
