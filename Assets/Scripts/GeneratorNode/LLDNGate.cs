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

public class LLDNGate : LLDNBase
{
    ParamConnection inputA;
    ParamConnection inputB;
    ParamNickname nickname;
    ParamBool gate;

    public LLDNGate()
        : base()
    { }

    public LLDNGate(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {

        this.inputA = new ParamConnection("A", null);
        this.inputA.description = "The audio signal to pass if the Gate is set to 'A'.";
        this.genParams.Add(this.inputA);

        this.inputB = new ParamConnection("B", null);
        this.inputB.description = "The audio signal to pass if the Gate is set to 'B'.";
        this.genParams.Add(this.inputB);

        this.gate = new ParamBool("Gate", "Channel", true);
        this.gate.description = "The switch to specify which audio signal to pass.";
        this.gate.SetLabels("A", "B");
        this.genParams.Add(this.gate);

        this.nickname = new ParamNickname("Name", "NoNm");
        this.nickname.description = "A short readable name to uniquely identify the gate. The name doesn't affect the gate's function, just for labeling.";
        this.genParams.Add(this.nickname);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection)
    {
        PxPre.Phonics.GenBase ret = null;

        if (this.gate.Value == true)
        {
            if(this.inputA.IsConnected() == true)
            {
                ret = 
                    this.inputA.Reference.SpawnGenerator(
                        freq, 
                        beatsPerSec, 
                        samplesPerSec, 
                        amp,
                        spawnFrom,
                        collection);
            }
        }
        else
        {
            if(this.inputB.IsConnected() == true)
            {
                ret = 
                    this.inputB.Reference.SpawnGenerator(
                        freq, 
                        beatsPerSec, 
                        samplesPerSec, 
                        amp,
                        spawnFrom,
                        collection);
            }
        }

        if(ret == null)
            return ZeroGen();

        return ret;
    }

    public override NodeType nodeType => NodeType.Gate;

    public override LLDNBase CloneType()
    {
        return new LLDNGate();
    }

    public override Category GetCategory() => Category.Special;

    public override bool VerifyConnectionUsed(ParamConnection connection)
    { 
        if(this.gate.Value == true)
        {
            return connection == this.inputA;
        }
        else
        {
            // We could probably just return true here instead of the extra check
            return connection == this.inputB;
        }
    }


    public override string description => 
        "Given two audio signals, all either one or the other to pass while muting the other.\n\n" +
        "This gate not a dynamic effect and must be changed manually through the wiring editor.\n\n" +
        "Can be used to help iterate, debug, and do A/B testing of wirings.";
}
