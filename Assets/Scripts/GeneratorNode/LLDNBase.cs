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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LLDNBase
{
    // SHHH! These are secret! 
    // They're undocumented common agreements between the
    // the LLDAW and its UI that these can override their
    // UI settings (if supported).
    public const string secretTitleProperty = "$_Title";
    public const string secretHeightProperty = "$_Height";
    public const string secretWidthProperty = "$_Width";
    /// <summary>
    /// Enums for each unique dervied GNBase class.
    /// </summary>
    public enum NodeType
    {
        Abs,
        Amplify,
        Chorus,
        Clamp,
        CombAdd,
        CombSub,
        CombMod,
        Comment,
        Constant,
        Cube,
        Cycle,
        Delay,
        EnvADSR,
        EnvAttack,
        EnvDecay,
        Gate,
        GateList,
        GateWave,
        Highlight,
        Hold,
        Invert,
        Lerp,
        MAD,
        Max,
        Min,
        Negate,
        NoiseWave,
        Output,
        PowerAmplify,
        Quant,
        QuickOut,
        Reference,
        Release,
        SawWave,
        Sign,
        SineWave,
        Smear,
        Square,
        SquareWave,
        Stutter,
        TriWave,
        Window,
        Void // Unset/Error type
    }

    /// <summary>
    /// The various catagories to organize the nodes into.
    /// </summary>
    public enum Category
    { 
        Wave,
        Combines,
        Envelopes,
        Voices,
        Operations,
        Special
    }

    /// <summary>
    /// The id to uniquly identify the node.
    /// </summary>
    private string guid = string.Empty;

    /// <summary>
    /// public accessor.
    /// </summary>
    public string GUID {get{return this.guid; } }

    /// <summary>
    /// The list of introspective parameters.
    /// </summary>
    protected List<ParamBase> genParams = new List<ParamBase>();

    /// <summary>
    /// Public enumerator for parameters.
    /// </summary>
    public IEnumerable<ParamBase> nodeParams 
    {
        get
        {
            return this.genParams; 
        } 
    }

    /// <summary>
    /// The position location on the graph.
    /// </summary>
    public Vector2 cachedUILocation;

    public abstract string description {get;}

    /// <summary>
    /// Create a generator based on the the node type.
    /// </summary>
    /// <param name="nodeType">The node type to create.</param>
    /// <param name="guid">The node's GUID. </param>
    /// <returns>The created node.</returns>
    public static LLDNBase CreateGenerator(LLDNBase.NodeType nodeType, string guid)
    {
        switch (nodeType)
        {
            case NodeType.Abs:
                return new LLDNAbs(guid);

            case NodeType.Amplify:
                return new LLDNAmplify(guid);

            case NodeType.Chorus:
                return new LLDNCloneChorus(guid);

            case NodeType.Clamp:
                return new LLDNClamp(guid);

            case NodeType.CombAdd:
                return new LLDNCombAdd(guid);

            case NodeType.CombSub:
                return new LLDNCombSub(guid);

            case NodeType.CombMod:
                return new LLDNCombMul(guid);

            case NodeType.Comment:
                return new LLDNComment(guid);

            case NodeType.Constant:
                return new LLDNConstant(guid);

            case NodeType.Cube:
                return new LLDNCube(guid);

            case NodeType.Cycle:
                return new LLDNCycle(guid);

            case NodeType.Delay:
                return new LLDNDelay(guid);

            case NodeType.EnvADSR:
                return new LLDNEnvADSR(guid);

            case NodeType.EnvAttack:
                return new LLDNEnvAttack(guid);

            case NodeType.EnvDecay:
                return new LLDNEnvDecay(guid);

            case NodeType.Gate:
                return new LLDNGate(guid);

            case NodeType.GateList:
                return new LLDNGateList(guid);

            case NodeType.GateWave:
                return new LLDNGateWave(guid);

            case NodeType.Highlight:
                return new LLDNHighlight(guid);

            case NodeType.Hold:
                return new LLDNHold(guid);

            case NodeType.Invert:
                return new LLDNInvert(guid);

            case NodeType.Lerp:
                return new LLDNLerp(guid);

            case NodeType.MAD:
                return new LLDNMAD(guid);

            case NodeType.Max:
                return new LLDNMax(guid);

            case NodeType.Min:
                return new LLDNMin(guid);

            case NodeType.Negate:
                return new LLDNNegate(guid);

            case NodeType.NoiseWave:
                return new LLDNNoise(guid);

            case NodeType.Output:
                return new LLDNOutput(guid);

            case NodeType.PowerAmplify:
                return new LLDNPowerAmplify(guid);

            case NodeType.Quant:
                return new LLDNQuant(guid);

            case NodeType.QuickOut:
                return new LLDNQuickOut(guid);

            case NodeType.Reference:
                return new LLDNReference(guid);

            case NodeType.Release:
                return new LLDNRelease(guid);

            case NodeType.SawWave:
                return new LLDNSawtoothWave(guid);

            case NodeType.Smear:
                return new LLDNSmear(guid);

            case NodeType.Sign:
                return new LLDNSign(guid);

            case NodeType.SineWave:
                return new LLDNSineWave(guid);

            case NodeType.Square:
                return new LLDNSquare(guid);

            case NodeType.SquareWave:
                return new LLDNSquareWave(guid);

            case NodeType.Stutter:
                return new LLDNStutter(guid);

            case NodeType.TriWave:
                return new LLDNTriangleWave(guid);

            case NodeType.Void:
                return null;

        }

        Debug.LogError($"Could not create node for node {nodeType.ToString()} at GUID {guid}" );
        return null;
    }

    /// <summary>
    /// Default constructor, creates the node with a randomly
    /// generated GUID.
    /// </summary>
    public LLDNBase()
    { 
        this.guid = System.Guid.NewGuid().ToString();

        this._Init();
    }

    /// <summary>
    /// Constructor that creates the node with a specified GUID.
    /// </summary>
    /// <param name="guid"></param>
    public LLDNBase(string guid)
    { 
        if(string.IsNullOrEmpty(guid) == true)
            this.guid = System.Guid.NewGuid().ToString();
        else
            this.guid = guid;

        this._Init();
    }

    /// <summary>
    /// Generates a new random GUID.
    /// </summary>
    public void RegenerateName()
    {
        this.guid = System.Guid.NewGuid().ToString();
    }

    public void SetGUID(string newGuid)
    { 
        this.guid = newGuid;
    }

    /// <summary>
    /// Create a GenBase based off the node's parameters and connections.
    /// </summary>
    /// <param name="freq">The frequency of the note being generated.</param>
    /// <param name="beatsPerSec">The BPM of the node.</param>
    /// <param name="samplesPerSec">The samples per second of the PCM being generated.</param>
    /// <param name="amp">The amplitude generators should generate at.</param>
    /// <returns>The GenBase generator.</returns>
    public abstract PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection);

    public abstract NodeType nodeType {get;}

    public bool SetConnection(ParamConnection ownedConParam, LLDNBase newConnection)
    {
        if(ownedConParam.IsConnected() == true)
        {
            if(ownedConParam.Reference == newConnection)
                return true;

            if (newConnection.HierarchyContains(this) == true)
                return false;
        }

        ownedConParam.SetReference(newConnection);
        return true;
    }

    /// <summary>
    /// Check if the node has a parameter that references a GNBase
    /// </summary>
    /// <param name="gb">The GNBase to compare against.</param>
    /// <returns>True if the node contains a parameter referencing the node, else false.</returns>
    public bool HierarchyContains(LLDNBase gb)
    {
        foreach (ParamBase pb in this.genParams)
        {
            if (pb.type != ParamBase.Type.PCMInput)
                continue;

            ParamConnection pc = pb as ParamConnection;
            if(pc.IsConnected() == false)
                continue;

            if (pc.Reference == gb)
                return true;

            if( pc.Reference.HierarchyContains(gb) == true)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Clear all PCM connection parameters in the node.
    /// </summary>
    public void ClearAllConnections()
    {
        foreach (ParamBase pb in this.genParams)
        {
            if (pb.type != ParamBase.Type.PCMInput)
                continue;

            ParamConnection pc = pb as ParamConnection;
            if (pc.IsConnected() == false)
                continue;

            pc.SetReference(null);
            
        }
    }

    public bool ClearKnowledgeOfConnection(LLDNBase gnb)
    {
        bool any = false;
        foreach (ParamBase pb in this.genParams)
        {
            if (pb.type != ParamBase.Type.PCMInput)
                continue;

            ParamConnection pc = pb as ParamConnection;
            if (pc.IsConnected() == false)
                continue;

            if(pc.Reference == gnb)
            {
                pc.SetReference(null);
                any = true;
            }
        }
        return any;
    }

    public ParamConnection GetConnectionParam(string name)
    {
        foreach (ParamBase pb in this.genParams)
        {
            if (pb.type != ParamBase.Type.PCMInput)
                continue;

            if(string.Equals(pb.name, name, System.StringComparison.OrdinalIgnoreCase) == false)
                continue;

            ParamConnection pc = pb as ParamConnection;
            return pc;
        }

        return null;
    }

    public IEnumerable<ParamConnection> GetParamConnections()
    {
        foreach (ParamBase pb in this.genParams)
        {
            if (pb.type != ParamBase.Type.PCMInput)
                continue;

            ParamConnection pc = pb as ParamConnection;
            yield return pc;
        }
    }

    /// <summary>
    /// Called as part of the loading process as another pass after all GNBases are created and Load()
    /// had been called. It usually involves stitching up references.
    /// </summary>
    /// <param name="ele"></param>
    /// <param name="scopedDocs"></param>
    /// <param name="directory"></param>
    /// <returns></returns>
    public virtual bool PostLoadXML(
        Dictionary<string, LLDNBase> directory,               // All GNBases in the SAME wiring document
        Dictionary<string, WiringDocument> scopedDocs)      // All the other wiring documents that were also in the same XML file
    {
        foreach (ParamBase pb in this.nodeParams)
            pb.PostLoad(directory, scopedDocs);

        return true;
    }

    public virtual bool LoadXML(System.Xml.XmlElement ele)
    { 

        foreach(System.Xml.XmlElement ch in ele)
        {
            ParamBase.Type ty = ParamBase.FromSerializationType(ch.Name);
            if(ty == ParamBase.Type.Invalid)
                continue;

            System.Xml.XmlAttribute attrName = ch.Attributes["name"];
            if(attrName == null)
                continue;

            string paramName = attrName.Value;
            ParamBase pb = this.GetParam( paramName, ty);
            if(pb == null)
                continue;

            pb.SetValueFromString(ch.InnerText);

            System.Xml.XmlAttribute attrViz = ch.Attributes["visible"];
            if(attrViz != null)
                pb.visible = ParamBool.ConvertFromString(attrViz.Value);
        }
        return true;
    }

    public virtual bool LoadXML(System.Xml.XmlElement ele, Dictionary<string, LLDNBase> directory)
    {
        foreach (System.Xml.XmlElement ch in ele)
        {
            ParamBase.Type ty = ParamBase.FromSerializationType(ch.Name);
            if (ty == ParamBase.Type.Invalid)
                continue;

            System.Xml.XmlAttribute attrName = ch.Attributes["name"];
            if (attrName == null)
                continue;

            string paramName = attrName.Value;
            ParamBase pb = this.GetParam(paramName, ty);
            if (pb == null)
                continue;

            pb.SetValueFromString(ch.InnerText, directory);
        }

        return true;

    }


    public virtual bool SaveXML(System.Xml.XmlElement ele)
    { 
        foreach(ParamBase pb in this.genParams)
        { 
            string paramType = ParamBase.ToSerializationType(pb.type);
            System.Xml.XmlElement paramEle = ele.OwnerDocument.CreateElement(paramType);
            paramEle.SetAttribute("name", pb.name);
            paramEle.InnerText = pb.GetStringValue();
            paramEle.SetAttribute("visible", ParamBool.ConvertToString(pb.visible));

            ele.AppendChild(paramEle);
        }

        return true;
    }

    public static System.Xml.XmlElement CreateSaveElement(System.Xml.XmlElement parent, LLDNBase gnb)
    { 
        System.Xml.XmlElement ret = parent.OwnerDocument.CreateElement("node");
        ret.SetAttribute("type", gnb.nodeType.ToString());
        ret.SetAttribute("id", gnb.GUID);
        ret.SetAttribute("x", gnb.cachedUILocation.x.ToString());
        ret.SetAttribute("y", gnb.cachedUILocation.y.ToString());
        parent.AppendChild(ret);

        gnb.SaveXML(ret);

        return ret;

    }

    /// <summary>
    /// Get a parameter of a specified type and name.
    /// </summary>
    /// <param name="name">The name of the parameter to retrive.</param>
    /// <returns>The found parameter, or null if none was found.</returns>
    public ParamBase GetParam(string name)
    { 
        foreach(ParamBase pb in this.genParams)
        { 
            if(string.Equals(pb.name,name, System.StringComparison.OrdinalIgnoreCase) == true)
                return pb;
        }
        return null;
    }

    /// <summary>
    /// Get a parameter of a specified type and name.
    /// </summary>
    /// <param name="name">The name of the parameter to retrive.</param>
    /// <param name="ty">The required type of the parameter to retrive.</param>
    /// <returns>The found parameter, or null if none was found.</returns>
    public ParamBase GetParam(string name, ParamBase.Type ty)
    { 
        foreach(ParamBase pb in this.genParams)
        {
            if(string.Equals(pb.name, name, System.StringComparison.OrdinalIgnoreCase) == false)
                continue;

            if(pb.type != ty)
                continue;

            return pb;
        }

        return null;
    }

    /// <summary>
    /// Get the node's bool parameter matching a specified name.
    /// </summary>
    /// <param name="name">The name of the parameter to retrive.</param>
    /// <returns>The found parameter, or null if none was found.</returns>
    public ParamBool GetBoolParam(string name)
    { 
        ParamBase pb = GetParam(name, ParamBase.Type.Bool);
        if(pb == null)
            return null;

        return (ParamBool)pb;
    }

    /// <summary>
    /// Get the node's int parameter matching a specified name.
    /// </summary>
    /// <param name="name">The name of the parameter to retrive.</param>
    /// <returns>The found parameter, or null if none was found.</returns>
    public ParamInt GetIntParam(string name)
    {
        ParamBase pb = GetParam(name, ParamBase.Type.Int);
        if(pb == null)
            return null;

        return (ParamInt)pb;
    }

    /// <summary>
    /// Get the node's float parameter matching a specified name.
    /// </summary>
    /// <param name="name">The name of the paramter to retrieve.</param>
    /// <returns>The found parameter, or null if none was found.</returns>
    public ParamFloat GetFloatParam(string name)
    { 
        ParamBase pb = GetParam(name, ParamBase.Type.Float);
        if(pb == null)
            return null;

        return (ParamFloat)pb;
    }

    /// <summary>
    /// Get the node's enum parameter matching a specified name.
    /// </summary>
    /// <param name="name">The name of the parameter to retrive</param>
    /// <returns>The found parameter, or null if none was found.</returns>
    public ParamEnum GetParamEnum(string name)
    {
        ParamBase pb = GetParam(name, ParamBase.Type.Enum);
        if(pb == null)
            return null;

        return (ParamEnum)pb;
    }

    public ParamText GetParamText(string name)
    { 
        ParamBase pb = GetParam(name, ParamBase.Type.Text);
        if(pb == null)
            return null;

        return (ParamText)pb;
    }

    public ParamConnection GetParamConnection(string name)
    { 
        ParamBase pb = GetParam(name, ParamBase.Type.PCMInput);
        if(pb == null)
            return null;

        return (ParamConnection)pb;
    }

    public abstract LLDNBase CloneType();

    public virtual LLDNBase Clone(Dictionary<string, LLDNBase> directory = null)
    { 
        LLDNBase clone = this.CloneType();

        for(int i = 0; i < clone.genParams.Count; ++i)
        { 
            ParamBase pbClone = clone.genParams[i];
            ParamBase pbThis = null;

            if(
                string.Equals(this.genParams[i].name, pbClone.name, System.StringComparison.OrdinalIgnoreCase) && 
                this.genParams[i].type == pbClone.type)
            {
                pbThis = this.genParams[i];
            }
            else
            { 
                pbThis = 
                    this.GetParam(
                        clone.genParams[i].name, 
                        clone.genParams[i].type);
            }

            if(pbThis == null)
                continue;

            string str = this.genParams[i].GetStringValue();

            
            clone.genParams[i].SetValueFromString(str);
            
            if (directory != null)
                clone.genParams[i].SetValueFromString(str, directory);
        }

        return clone;
    }

    /// <summary>
    /// Get the node's assigned category.
    /// </summary>
    /// <returns>The node's category.</returns>
    public abstract Category GetCategory();

    /// <summary>
    /// The node's category's color.
    /// </summary>
    /// <returns>The node's categoryu's color.</returns>
    public Color GetColor()
    { 
        return GetCategoryColor(this.GetCategory());
    }

    public Color GetEdgeColor()
    { 
        return GetCategoryEdgeColor(this.GetCategory());
    }

    /// <summary>
    /// Based off a category value, return the color assigned to 
    /// represent the category.
    /// </summary>
    /// <param name="c">The category to retrieve the color for.</param>
    /// <returns>The catagory's color.</returns>
    public static Color GetCategoryColor(Category c)
    { 
        switch(c)
        { 
            case Category.Wave:
                return new Color(0.5f, 1.0f, 0.5f);

            case Category.Voices:
                return new Color(1.0f, 0.5f, 0.5f);

            case Category.Operations:
                return new Color(1.0f, 0.5f, 1.0f);

            case Category.Envelopes:
                return new Color(0.5f, 0.5f, 1.0f);

            case Category.Combines:
                return new Color(1.0f, 0.75f, 0.5f);

            case Category.Special:
                return new Color(0.9f, 0.9f, 0.9f);
        }

        return Color.white;
    }

    public static Color GetCategoryEdgeColor(Category c)
    { 
        if(c == Category.Special)
            return new Color(0.6f, 0.6f, 0.6f);

        return GetCategoryColor(c);
    }

    public bool ReferencesDocument(WiringDocument wd)
    { 
        foreach(ParamBase pb in this.nodeParams)
        { 
            if(pb.ReferencesDocument(wd) == true)
                return true;
        }
        return false;
    }

    protected abstract void _Init();

    public virtual bool CanMakeNoise() => false;

    public virtual bool HasOutput() => true;

    public virtual bool VerifyConnectionUsed(ParamConnection connection) => true;

    public static PxPre.Phonics.GenBase ZeroGen()
    { 
        return new PxPre.Phonics.GenZero();
    }
}
