using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionButtonBase<ty>
{
    public ty payload;

    public PxPre.UIL.EleGenButton<PushdownButton> button;
    public PxPre.UIL.EleText title;
    public PxPre.UIL.EleText descr;

    public PxPre.UIL.EleBoxSizer infoSizer;
    public PxPre.UIL.EleText instr;

    public void SetSelected(bool sel, IConnectionButtonBridge dlg)
    { 
        if(sel == true)
        {
            button.Button.targetGraphic.color = dlg.SelectedButtonColor;
            this.infoSizer.Remove(this.instr);
            this.instr.text.gameObject.SetActive(false);

        }
        else
        { 
            button.Button.targetGraphic.color = dlg.UnselectedButtonColor;
            if(this.infoSizer.HasEntry(this.instr) == false)
                this.infoSizer.Add(this.instr, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

            this.instr.text.gameObject.SetActive(true);
        }

        dlg.SetLayoutDirty();
    }
}
