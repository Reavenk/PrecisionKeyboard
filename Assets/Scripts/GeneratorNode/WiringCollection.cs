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

// We separate the core of having documents from WiringCollection into WiringCollectionBase
// so we can encapsulate is and make sure the coupling between documents and documentLookup 
// is protected.
public class WiringCollectionBase
{
    /// <summary>
    /// All the loaded wiring documents.
    /// </summary>
    private List<WiringDocument> documents = new List<WiringDocument>();
    private Dictionary<string, WiringDocument> documentLookup = new Dictionary<string, WiringDocument>();

    public IEnumerable<WiringDocument> Documents {get{return this.documents; } }

    // When setting the protections of functions:
    //  modifications are protected
    //  queries are public.

    public int Count()
    {
        return this.documents.Count;
    }

    public bool HasAny()
    {
        return this.documents.Count > 0;
    }

    protected void Clear()
    {
        this.documents.Clear();
        this.documentLookup.Clear();
    }

    protected bool AddDocument(WiringDocument wd)
    { 
        if(this.documentLookup.ContainsKey(wd.guid))
            return false;

        this.documents.Add(wd);
        this.documentLookup.Add(wd.guid, wd);

        return true;
    }

    public bool InsertDocument(WiringDocument wd, int idx)
    { 
        if(this.documentLookup.ContainsKey(wd.guid))
            return false;

        this.documents.Insert(idx, wd);
        this.documentLookup.Add(wd.guid, wd);

        return true;
    }

    protected bool RemoveDocument(WiringDocument doc)
    { 
        if(this.documents.Remove(doc) == false)
            return false;

        this.documentLookup.Remove(doc.guid);
        return true;
    }

    public bool ContainsDocument(WiringDocument doc)
    {
        foreach (WiringDocument docIt in this.documents)
        {
            if (docIt == doc)
                return true;
        }
        return false;
    }

    public bool HasWiringGUID(string guid)
    { 
        return this.documentLookup.ContainsKey(guid);
    }

    public WiringDocument GetDocument(string guid)
    { 
        WiringDocument ret;
        this.documentLookup.TryGetValue(guid, out ret);
        return ret;
    }

    public WiringDocument GetDocument(int idx)
    { 
        if(idx < 0 || idx >= this.documents.Count)
            return null;

        return this.documents[idx];
    }

    protected bool Append(WiringCollectionBase otherDoc)
    {
        bool any = false;
        foreach(WiringDocument doc in otherDoc.Documents)
        {
            if(this.AddDocument(doc) == true)
                any = true;
        }

        return any;
    }

    public int GetWiringIndex(WiringDocument wd)
    { 
        return this.documents.IndexOf(wd);
    }

    public List<WiringDocument> OrganizedByName()
    {
        List<WiringDocument> lst = 
            new List<WiringDocument>( this.documents);
        lst.Sort((x, y)=>{ return string.Compare(x.GetName(), y.GetName()); });
        return lst;
    }

    public List<WiringDocument> OrganizedByCategory()
    { 
        HashSet<WiringDocument.Category> hs = 
            new HashSet<WiringDocument.Category>();

        // Collect all the categories involved
        foreach(WiringDocument wd in this.documents)
            hs.Add(wd.category);

        // Gather them by name so we can sort that
        Dictionary<string, WiringDocument.Category> catFromName = 
            new Dictionary<string, WiringDocument.Category>();

        // And by grouping.
        Dictionary<WiringDocument.Category, List<WiringDocument>> docsByCategory = 
            new Dictionary<WiringDocument.Category, List<WiringDocument>>();

        // Fill in those containers.
        foreach(WiringDocument.Category cat in hs)
        {
            catFromName.Add(WiringDocument.GetCategoryName(cat), cat);
            docsByCategory.Add(cat, new List<WiringDocument>());
        }
        List<string> catNames = new List<string>(catFromName.Keys);
        catNames.Sort();

        // Cluster all documents by category
        foreach(WiringDocument wd in this.documents)
        { 
            docsByCategory[wd.category].Add(wd);
        }

        List<WiringDocument> lst = 
            new List<WiringDocument>();

        // Clear it out so we can rebuilt it.
        // Rebuild with organization rules
        foreach(string cn in catNames)
        { 
            WiringDocument.Category cat = catFromName[cn];
            List<WiringDocument> lstToSort = docsByCategory[cat];

            lstToSort.Sort(
                (x, y)=>
                { 
                    return string.Compare(x.GetName(), y.GetName()); 
                });

            lst.AddRange(lstToSort);
        }

        return lst;
    }

    public List<WiringDocument> GetDocumentsListCopy()
    { 
        return new List<WiringDocument>(this.documents);
    }

    public void Reset(IEnumerable<WiringDocument> wds)
    { 
        this.documentLookup.Clear();
        this.documents.Clear();

        foreach(WiringDocument doc in wds)
            this.AddDocument(doc);
    }

    public bool RepositionIndex(int prevPos, int newPos)
    {
        if(prevPos < 0 || newPos < 0)
            return false;

        if(prevPos >= this.documents.Count || newPos >= this.documents.Count)
            return false;

        WiringDocument doc = this.documents[prevPos];
        this.documents.RemoveAt(prevPos);
        this.documents.Insert(newPos, doc);

        return true;
    }
}

public class WiringCollection : WiringCollectionBase
{
    /// <summary>
    /// The current wiring being edited by the UI and played by the keyboard.
    /// </summary>
    protected WiringDocument activeDocument = null;
    public WiringDocument Active { get { return this.activeDocument; } }

    public List<string> documentMessages = new List<string>();
    List<string> DocumentMessages {get{return this.documentMessages; } }


    public LLDNOutput ActiveOutput()
    { 
        if(this.activeDocument == null)
            return null;

        return this.activeDocument.Output;
    }

    /// <summary>
    /// Load all wiring documents in a *.phon file.
    /// </summary>
    /// <param name="doc">The XML document in a phon format.</param>
    /// <param name="wdActive">The active found document.</param>
    /// <returns>True if the load was successful, else false.</returns>
    public bool LoadDocument(System.Xml.XmlDocument doc, bool clearFirst )
    {
        if(clearFirst == true)
            this.Clear();

        System.Xml.XmlElement root = doc.DocumentElement;

        // We have a seperate copy of what's loaded because if we're appending 
        // (if clearFirst is false) we could have other stuff in there. We need
        // a list of only what was created from this load.
        List<WiringDocument> loaded = new List<WiringDocument>();
        //
        Dictionary<string, WiringDocument> loadedByID = 
            new Dictionary<string, WiringDocument>();

        foreach (System.Xml.XmlElement ele in root)
        {
            if (ele.Name == "message")
                this.documentMessages.Add(ele.InnerText);
            else if (ele.Name == WiringDocument.xmlName)
            {
                string guid = string.Empty;
                System.Xml.XmlAttribute attrGUID = ele.Attributes["id"];
                if(attrGUID != null)
                    guid = attrGUID.Value;

                WiringDocument wd = new WiringDocument(false, guid);
                if (wd.LoadXML(ele) == false)
                    continue;

                loaded.Add(wd);
                loadedByID.Add(wd.guid, wd);

                this.AddDocument(wd);
            }
        }

        foreach(WiringDocument wd in loaded)
        { 
            Dictionary<string, LLDNBase> dirLocal = new Dictionary<string, LLDNBase>();
            foreach (LLDNBase gb in wd.Generators)
                dirLocal.Add(gb.GUID, gb);
        
            foreach (LLDNBase gb in wd.Generators)
                gb.PostLoadXML(dirLocal, loadedByID);
        }

        System.Xml.XmlAttribute attrActive = root.Attributes["active"];
        if (attrActive != null)
        {
            string activeGUID = attrActive.Value;
            foreach (WiringDocument wd in loaded)
            {
                if (wd.guid == activeGUID)
                {
                    this.activeDocument = wd;
                    break;
                }
            }
        }

        this.EnsureActive();

        return true;
    }

    public bool EnsureActive()
    {
        if (this.activeDocument == null && this.Count() > 0)
            this.activeDocument = this.GetDocument(0);

        return this.activeDocument != null;
    }

    /// <summary>
    /// Convert the wiring documents to an XML *.phon form.
    /// </summary>
    /// <returns>The wiring document.</returns>
    public System.Xml.XmlDocument SaveDocument()
    {
        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
        System.Xml.XmlElement root = doc.CreateElement("phonics");
        doc.AppendChild(root);

        foreach (WiringDocument wd in this.Documents)
        {
            System.Xml.XmlElement eleWD = wd.SaveXML(doc);
            root.AppendChild(eleWD);
        }

        if (this.activeDocument != null)
            root.SetAttribute("active", this.activeDocument.guid);

        return doc;
    }

    /// <summary>
    /// Get the XML string version of the currently loaded wiring documents.
    /// </summary>
    /// <returns>The XML string version of the currently loaded wiring documents.</returns>
    public string ConvertToXMLString()
    {
        System.Xml.XmlDocument doc =
                this.SaveDocument();

        using (var stringWriter = new System.IO.StringWriter())
        {
            System.Xml.XmlWriterSettings xmlWriteSettings = new System.Xml.XmlWriterSettings();
            xmlWriteSettings.OmitXmlDeclaration = true;
            xmlWriteSettings.Indent = true;
            xmlWriteSettings.NewLineOnAttributes = false;

            using (var xmlTextWriter = System.Xml.XmlWriter.Create(stringWriter, xmlWriteSettings))
            {
                doc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                string stringval = stringWriter.GetStringBuilder().ToString();

                return stringval;
            }
        }
    }

    //public bool RemoveNodeFromActive(GNBase node, bool removeConnections)
    //{ 
    //    if(this.activeDocument == null)
    //        return false;
    //
    //    return this.activeDocument.RemoveNode(node, removeConnections);
    //}

    public bool RemoveDocument(WiringDocument doc, bool resetActive = true)
    { 
        if(doc == null)
            return false;

        if(base.RemoveDocument(doc) == false)
            return false;

        if(this.activeDocument == doc)
        { 
            this.activeDocument = null;
            if(resetActive == true)
            { 
                if(this.Count() > 0)
                    this.activeDocument = this.GetDocument(0);
            }
        }
        return true;
    }

    public bool SetActiveDocument(WiringDocument doc, bool ignoreCurrent = true)
    { 
        if(ignoreCurrent == true && doc == this.activeDocument)
            return false;

        if(this.ContainsDocument(doc) == false)
            return false;
        
        this.activeDocument = doc;
        return true;
    }

    public new void Clear()
    { 
        this.activeDocument = null;

        base.Clear();

        this.documentMessages.Clear();
    }

    public new bool AddDocument(WiringDocument wd)
    { 
        return base.AddDocument(wd);
    }

    public new bool InsertDocument(WiringDocument wd, int idx)
    { 
        return base.InsertDocument(wd, idx);
    }

    public bool RemoveActiveDocument()
    { 
        if(this.activeDocument == null)
            return false;

        return this.RemoveDocument(this.activeDocument);
    }

    public WiringDocument AddNewWiringDocument(string docName, bool setActive)
    {
        WiringDocument wd = new WiringDocument(true, string.Empty);
        wd.SetName(docName);

        if(this.AddDocument(wd) == false)
            return null;

        if(setActive == true)
            this.SetActiveDocument(wd);

        return wd;
    }

    public static WiringDocument CreateBlankWiringDocument(string docName)
    { 
        WiringDocument wd = new WiringDocument(true, string.Empty);
        wd.SetName(docName);
        return wd;
    }

    public int GetActiveIndex()
    { 
        return this.GetWiringIndex(this.Active);
    }
}
