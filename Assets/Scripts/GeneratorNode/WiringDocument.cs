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
using UnityEngine;

public class WiringDocument
{
    public enum Category
    { 
        Unlabled,
        Misc,
        Keys,
        Brass,
        Bell,
        Book,
        Strings,
        Voice,
        Choir,
        Robot,
        Alien,
        Electro
    }

    public static string GetCategoryName(Category cat)
    { 
        switch(cat)
        { 
            case Category.Misc:
                return "misc";

            case Category.Keys:
                return "keys";

            case Category.Brass:
                return "brass";

            case Category.Bell:
                return "bell";

            case Category.Book:
                return "book";

            case Category.Strings:
                return "strings";

            case Category.Voice:
                return "voice";

            case Category.Choir:
                return "choir";

            case Category.Robot:
                return "robot";

            case Category.Alien:
                return "alien";

            case Category.Electro:
                return "electro";

            default:
            case Category.Unlabled:
                return "unlabled";
        }
    }

    public static Category GetCategoryFromName(string name)
    { 
        switch(name)
        { 
            case "misc":
                return Category.Misc;

            case "keys":
                return Category.Keys;

            case "brass":
                return Category.Brass;

            case "bell":
                return Category.Bell;

            case "book":
                return Category.Book;

            case "strings":
                return Category.Strings;

            case "voice":
                return Category.Voice;

            case "choir":
                return Category.Choir;

            case "robot":
                return Category.Robot;

            case "alien":
                return Category.Alien;

            case "electro":
                return Category.Electro;

            default:
            case "unlabled":
                return Category.Unlabled;
        }
    }

    public const string xmlName = "wiring";
    public const int MaxNameLen = 50;

    public readonly string guid;

    protected string name;
    public Vector2 cachedDim;

    LLDNOutput output;
    //
    public LLDNOutput Output 
    {get{return this.output; } }

    LLDNGateList gateList;
    //
    public LLDNGateList GateList
    { get{return this.gateList; } }

    List<LLDNBase> generators = 
        new List<LLDNBase>();

    public IEnumerable<LLDNBase> Generators 
    { get{return this.generators; } }

    public Category category;

    public void SetName(string str)
    { 
        if(str.Length > MaxNameLen)
            str = str.Substring(0, MaxNameLen);

        this.name = str;
    }

    public string GetName()
    { 
        return this.name;
    }

    public void AddGenerator(LLDNBase generator)
    { 
        switch(generator.nodeType)
        { 
            case LLDNBase.NodeType.GateList:
                { 
                    LLDNGateList convGateList = generator as LLDNGateList;
                    if(gateList != null)
                    { 
                        // Nope! There can only be one!
                        if(this.gateList != null)
                            return;

                        this.gateList = convGateList;
                    }
                }
                break;
        }

        this.generators.Add(generator);
    }

    public WiringDocument(bool createOutput, string guid)
    { 
        if(string.IsNullOrEmpty(guid) == true)
            this.guid = System.Guid.NewGuid().ToString();
        else
            this.guid = guid;

        if(createOutput == true)
        {
            this.output = new LLDNOutput();
            this.output.cachedUILocation = new Vector2(300.0f, -50.0f);
            this.generators.Add(this.output);

            LLDNSineWave sine = new LLDNSineWave();
            sine.cachedUILocation = new Vector2(30.0f, -30.0f);
            generators.Add(sine);

            this.output.SetConnection_Input(sine);
        }
    }

    public bool RemoveNode(LLDNBase node, bool removeConnections = true)
    { 
        if(this.generators.Remove(node) == false)
            return false;

        if(this.gateList == node)
            this.gateList = null;

        if(removeConnections == true)
        {
            node.ClearAllConnections();

            foreach(LLDNBase g in this.generators)
                g.ClearKnowledgeOfConnection(node);
        }

        return true;
    }

    public PxPre.Phonics.GenBase CreateGenerator(
        float freq, 
        float beatsPerSec, 
        float mastervol, 
        int samplesPerSec,
        WiringDocument spawnFrom,
        WiringCollection collection)
    { 
        if(this.output == null)
            return null;

        return 
            this.output.SpawnGenerator(
                freq, 
                beatsPerSec, 
                samplesPerSec, 
                mastervol,
                spawnFrom,
                collection);
    }

    public WiringDocument Clone(string newName)
    {

        // Fill up the directory. Used so we can use existing
        // cloning utilities for cloning connection params.
        Dictionary<string, LLDNBase> thisLookup = 
            new Dictionary<string, LLDNBase>();
        
        foreach(LLDNBase gnb in this.generators)
            thisLookup.Add(gnb.GUID, gnb);

        // Copy all the generators.
        Dictionary< LLDNBase, LLDNBase> thisToThatMap = 
            new Dictionary<LLDNBase, LLDNBase>();

        WiringDocument wd = new WiringDocument(false, null);

        foreach(LLDNBase gnb in this.generators)
        { 
            LLDNBase clone = gnb.Clone(thisLookup);
            clone.cachedUILocation = gnb.cachedUILocation;
            wd.generators.Add(clone);
            thisToThatMap.Add(gnb, clone);
        }

        // Convert the connections to point to their own equivalents.
        foreach(LLDNBase gnb in wd.generators)
        { 
            foreach(ParamBase pb in gnb.nodeParams)
            { 
                if(pb.type == ParamBase.Type.PCMInput)
                { 
                    ParamConnection pcon = pb as ParamConnection;
                    if(pcon.IsConnected() == false)
                        continue;

                    LLDNBase conv;
                    if(thisToThatMap.TryGetValue(pcon.Reference, out conv) == true)
                        pcon.SetReference(conv);
                    else
                        pcon.ClearReference();
                }
            }
        }

        // Fix up the output
        if(this.output != null)
        {
            LLDNBase gbOutput = thisToThatMap[this.output];
            if(gbOutput != null && gbOutput.nodeType == LLDNBase.NodeType.Output)
                wd.output = (LLDNOutput)gbOutput;
        }

        // Finish
        if(string.IsNullOrEmpty(newName) == false)
            wd.SetName(newName);
        else
            wd.SetName(this.name);

        return wd;
    }

    public void Clear()
    { 
        this.output = null;
        this.generators.Clear();
    }

    public string GetCategoryName()
    { 
        return GetCategoryName(this.category);
    }

    public bool LoadXML(System.Xml.XmlElement ele)
    { 
        if(ele.Name != "wiring")
            return false;

        System.Xml.XmlAttribute attrName = ele.Attributes["name"];
        if(attrName != null)
        {
            this.name = attrName.Value;

            if(this.name.Length > MaxNameLen)
                this.name = this.name.Substring(0, MaxNameLen);
        }

        System.Xml.XmlAttribute attrCat = ele.Attributes["cat"];
        if(attrCat != null)
            this.category = GetCategoryFromName(attrCat.Value);

        Dictionary<string, LLDNBase> directory = 
            new Dictionary<string, LLDNBase>();

        Dictionary<LLDNBase, System.Xml.XmlElement> xmlToGNMap = 
            new Dictionary<LLDNBase, System.Xml.XmlElement>();

        foreach(System.Xml.XmlElement eleNode in ele)
        {
            if(eleNode.Name != "node")
                continue;

            System.Xml.XmlAttribute attrType = eleNode.Attributes["type"];
            if(attrType == null)
                continue;

            LLDNBase.NodeType nt;
            if(System.Enum.TryParse<LLDNBase.NodeType>(attrType.Value, true, out nt) == false)
                continue;


            string guid = null;
            System.Xml.XmlAttribute attrID = eleNode.Attributes["id"];
            if(attrID != null)
                guid = attrID.Value;

            LLDNBase newGN = LLDNBase.CreateGenerator(nt, guid);
            if(newGN.LoadXML(eleNode) == false)
                continue;

            System.Xml.XmlAttribute attrX = eleNode.Attributes["x"];
            if(attrX != null)
                float.TryParse(attrX.Value, out newGN.cachedUILocation.x);

            System.Xml.XmlAttribute attrY = eleNode.Attributes["y"];
            if(attrY != null)
                float.TryParse(attrY.Value, out newGN.cachedUILocation.y);

            this.generators.Add(newGN);
            xmlToGNMap.Add(newGN, eleNode);
            directory.Add(newGN.GUID, newGN);
        }

        foreach(LLDNBase gnb in this.generators)
            gnb.LoadXML(xmlToGNMap[gnb], directory);

        string outputID = null;
        System.Xml.XmlAttribute attrOutput = ele.Attributes["output"];
        if(attrOutput != null)
            outputID = attrOutput.Value;

        if(string.IsNullOrEmpty(outputID) == false)
        { 
            LLDNBase gnbOut;
            if(directory.TryGetValue(outputID, out gnbOut) == true)
                this.output = gnbOut as LLDNOutput;
        }
        
        return true;
    }

    public System.Xml.XmlElement SaveXML(System.Xml.XmlDocument xmlDoc)
    { 
        System.Xml.XmlElement wdDoc = xmlDoc.CreateElement(xmlName);
        wdDoc.SetAttribute("name", this.name);
        wdDoc.SetAttribute("id", this.guid);
        wdDoc.SetAttribute("cat", this.GetCategoryName());

        if (this.output != null)
            wdDoc.SetAttribute("output", this.output.GUID);

        foreach(LLDNBase gnb in this.generators)
            LLDNBase.CreateSaveElement(wdDoc, gnb);

        return wdDoc;
    }

    public Dictionary<string, LLDNBase> Directory()
    { 
        Dictionary<string, LLDNBase> ret = new Dictionary<string, LLDNBase>();

        foreach(LLDNBase gnb in this.generators)
            ret.Add(gnb.GUID, gnb);

        return ret;
    }

    public bool ReferencesDocument(WiringDocument wd, bool includeSelf = true)
    { 
        if(includeSelf == true && wd == this)
            return true;

        foreach(LLDNBase gnb in this.generators)
        { 
            if(gnb.ReferencesDocument(wd) == true)
                return true;
        }
        return false;
    }

    public bool GuessMakesNoise(WiringCollection collection, HashSet<WiringDocument> encountered = null)
    { 
        if(this.output == null)
            return false;

        if(encountered == null)
            encountered = new HashSet<WiringDocument>();

        encountered.Add(this);

        Queue<LLDNBase> toScan = new Queue<LLDNBase>();
        toScan.Enqueue(this.output);

        List<LLDNReference> refs = new List<LLDNReference>();

        while(toScan.Count > 0)
        { 
            LLDNBase check = toScan.Dequeue();

            foreach(ParamConnection pc in check.GetParamConnections())
            { 
                if(pc.IsConnected() == false)
                    continue;

                LLDNBase gb = pc.Reference;
                LLDNReference gnr = gb as LLDNReference;
                if(gnr != null && collection != null)
                {
                    // Check these after everything else in the current doc.
                    refs.Add(gnr);
                }
                else
                {
                    if(gb.CanMakeNoise() == true)
                        return true;

                    toScan.Enqueue(gb);
                }
            }
        }

        foreach(LLDNReference gnr in refs)
        { 
            if(gnr.reference == null)
                continue;

            WiringDocument wd = 
                collection.GetDocument(gnr.reference.referenceGUID);

            if(encountered.Contains(wd) == true)
                continue;

            if(wd.GuessMakesNoise(collection, encountered) == true)
                return true;
        }
        return false;
    }

    public string GetProcessedWiringName(WiringCollection wc)
    { 
        string convName = 
            System.Text.RegularExpressions.Regex.Replace(
                this.name, 
                "{<[\\w]*>}", 
                (m)=>{return ProcessConvertingMatch(m, this, wc);});

        return convName;
    }

    public static string ProcessConvertingMatch(System.Text.RegularExpressions.Match m, WiringDocument wd, WiringCollection wc)
    { 
        if(m.Value == "{<idx>}")
        { 
            int idx = wc.GetWiringIndex(wd);
            if(idx == -1)
                return "--";

            return (idx + 1).ToString();
        }

        return m.Value;
    }
}
