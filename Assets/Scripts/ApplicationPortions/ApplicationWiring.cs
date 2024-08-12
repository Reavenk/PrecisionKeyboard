using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Application : MonoBehaviour
{
    /// <summary>
    /// The text to show the filename of the document.
    /// </summary>
    public UnityEngine.UI.Text filenameText;

    //public TextAsset startingInstruments;

    [System.Serializable]
    public class InternalDocument
    { 
        public string idName;
        public string title;
        public TextAsset textAsset;
        public string resourceName;
    }

    public List<InternalDocument> internalDocuments = new List<InternalDocument>();

    public void LoadStartingDocument()
    { 
        this.LoadInternalDocument("startingdoc");
    }

    public bool LoadInternalDocument(string idName)
    { 
        foreach(InternalDocument intDoc in this.internalDocuments)
        { 
            if(intDoc.idName == idName)
                return this.LoadInternalDocument(intDoc);
        }
        return false;
    }

    public bool LoadInternalDocument(InternalDocument doc)
    {
        string textContent = null;

        if(doc.textAsset != null)
            textContent = doc.textAsset.text;
        else
        { 
            TextAsset ta = Resources.Load<TextAsset>(doc.resourceName);
            if(ta != null)
                textContent = ta.text;
        }

        if(string.IsNullOrEmpty(textContent) == true)
            return false;

        try
        {
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(textContent);

            if(this.wirings.LoadDocument(xmlDoc, true) == false)
                return false;

            foreach (string str in this.wirings.documentMessages)
                this.EnqueueDocumentMessage(str);
        }
        catch(System.Exception ex)
        { 
            Debug.Log($"Error loading internal document {doc.idName} : {ex.Message}");
            return false;
        }

        this.wirings.SetDisplayName(doc.title);
        this.undoMgr.ClearSession();
        this.FlagAppStateDirty();
        this.DoDropdownTextUpdate(this.filenameText);
        this.SetActiveDocument(this.wirings.Active, true);
        return true;
    }

    public void AddDefaultWiringDocument(bool setDirty)
    {
        this.AddNewWiringDocument("Default");
    }

    public void AddNewWiringDocument(string docName)
    {
        WiringDocument wdoc = WiringCollection.CreateBlankWiringDocument(docName);

        this.AddWiringDocument(wdoc, true);
    }

    void RemakeTitlebar()
    {
        string title = this.wirings.GetDisplayName(true,true);
        if(this.undoMgr.IsDirty() == true)
            title = $"<i>{title}*</i>";

        this.filenameText.text = title;

    }

    public bool SetActiveDocument(WiringDocument wd, bool force)
    {
        if (this.wirings.SetActiveDocument(wd, force == false) == false)
            return false;

        this.RefreshActiveOutput();

        foreach(IAppEventSink iaes in this.eventSinks)
            iaes.OnWiringActiveChanges(this.Wirings, this.Wirings.Active);

        return true;
    }

    public bool SetActiveDocument(int programId, bool collapseModal, bool force)
    { 
        if(this.wirings == null || this.wirings.Count() == 0)
            return false;

        WiringDocument wd = this.Wirings.GetDocument(programId);
        if(wd == null)
            return false;

        if(wd == this.Wirings.Active && force == false)
            return false;

        ModalStack.CollapseStack();
        return this.SetActiveDocument(wd, force);
    }

    public bool LoadDocumentFile(string path)
    {
        try
        {
            WiringFile newLoad = this.wirings.CreatePotentialSuccessor();
            newLoad.Load(path);

            this.wirings = newLoad;

            this.undoMgr.ClearSession();
            this.FlagAppStateDirty();

            foreach (TabRecord tr in this.tabs)
                tr.pane.OnDocumentChanged(newLoad, newLoad.Active, false); // TODO: REmove this update

            return true;
        }
        catch(System.Exception)
        { 
            // TODO: On error...
            return false;
        }
    }

    public bool LoadDocumentString(string str)
    {
        try
        {
            WiringFile newLoad = this.wirings.CreatePotentialSuccessor();

            if(newLoad.LoadXML(str, true, false) == false)
                return false;

            this.wirings = newLoad;

            this.undoMgr.ClearSession();
            this.FlagAppStateDirty();

            foreach (TabRecord tr in this.tabs)
                tr.pane.OnDocumentChanged(newLoad, newLoad.Active, false);

            return true;
        }
        catch(System.Exception)
        { 
            return false;
        }

    }

    public bool AppendDocumentFile(string path)
    {
        try
        {
            WiringFile newLoad = this.wirings.CreatePotentialSuccessor();
            newLoad.Load(path, true);

            if(newLoad.Count() == 0)
                return false;

            this.Wirings.Append(newLoad);

            using( PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Appended Document(s)"))
            { 
                foreach(WiringDocument wd in newLoad.Documents)
                    this.AddWiringDocument(wd, false);
            }

            return true;
        }
        catch(System.Exception)
        { 
            // TODO: On error...
            return false;
        }
    }


    public bool DeleteActiveWiring()
    {
        if(this.DeleteWirings(this.Wirings.Active) == false)
            return false;

        // TODO: Figure out integrating into undo with this
        if (this.Wirings.Count() == 0)
            this.AddDefaultWiringDocument(true);

        // TODO: Figure out integrating into undo with this
        this.SetActiveDocument(this.wirings.Active, true);
        return true;
    }

    public bool CloneActiveWiring(string newName)
    {
        if(this.wirings.Active == null)
            return false;

        if(string.IsNullOrEmpty(newName) == true)
            newName = this.wirings.Active.GetName();

        WiringDocument wdClone = this.wirings.Active.Clone(newName);

        using( PxPre.Undo.UndoDropSession usd = new PxPre.Undo.UndoDropSession(this.undoMgr, $"Cloned {this.Wirings.Active.GetProcessedWiringName(this.Wirings)}"))
        { 
            this.AddWiringDocument(wdClone);
        }

        return true;
    }
}
