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

public class LLDNDelay : LLDNBase
{
    ParamConnection input;
    ParamTimeLen offset;
    ParamFloat dampen;
    ParamInt voiceCt;

    public LLDNDelay()
        : base()
    { }

    public LLDNDelay(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The input audio sample to apply the delay filter to.";
        this.genParams.Add(this.input);

        this.offset = new ParamTimeLen("Offset", 0.25f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.offset.description = "The duration of time between duplicated voices.";
        this.genParams.Add(this.offset);

        this.dampen = new ParamFloat("Dampen", "Attenuation", 1.0f, 0.0f, 1.0f);
        this.dampen.description = "The compounding factor to lower the gain of each additional voice being played.";
        this.genParams.Add(this.dampen);

        this.voiceCt = new ParamInt("Voices", "Voices", 2, 2, 8);
        this.voiceCt.description = "The number of copies to create.";
        this.genParams.Add(this.voiceCt);
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

        float len = 
            this.offset.GetWavelength(freq, beatsPerSec);
            
        return 
            new PxPre.Phonics.GenDelay( 
                samplesPerSec, 
                gb, 
                this.voiceCt.value,
                len,
                this.dampen.value);
    }

    public override NodeType nodeType => NodeType.Delay;

    public override LLDNBase CloneType()
    {
        return new LLDNDelay();
    }

    public override Category GetCategory() => Category.Voices;

    public override string description => 
        "The delay node takes an audio sample and overlays it multiple times with an applied gain modification and time offset.\n\n" +
        "This is similar to the Chorus node except it does not offset the note frequencies, and does not have the same overhead involving graph hierarchy complexity.\n\n" +
        "A short duration value can be used to simulate reverb effects.\n" +
        "A longer duration value can be used to simulate echo effects.\n\n" +
        "Besides the dampening effect, this node simply adds offset versions of the input audio signal, which can cause the volume to greatly increase. It is recommended to consider putting an Amplify node afterwards to control lowering the output volume if it is too loud.";
}
