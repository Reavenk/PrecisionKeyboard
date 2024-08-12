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

public class LLDNEnvDecay : LLDNBase
{
    ParamConnection input;
    ParamTimeLen offset;
    ParamTimeLen duration;
    ParamFloat sustain;

    public LLDNEnvDecay()
        : base()
    {}

    public LLDNEnvDecay(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal to apply the decay envelope to.";
        this.genParams.Add(this.input);

        this.offset = new ParamTimeLen("Offset", 0.25f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.offset.description = "The duration of time the decay envelope will be active.\nWhen the envelope starts, the input gain will be 1.0.\nWhen the envelope is finished, the input signal will be the value of Sustain.";
        this.genParams.Add(this.offset);

        this.duration = new ParamTimeLen("Decay", 0.25f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.duration.description = "The amount of time the note will play before the envelope is started.";
        this.genParams.Add(this.duration);

        this.sustain = new ParamFloat("Sustain", "Volume", 0.5f, 0.0f, 1.0f);
        this.sustain.description = "The gain of the input signal after the envlope is finished.";
        this.genParams.Add(this.sustain);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection)
    {
        float len = 
            this.duration.GetWavelength(freq, beatsPerSec);

        if (this.input.IsConnected() == false)
        { 
            return 
                new PxPre.Phonics.GenLinDecayEmpty(
                    this.offset.value, 
                    len,
                    this.sustain.value,
                    samplesPerSec);
        }

        PxPre.Phonics.GenBase ip = 
            this.input.Reference.SpawnGenerator(
                freq,
                beatsPerSec,
                samplesPerSec, 
                amp,
                spawnFrom,
                collection);

        return new PxPre.Phonics.GenLinDecay(
            this.offset.value, 
            len,
            this.sustain.value,
            samplesPerSec,
            ip);
    }

    public override NodeType nodeType => NodeType.EnvDecay;

    public override LLDNBase CloneType()
    { 
        return new LLDNEnvDecay();
    }

    public override Category GetCategory() => Category.Envelopes;

    public override string description => 
        "Applies a decay envelope to an audio signal.\n\n" +
        "After an input signal plays at full gain for a certain amount of time, it will be lowered to a target sustain gain over a specific amount of time.\n\n" +
        "Note that this is different from the full ASDR decay, in that it will take effect after the specified amount of time has passed, even if there are other envelope effects.";
}
