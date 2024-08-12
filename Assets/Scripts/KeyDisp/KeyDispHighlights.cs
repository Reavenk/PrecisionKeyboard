// <copyright file=".cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDispHighlights : KeyDispBase
{
    public Sprite icoROYGBIVOn;
    public Sprite icoROYGBIVOff;
    public PulldownInfo pulldownROYG;

    public override bool AvailableDuringExercise(PaneKeyboard.ExerciseDisplayMode exDisp)
    {
        return false;
    }

    public void OnButton_ROYGBIV()
    {
        PxPre.DropMenu.Props dropProp = PxPre.DropMenu.DropMenuSingleton.MenuInst.props;
        Color cSel = dropProp.selectedColor;
        Color cUns = dropProp.unselectedColor;

        PxPre.DropMenu.StackUtil stack = new PxPre.DropMenu.StackUtil();
        KeyCollection.OctaveHighlighting oh = this.keyboard.OctaveHighlighting;

        stack.AddAction(
            this.icoROYGBIVOn,
            (oh == KeyCollection.OctaveHighlighting.ROYGBIVM) ? cSel : cUns,
            "ROYGBIVM Background",
            () => { this.OnMenu_OctaveROYGBIVM(); });

        stack.AddAction(
            this.icoROYGBIVOff,
            (oh == KeyCollection.OctaveHighlighting.Black) ? cSel : cUns,
            "Black Background",
            () => { this.OnMenu_OctaveBlack(); });


        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            stack.Root,
            this.pulldownROYG.rootPlate);

        this.app.DoVibrateButton();
    }

    public void UpdateROYGBIV(KeyCollection.OctaveHighlighting oh, bool anim)
    { 
        switch(oh)
        { 
            case KeyCollection.OctaveHighlighting.Black:
                this.pulldownROYG.icon.sprite = icoROYGBIVOff;
                break;

            case KeyCollection.OctaveHighlighting.ROYGBIVM:
                this.pulldownROYG.icon.sprite = icoROYGBIVOn;
                break;
        }

        if(anim == true)
            pulldownROYG.SetInfoAnimate(this.app);
    }

    public void OnMenu_OctaveROYGBIVM()
    {
        this.keyboard.SetOctaveBackground(KeyCollection.OctaveHighlighting.ROYGBIVM);
    }

    public void OnMenu_OctaveBlack()
    {
        this.keyboard.SetOctaveBackground(KeyCollection.OctaveHighlighting.Black);
    }


}
