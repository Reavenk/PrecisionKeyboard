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

public class LLDNConstant : LLDNBase
{
    ParamFloat floatVal = null;

    public LLDNConstant()
        : base()
    { }

    public LLDNConstant(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.floatVal = new ParamFloat("Value", "Value", 0.0f, -100.0f, 100.0f);
        this.floatVal.description = "The constant value to generate as a static audio signal.";
        this.genParams.Add(this.floatVal);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection)
    {
        return new PxPre.Phonics.GenConstant(this.floatVal.value);
    }

    public override NodeType nodeType => NodeType.Constant;

    public override LLDNBase CloneType()
    {
        return new LLDNConstant();
    }

    public override Category GetCategory() => Category.Wave;

    public override string description => 
        "Turn a constant value into an audio signal.\n\n" + 
        "The audio signal will just be the constant value repeated for all samples.\n\n" +
        "Note that because there is no variablility, a Constant node will never be generate audible audio by itself.";
}
