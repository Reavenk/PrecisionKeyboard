// TODO: Check if can be removed.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DlgTestQuery : MonoBehaviour
{
    public UnityEngine.UI.Text title;
    public UnityEngine.UI.Text message;
    public UnityEngine.UI.Text instr;
    public UnityEngine.UI.InputField input;

    public UnityEngine.UI.Text confirmButtonText;
    public UnityEngine.UI.Text cancelButtonText;

    public System.Action<string> onConfirm = null;
    public System.Action onCancel = null;

    public void OnButton_Confirm()
    { 
        if(this.onConfirm != null)
            this.onConfirm(this.input.text);

        GameObject.Destroy(this.gameObject);
    }

    public void OnButton_Cancel()
    { 
        if(this.onCancel != null)
            this.onCancel();

        GameObject.Destroy(this.gameObject);
    }
}
