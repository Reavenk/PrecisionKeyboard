using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamThumbNick : ParamThumbBase
{
    public UnityEngine.UI.Text nicknameText;
    protected ParamNickname nickname;

    public override bool SetParamNickname(IWiringEditorBridge editorBridge, ParamNickname nickname)
    {
        base.SetParamNickname(editorBridge, nickname);

        this.nickname = nickname;
        this.UpdateDisplayValue();
        return true;
    }

    public override ParamNickname GetParamNickname()
    { 
        return nickname; 
    }

    public override bool UpdateDisplayValue()
    { 
        if(this.nickname == null)
            return false;

        this.nicknameText.text = 
            this.nickname.GetStringValue();

        return true;
    }
}
