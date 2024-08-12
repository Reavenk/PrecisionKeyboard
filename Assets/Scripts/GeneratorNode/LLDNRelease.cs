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

public class LLDNRelease : LLDNBase
{
    ParamConnection input = null;

    /// <summary>
    /// The time length of the release.
    /// </summary>
    ParamTimeLen release = null;

    public LLDNRelease()
        : base()
    { }

    public LLDNRelease(string guid)
        : base(guid)
    { }

    protected override void _Init()
    { 
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal to apply the release envelope to.";
        this.genParams.Add(this.input);

        this.release = new ParamTimeLen("Release", 1.0f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.release.description = "The amount of time to release the note.";
        this.genParams.Add(this.release);
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
            this.release.GetWavelength(freq, beatsPerSec);


        if (this.input.IsConnected() == false)
        { 
            return 
                new PxPre.Phonics.GenReleaseEmpty(
                    samplesPerSec,
                    len);
        }

        PxPre.Phonics.GenBase gb = 
            this.input.Reference.SpawnGenerator(
                freq, 
                beatsPerSec, 
                samplesPerSec, 
                amp,
                spawnFrom,
                collection);

        return 
            new PxPre.Phonics.GenRelease(
                samplesPerSec,
                gb, 
                len);
    }

    public override NodeType nodeType => NodeType.Release;

    public override LLDNBase CloneType()
    {
        return new LLDNRelease();
    }

    public override Category GetCategory() => Category.Envelopes;

    public override string description => 
        "Apply a release envelope to the audio signal.\n\n"+
        "When the user releases the keyboard key, the note will continue to fade out for a while.";
}
