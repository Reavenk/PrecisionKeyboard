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

public class LLDNHighlight : LLDNBase
{
    ParamText buttonText = null;
    public ParamText actionString = null;
    public ParamText icon = null;

    ParamText secrTitle = null;
    public ParamFloat secrWidth = null;
    
    public ParamFloat fontScale = null;
    public ParamBool encase = null;

    public LLDNHighlight()
        : base()
    { }

    public LLDNHighlight(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.secrTitle = new ParamText(LLDNBase.secretTitleProperty, "Highlight");
        this.secrTitle.description = "The text on the button";
        this.secrTitle.editable = false;
        this.genParams.Add(this.secrTitle);

        this.buttonText = new ParamText("Button", "Press Me");
        this.buttonText.description = "The button label text.";
        this.buttonText.editable = false;
        this.genParams.Add(this.buttonText);

        this.actionString = new ParamText("Action", "Nothing");
        this.actionString.description = "The action to perform. This is hardcoded to Precision Keyboard.";
        this.actionString.editable = false;
        this.genParams.Add(this.actionString);

        this.secrWidth = new ParamFloat(LLDNBase.secretWidthProperty, "pts", 0.0f, 0.0f, 1000.0f);
        this.secrWidth.description = "The width override of the node.";
        this.secrWidth.editable = false;
        this.genParams.Add(this.secrWidth);

        this.fontScale = new ParamFloat("FontScale", "multiplier", 1.0f, 0.0f, 10.0f);
        this.fontScale.editable = false;
        this.genParams.Add(this.fontScale);

        this.encase = new ParamBool("Encase", "encasement", false);
        this.encase.editable = false;
        this.genParams.Add(this.encase);

        this.icon = new ParamText("Icon", "");
        this.icon.editable = false;
        this.genParams.Add(this.icon);
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

    public override NodeType nodeType => NodeType.Highlight;

    public override LLDNBase CloneType()
    {
        return new LLDNHighlight();
    }

    public override Category GetCategory() => Category.Special;

    public override string description => 
        "Highlight something in UI. This is very specific to Precision Keyboard.";

    public override bool CanMakeNoise() => false;

    public override bool HasOutput() => false;
}
