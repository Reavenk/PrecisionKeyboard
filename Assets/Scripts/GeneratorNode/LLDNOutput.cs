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

using System.Collections.Generic;

public class LLDNOutput : LLDNBase
{
    // While we can never pull up the output documentation, we still add it in for
    // rigor, and because that may change in the future.

    ParamConnection conInput;

    public LLDNOutput()
        : base()
    {
    }

    public LLDNOutput(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        conInput = new ParamConnection("Input", null);
        conInput.description = "The final output output audio signal.";
        this.genParams.Add(conInput);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection)
    { 
        if(this.conInput == null || this.conInput.IsConnected() == false)
            return ZeroGen();

        return this.conInput.Reference.SpawnGenerator(
            freq, 
            beatsPerSec, 
            samplesPerSec, 
            amp,
            spawnFrom,
            collection);
    }

    public override NodeType nodeType => NodeType.Output;

    public bool SetConnection_Input(LLDNBase input)
    { 
        return this.SetConnection(this.conInput, input);
    }

    public override LLDNBase CloneType()
    { 
        return new LLDNOutput();
    }

    public bool TreeAppearsValid(WiringCollection wc, WiringDocument owner)
    { 
        if(this.conInput.IsConnected() == false)
            return false;

        Queue<LLDNBase> toScan = new Queue<LLDNBase>();
        toScan.Enqueue(this.conInput.Reference);

        while(toScan.Count > 0)
        {
            LLDNBase f = toScan.Dequeue();
            if(f.CanMakeNoise() == true)
                return true;

            if(f.nodeType == NodeType.Reference && wc != null)
            { 
                LLDNReference refr = f as LLDNReference;
                if(refr != null && string.IsNullOrEmpty(refr.reference.referenceGUID) == false && owner != null)
                { 
                   WiringDocument refWD = wc.GetDocument(refr.reference.referenceGUID);
                    if(refWD != null)
                    { 
                        if(refWD == owner)
                            return false;

                        if(refWD.Output != null)
                            toScan.Enqueue(refWD.Output);
                    }
                }
            }

            foreach(ParamConnection pc in f.GetParamConnections())
            {
                if(pc.IsConnected() == false)
                    continue;

                if(f.VerifyConnectionUsed(pc) == false)
                    continue;

                toScan.Enqueue(pc.Reference);
            }
        }

        return false;
    }

    public override Category GetCategory() => Category.Special;

    public override string description => "An audio signal node that represents the final value.";

    public override bool HasOutput() => false;
}
