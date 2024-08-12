using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamEditBase : UnityEngine.EventSystems.EventTrigger
{
    protected ParamThumbBase baseThumb;
    protected EditParamDlgRoot dlgRoot;
    protected LLDNBase owner;

    public virtual bool SetParam(ParamBool pb){return false; }
    public virtual bool SetParam(ParamInt pi){return false; }
    public virtual bool SetParam(ParamFloat pf){return false; }
    public virtual bool SetParam(ParamEnum pe){return false; }
    public virtual bool SetParam(ParamTimeLen ptl){return false;}
    public virtual bool SetParam(ParamNickname ptn){return false; }
    public virtual bool SetParam(ParamWireReference pwr){return false; }

    public virtual bool Initialize(EditParamDlgRoot root, LLDNBase owner, ParamThumbBase baseThumb)
    { 
        this.dlgRoot = root;
        this.baseThumb = baseThumb;
        this.owner = owner;
        return true;
    }

    public virtual void OnConfirm() { }
}
