using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamEditNickname : ParamEditBase
{
    ParamNickname paramNick = null;
    string origValue = string.Empty;

    public UnityEngine.UI.InputField input;

    public override bool SetParam(ParamNickname pn)
    {
        this.paramNick = pn;
        this.origValue = this.paramNick.GetStringValue();
        this.UpdateInputsFields();

        this.input.onValueChanged.AddListener(
            (x)=>{ this.OnInput_NickValueEdit(x); });

        return true;
    }

    public override void OnConfirm()
    {
        if (this.paramNick != null)
        {
            if (this.paramNick.GetStringValue() != this.origValue)
            {
                this.dlgRoot.application.SetLLDAWParamValue(
                    this.owner, 
                    this.paramNick, 
                    this.origValue, 
                    this.paramNick.GetStringValue());

                return;
            }
        }
    }

    void UpdateInputsFields()
    { 
        this.input.text = paramNick.GetStringValue();
    }

    public void OnInput_NickValueEdit(string x)
    {
        this.paramNick.SetValueFromString(x);
        this.baseThumb.UpdateDisplayValue();
    }
}
