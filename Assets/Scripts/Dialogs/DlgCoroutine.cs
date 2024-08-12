// TODO: Check if can be removed.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DlgCoroutine : MonoBehaviour
{
    public UnityEngine.UI.Text title;
    public UnityEngine.UI.Text message;
    public UnityEngine.UI.Text cancelText;

    public System.Action onCancel = null;
    public Coroutine coroutine = null;

    public void OnButton_Cancel()
    { 
        if(this.coroutine != null)
            this.StopCoroutine(this.coroutine);

        if(this.onCancel != null)
            this.onCancel();

        GameObject.Destroy(this.gameObject);
    }


}
