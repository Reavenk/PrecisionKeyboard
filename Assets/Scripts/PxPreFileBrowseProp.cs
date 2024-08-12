using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PxPreFileBrowseProp : PxPre.FileBrowse.FileBrowseProp
{
    public override UnityEngine.UI.Button CreateButton(Transform parent, string text, params System.Type [] btnCmp)
    { 
        PushdownButton pshbtn = 
            this.CreateTypedButton<PushdownButton>(parent, text);

        pshbtn.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);

        return pshbtn;
    }
}
