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

public class LLDNInvert : LLDNBase
{
    ParamConnection input = null;

    public LLDNInvert()
        : base()
    { }

    public LLDNInvert(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal that will be inverted.";
        this.genParams.Add(this.input);
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

        return 
            new PxPre.Phonics.GenInvert(gb);
    }

    public override NodeType nodeType => NodeType.Invert;

    public override LLDNBase CloneType()
    {
        return new LLDNInvert();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "Inverts an audio signal.\n\n" +
        "Similar to negative the audio signal but pivoted at the value of 0.5, so that value of 1.0"+
        "will be 0.0, and value of 0.0 will be at 1.0 - with all other value on the numberline changed" +
        "accordingly.";

}
