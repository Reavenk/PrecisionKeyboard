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

public class LLDNCycle : LLDNBase
{
    ParamConnection input;
    ParamTimeLen offset;
    ParamTimeLen recordAmt;

    public LLDNCycle()
        : base()
    { }

    public LLDNCycle(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The input audio to record and playback.";
        this.genParams.Add(this.input);

        this.offset = new ParamTimeLen("Offset", 0.0f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.offset.description = "The amount of time to pass before recording. Audio will still be audible during the offset time.";
        this.genParams.Add(this.offset);

        this.recordAmt = new ParamTimeLen("Amount", 1.0f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.recordAmt.description = "The amount of time to record.";
        this.genParams.Add(this.recordAmt);

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

        float offset = this.offset.GetWavelength(freq, beatsPerSec);
        float record = this.recordAmt.GetWavelength(freq, beatsPerSec);
            
        return 
            new PxPre.Phonics.GenCycle( 
                (int)(offset * samplesPerSec), 
                (int)(record * samplesPerSec), 
                gb);
    }

    public override NodeType nodeType => NodeType.Cycle;

    public override LLDNBase CloneType()
    {
        return new LLDNCycle();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "After a certain amount of time, start recording and replaying a small window of time in a loop.";
}
