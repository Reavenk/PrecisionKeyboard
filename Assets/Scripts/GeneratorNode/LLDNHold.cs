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

public class LLDNHold : LLDNBase
{
    ParamConnection input = null;
    ParamTimeLen holdTime = null;

    public LLDNHold()
        : base()
    { }

    public LLDNHold(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal that will be stalled.";
        this.genParams.Add(this.input);

        this.holdTime = new ParamTimeLen("Hold", 1.0f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.holdTime.description = "The amount of time to hold the audio before letting it play.";
        this.genParams.Add(this.holdTime);
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

        float timeStall = this.holdTime.GetWavelength(freq, beatsPerSec);

        return  new PxPre.Phonics.GenHold(gb, (int)(timeStall * samplesPerSec));
    }

    public override NodeType nodeType => NodeType.Hold;

    public override LLDNBase CloneType()
    {
        return new LLDNHold();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "Hold an audio signal.\n\n" +
        "Pauses an audio signal when it first plays for a short amount of time.";

}