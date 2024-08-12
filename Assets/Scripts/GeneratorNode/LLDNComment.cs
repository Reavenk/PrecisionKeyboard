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

public class LLDNComment : LLDNBase
{
    // Default node values, this does kind of break 
    // design rigor because it assumes this backend
    // library is going to have an editor of a certain
    // design and scale - but that can be addressed later.
    public const float defaultHeight = 50.0f;
    public const float defaultWidth = 300.0f;

    ParamText text = null;

    public ParamText secrTitle = null;
    public ParamFloat secrHeight = null;
    public ParamFloat secrWidth = null;

    public ParamFloat fontScale = null;
    public ParamBool encase = null;

    public LLDNComment()
        : base()
    { }

    public LLDNComment(string guid)
        : base(guid)
    { }

    protected override void _Init()
    { 
        this.secrTitle = new ParamText(secretTitleProperty, "Comment");
        this.secrTitle.editable = false;
        this.genParams.Add(this.secrTitle);

        this.text = new ParamText("text", "");
        this.text.description = "A comment left by the author giving some note about the wiring - possibly specific to the region the comment was placed at.";
        this.genParams.Add(this.text);

        this.secrHeight = new ParamFloat(secretHeightProperty, "pt", defaultHeight,10.0f, 1000.0f);
        this.secrHeight.editable = false;
        this.genParams.Add(this.secrHeight);

        this.secrWidth = new ParamFloat(secretWidthProperty, "pt", 0.0f, 0.0f, 1000.0f);
        this.secrWidth.editable = false;
        this.genParams.Add(this.secrWidth);

        this.fontScale = new ParamFloat("FontScale", "multiplier", 1.0f, 0.0f, 10.0f);
        this.fontScale.editable = false;
        this.genParams.Add(this.fontScale);

        this.encase = new ParamBool("Encase", "encasement", false);
        this.encase.editable = false;
        this.genParams.Add(this.encase);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp, 
        WiringDocument spawnFrom,
        WiringCollection collection)
    {
        return null;
    }

    public override LLDNBase CloneType()
    {
        return new LLDNComment();
    }

    public override NodeType nodeType => NodeType.Comment;

    public override Category GetCategory() => Category.Special;

    public override string description => "Texts annotations.";

    public override bool HasOutput() => false;

    public string GetComment()
    { 
        return this.text.GetStringValue();
    }

    public void SetComment(string comment)
    { 
        this.text.SetValueFromString(comment);
    }
}
