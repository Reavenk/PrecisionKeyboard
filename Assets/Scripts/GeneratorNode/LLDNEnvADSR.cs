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

public class LLDNEnvADSR : LLDNBase
{
    ParamConnection input;
    ParamTimeLen paramOffset;
    ParamTimeLen paramAttackTime;
    ParamTimeLen paramDecayTime;
    ParamFloat paramSustainVal;
    ParamTimeLen paramRelease;

    public LLDNEnvADSR()
        : base()
    { }

    public LLDNEnvADSR(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The input audio signal to apply the integrated ASDR envelope to.";
        this.genParams.Add(this.input);

        this.paramOffset = new ParamTimeLen("Offset", 0.0f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.paramOffset.description = "The duration of time before input is passed and the envelope is started. Use a value of 0 to play instantly.";
        this.genParams.Add(this.paramOffset);

        this.paramAttackTime = new ParamTimeLen("Attack", 0.25f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.paramAttackTime.description = "The duration of time for the attack.\nDuring the attack, the audio sample gain is raised from 0.0 to 1.0.";
        this.genParams.Add(this.paramAttackTime);

        this.paramDecayTime = new ParamTimeLen("Decay", 0.25f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.paramDecayTime.description = "The duration of time for the decay.\nThe decay starts right after the attack. During the decay, the audio sample gain is lowered from 1.0 to the Sustain value.";
        this.genParams.Add(this.paramDecayTime);

        this.paramSustainVal = new ParamFloat("Sustain", "Volume", 0.5f, 0.0f, 1.0f);
        this.paramSustainVal.description = "The gain of the sustain. the attack and decay phase, this will be the gain until the keyboard note is released.";
        this.genParams.Add(this.paramSustainVal);

        this.paramRelease = new ParamTimeLen("Release", 0.1f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.paramRelease.description = "The duration of time for the release enevelope. This happens after the keyboard note being played is released.";
        this.genParams.Add(this.paramRelease);
    }

    public override NodeType nodeType => NodeType.EnvADSR;

    public override Category GetCategory() => Category.Envelopes;

    public override LLDNBase CloneType()
    { 
        return new LLDNEnvADSR();
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
        { 
            return 
                new PxPre.Phonics.GenADSREmpty(
                    samplesPerSec, 
                    this.paramOffset.value, 
                    this.paramAttackTime.value, 
                    this.paramDecayTime.value,
                    this.paramSustainVal.value, 
                    this.paramRelease.value);
        }

        PxPre.Phonics.GenBase gb = 
            this.input.Reference.SpawnGenerator(
                freq, 
                beatsPerSec, 
                samplesPerSec, 
                amp,
                spawnFrom,
                collection);

        return new PxPre.Phonics.GenADSR( 
            gb, 
            samplesPerSec, 
            this.paramOffset.value, 
            this.paramAttackTime.value, 
            this.paramDecayTime.value,
            this.paramSustainVal.value, 
            this.paramRelease.value);
    }

    public override string description => 
        "A fully integrated ASDR envelope.\n\n" +
        "The node provides controls to attack, sustain, decay and release of an audio signal without the need for multiple nodes for the Attack, Decay and Release.\n\n" +
        "It is advised to put this node at the end of the wiring graph (right before the output) when possible.";
}
