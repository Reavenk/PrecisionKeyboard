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

public class LLDNMAD : LLDNBase
{
    ParamConnection input;
    ParamFloat mul;
    ParamFloat add;

    public LLDNMAD()
        : base()
    { }

    public LLDNMAD(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.genParams.Add(this.input);

        this.mul = new ParamFloat("Mul", "Multiple", 0.5f, -2.0f, 2.0f);
        this.mul.description = "The constant value to scale the audio signal by.";
        this.genParams.Add(this.mul);

        this.add = new ParamFloat("Add", "Addition", 0.5f, -2.0f, 2.0f);
        this.add.description = "The constant value to add to the audio signal.";
        this.genParams.Add(this.add);
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

        return 
            new PxPre.Phonics.GenMAD( 
                gb, 
                this.mul.value, 
                this.add.value);
    }

    public override NodeType nodeType => NodeType.MAD;

    public override LLDNBase CloneType()
    {
        return new LLDNMAD();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "A 'multipy and then add' operation.\n\n" +
        "Given an audio signal, create a new audio signal by multiplying it by a constant value - and then add another constant value to it.\n\n" +
        "This can be used to linearly change the range of an audio signal; for example, to convert an audio signal from a wave generator to a Lerp parameter.";
}
