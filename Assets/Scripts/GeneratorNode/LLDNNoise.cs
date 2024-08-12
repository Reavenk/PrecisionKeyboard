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

public class LLDNNoise : LLDNBase
{
    protected ParamFloat gain;

    public LLDNNoise()
        : base()
    { }

    public LLDNNoise(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.gain = new ParamFloat("Gain", "Volume", 0.8f, 0.0f, 1.0f, "clampeddial");
        this.gain.description = "The volume of the audio signal.";
        this.genParams.Add(this.gain);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection)
    {
        return
            new PxPre.Phonics.GenWhite(
                amp * this.gain.value);
    }

    public override NodeType nodeType 
    {
        get {
            return NodeType.NoiseWave;
        }
    }

    public override LLDNBase CloneType()
    { 
        return new LLDNNoise();
    }

    public override Category GetCategory() => Category.Wave;

    public override string description => 
        "Generate a white noise audio signal.\n\n"+
        "The noise will sound the same no matter what key is pressed - by definition that's what white noise is.";

    public override bool CanMakeNoise() => true;
}
