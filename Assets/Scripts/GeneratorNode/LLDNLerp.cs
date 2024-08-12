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

public class LLDNLerp : LLDNBase
{
    ParamConnection connectionA;
    ParamConnection connectionB;
    ParamConnection connectionFactor;

    public LLDNLerp()
        : base()
    { }

    public LLDNLerp(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.connectionA = new ParamConnection("A", null);
        this.connectionA.description = "The left audio signal.";

        this.connectionB = new ParamConnection("B", null);
        this.connectionB.description = "The right audio signal.";

        this.connectionFactor = new ParamConnection("Factor", null);
        this.connectionFactor.description = "The blending factor between the left and right audio signal.\nA sample value of 0.0 will only play the left.\nA sample value of 1.0 will only play the right. A value between will interpolate between the left and right.\nA value below 0.0 or above 1.0 will extrapolate.";

        this.genParams.Add(this.connectionA);
        this.genParams.Add(this.connectionB);
        this.genParams.Add(this.connectionFactor);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection)
    { 
        if(this.connectionA.IsConnected() == false && this.connectionB.IsConnected() == false)
            return ZeroGen();

        PxPre.Phonics.GenBase ga = null;
        if (this.connectionA.IsConnected() == true)
        {
            ga = 
                this.connectionA.Reference.SpawnGenerator(
                    freq, 
                    beatsPerSec, 
                    samplesPerSec, 
                    amp,
                    spawnFrom,
                    collection);
        }

        PxPre.Phonics.GenBase gb = null;
        if (this.connectionB.IsConnected() == true)
        {
            gb = 
                this.connectionB.Reference.SpawnGenerator(
                    freq, 
                    beatsPerSec, 
                    samplesPerSec, 
                    amp,
                    spawnFrom,
                    collection);
        }
        

        PxPre.Phonics.GenBase gf = null;
        if (this.connectionFactor.IsConnected() == true)
        {
            gf = 
                this.connectionFactor.Reference.SpawnGenerator(
                    freq, 
                    beatsPerSec, 
                    samplesPerSec, 
                    amp,
                    spawnFrom,
                    collection);
        }
        else 
        { 
            // If we have both inputs but no factor, return halfsies
            if(ga != null && gb == null)
                gf = new PxPre.Phonics.GenConstant(0.5f);
            // Or else just return which-ever is valid
            else if(ga == null)
                return gb;
            else
                return ga;
        }

        if(ga == null)
            ga = ZeroGen();

        if(gb == null)
            gb = ZeroGen();

        return new PxPre.Phonics.GenLerp(ga, gb, gf);
    }

    public override NodeType nodeType => NodeType.Lerp;

    public override LLDNBase CloneType()
    {
        return new LLDNLerp();
    }

    public override Category GetCategory() => Category.Combines;

    public override string description => 
        "Linearly interpolate between two audio signals with a third audio signal used as the blending factor.\n\n" +
        "This can be used to create gates or LFOs.";
}
