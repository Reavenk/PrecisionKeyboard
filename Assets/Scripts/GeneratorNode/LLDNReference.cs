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

public class LLDNReference : LLDNBase
{

    public ParamWireReference reference;

    public LLDNReference()
        : base()
    { }

    public LLDNReference(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.reference = new ParamWireReference("Wiring", "");
        this.reference.description = "Another wiring document to reference the output of. Note that referencing something that will end up referencing the original wiring (i.e. cyclic referencing) is not allowed.";
        this.genParams.Add(this.reference);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp, 
        WiringDocument spawnFrom,
        WiringCollection collection)
    {
        if(string.IsNullOrEmpty(reference.referenceGUID) == true)
            return ZeroGen();

        WiringDocument wd = collection.GetDocument(reference.referenceGUID);

        // The last one is a check against cyclic redundancy. There are 
        // other things that prevent this, but if for some reason those 
        // fail, we check one last time and just shut down the process 
        // with a zero signal.
        if(wd == null || wd.Output == null || wd == spawnFrom) 
            return ZeroGen();

        return 
            wd.Output.SpawnGenerator(
                freq, 
                beatsPerSec, 
                samplesPerSec, 
                amp, 
                spawnFrom, 
                collection);
    }

    public override LLDNBase CloneType()
    {
        return new LLDNReference();
    }

    public override NodeType nodeType => NodeType.Reference;

    public override Category GetCategory() => Category.Special;

    public override string description => "Reference the output of another wiring document.";

    public override bool HasOutput() => true;

}
