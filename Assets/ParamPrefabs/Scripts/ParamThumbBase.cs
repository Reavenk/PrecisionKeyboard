using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamThumbBase : MonoBehaviour
{
    private WiringCollection collection;
    public WiringCollection Collection {get{return this.collection; } }

    private WiringDocument document;
    public WiringDocument Document {get{return this.document; } }

    protected IWiringEditorBridge editorBridge;

    public virtual bool SetParamBool(IWiringEditorBridge editorBridge, ParamBool pb)
    {
        this.editorBridge = editorBridge;
        return false;
    }

    public virtual bool SetParamFloat(IWiringEditorBridge editorBridge, ParamFloat pf)
    {
        this.editorBridge = editorBridge;
        return false;
    }

    public virtual bool SetParamInt(IWiringEditorBridge editorBridge, ParamInt pi)
    {
        this.editorBridge = editorBridge;
        return false;
    }

    public virtual bool SetParamEnum(IWiringEditorBridge editorBridge, ParamEnum pe)
    {
        this.editorBridge = editorBridge;
        return false;
    }

    public virtual bool SetParamTimeLen(IWiringEditorBridge editorBridge, ParamTimeLen ptl)
    {
        this.editorBridge = editorBridge;
        return false;
    }

    public virtual bool SetParamWireReference(IWiringEditorBridge editorBridge, ParamWireReference pwr)
    {
        this.editorBridge = editorBridge;
        return false;
    }

    public virtual bool SetParamNickname(IWiringEditorBridge editorBridge, ParamNickname nickname)
    {
        this.editorBridge = editorBridge;
        return false;
    }

    public virtual ParamBool GetParamBool()
    {return null; }

    public virtual ParamFloat GetParamFloat()
    {return null; }

    public virtual ParamInt GetParamInt()
    {return null; }

    public virtual ParamEnum GetParamEnum()
    {return null; }

    public virtual ParamTimeLen GetParamTimeLen()
    { return null;}

    public virtual ParamNickname GetParamNickname()
    { return null;}

    public virtual ParamWireReference GetParamWireReference()
    { return null;}

    public virtual bool UpdateDisplayValue()
    { 
        return false;
    }

    public void SetRectTransformInParent()
    { 
        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;

        RectTransform rt = this.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }


    public void AttachToEdit(IWiringEditorBridge editorBridge, WiringCollection collection, WiringDocument parentDocument, ParamEditBase editBase)
    { 
        this.collection = collection;
        this.document = parentDocument;
        this.editorBridge = editorBridge;

        ParamBool pb = this.GetParamBool();
        if(pb != null)
        {
            editBase.SetParam(pb);
            return;
        }

        ParamInt pi = this.GetParamInt();
        if(pi != null)
        { 
            editBase.SetParam(pi);
            return;
        }

        ParamFloat pf = this.GetParamFloat();
        if(pf != null)
        { 
            editBase.SetParam(pf);
            return;
        }

        ParamEnum pe = this.GetParamEnum();
        if(pe != null)
        { 
            editBase.SetParam(pe);
            return;
        }

        ParamTimeLen ptl = this.GetParamTimeLen();
        if(ptl != null)
        { 
            editBase.SetParam(ptl);
            return;
        }

        ParamNickname ptn = this.GetParamNickname();
        if(ptn != null)
        { 
            editBase.SetParam(ptn);
            return;
        }

        ParamWireReference pwr = this.GetParamWireReference();
        if(pwr != null)
        { 
            editBase.SetParam(pwr);
            return;
        }
    }
}
