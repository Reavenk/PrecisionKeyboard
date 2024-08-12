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

public class LLDNEnvAttack : LLDNBase
{
    ParamConnection input = null;
    ParamTimeLen offset;
    ParamTimeLen attack;
    //ParamTimeLen offset;

    public LLDNEnvAttack()
        : base()
    {}

    public LLDNEnvAttack(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The input audio signal to apply the attack envelope to.";
        this.genParams.Add(this.input);

        this.offset = new ParamTimeLen("Offset", 0.0f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.offset.description = "The duration to wait before starting the attack.\n During that time, the audio signal will be silent.";
        this.genParams.Add(this.offset);

        this.attack = new ParamTimeLen("Attack", 0.25f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.attack.description = "The duration of the envelope.\nDuring the amount of time, the gain of the input audio signal will be raised from 0.0 to 1.0.";
        this.genParams.Add(this.attack);

        //this.offset = new ParamTimeLen("Offset", 0.0f, ParamTimeLen.TimeLenType.Seconds);
        //this.genParams.Add(this.offset);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection)
    {
        float off = this.offset.GetWavelength(freq, beatsPerSec);
        float len = this.attack.GetWavelength(freq, beatsPerSec);

        if (this.input.IsConnected() == false)
        {
            return 
                new PxPre.Phonics.GenLinAttackEmpty(
                    (int)(off * samplesPerSec),
                    (int)(len * samplesPerSec));
        }

        PxPre.Phonics.GenBase ip =
            this.input.Reference.SpawnGenerator(
                freq,
                beatsPerSec,
                samplesPerSec,
                amp,
                spawnFrom,
                collection);

        

        return new PxPre.Phonics.GenLinAttack(
            (int)(off * samplesPerSec),
            (int)(len * samplesPerSec),
            ip);
    }

    public override NodeType nodeType 
    {
        get 
        {
            return NodeType.EnvAttack;
        }
    }

    public override LLDNBase CloneType()
    { 
        return new LLDNEnvAttack();
    }

    public override Category GetCategory() => Category.Envelopes;

    public override string description => 
        "When a audio signal is started, an attack envelope can raise the gain from 0.0 to 1.0 to transition its loudness.\n\n"+
        "Even if a wiring instrument has a short attack that's only a small fraction of a second, using an Attack node (or attack in an ADSR node) is recommended to avoid transients.";
}
