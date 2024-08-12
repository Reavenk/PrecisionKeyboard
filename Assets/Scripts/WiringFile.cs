using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiringFile : WiringCollection
{
    bool dirty = false;
    public bool Dirty {get{return this.dirty; } }

    string filepath;
    string filename;
    string displayName;

    public string Path {get{return filepath; } }
    public string Filename {get{return this.filename; } }

    public string GetDisplayName(bool interpret, bool dirtyAsterisk)
    {
        string ret = "Untitled";

        if(interpret == false)
            ret = this.displayName;
        else
        {
            if (string.IsNullOrEmpty(displayName) == false)
                ret = displayName;
            else if(string.IsNullOrEmpty(filename) == false)
                ret = filename;
        }

        if(dirtyAsterisk == true && this.dirty == true)
            ret = $"<i>{ret}</i>*";

        return ret;
    }

    public void SetDisplayName(string name)
    { 
        this.displayName = name;
    }

    //public void SetDirty(bool newDirty = true, bool force = false)
    //{ 
    //    if(this.dirty == newDirty && force == false)
    //        return;
    //
    //    this.dirty = newDirty;
    //    this.BroadcastDirtyChange();
    //}
    //
    //public void ClearDirty(bool force = false)
    //{ 
    //    this.SetDirty(false, force);
    //}

    public bool Load(string filepath, bool append = false)
    {
        if(append == false)
            this.Clear();

        try
        {
            string fileContents = System.IO.File.ReadAllText(filepath);
            if(this.LoadXML(
                fileContents, 
                append == false, 
                append) == false)
            {
                return false;
            }

            this.SetFilepath(filepath, true, true);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error loading document: " + ex.Message);
            return false;
        }

    }

    public bool LoadXML(string xmlContent, bool clearDirty, bool append = false)
    {
        try
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmlContent);
            return this.LoadXML(doc, clearDirty, append);
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error loading document: " + ex.Message);
            return false;
        }

    }


    public bool LoadXML(System.Xml.XmlDocument doc, bool clearDirty, bool append = false)
    {
        if(this.LoadDocument(doc, append == false) == false)
            return false;

        return true;
    }

    public bool Save(string filepath)
    {
        string docXmlString = this.ConvertToXMLString();

        this.SetFilepath(filepath, true, false);

        if (string.IsNullOrEmpty(docXmlString) == true)
            return false; //TODO: Error

        try
        {
            System.IO.File.WriteAllText(this.filepath, docXmlString);
        }
        catch(System.Exception ex)
        { 
            Debug.Log("Error when trying to save wiring: " + ex.Message);
            return false;
        }

        return true;
    }

    public void SetFilepath(string filepath, bool clearDisplay, bool clearDirty)
    {
        this.filepath = filepath;
        this.filename = System.IO.Path.GetFileName(filepath);

        if (clearDisplay)
            this.displayName = string.Empty;
    }

    public bool Save()
    { 
        return this.Save(this.filepath);
    }

    public new void Clear()
    { 
        base.Clear();

        this.filepath = string.Empty;
        this.filename = string.Empty;
        this.displayName = string.Empty;
        this.dirty = false;
    }

    public WiringFile CreatePotentialSuccessor()
    { 
        WiringFile wf = new WiringFile();

        return wf;
    }

    public new bool AddDocument(WiringDocument wd)
    { 
        if(base.AddDocument(wd) == false)
            return false;

        return true;
    }

    public new bool Append(WiringCollectionBase wd)
    { 
        if(base.Append(wd) == true)
        {
            return true;
        }
        return false;
    }

    public new bool RemoveDocument(WiringDocument doc)
    {
        if(base.RemoveDocument(doc, true) == false)
            return false;

        return true;
    }
}
