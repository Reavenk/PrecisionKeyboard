// <copyright file=".cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDispDefault : KeyDispBase
{

    public Sprite exerciseIcon;

    public override bool AvailableDuringExercise(PaneKeyboard.ExerciseDisplayMode exDisp)
    {
        return true;
    }

    public override Sprite GetTabIcon(bool exerciseRunning)
    {
        return 
            exerciseRunning ?
                this.exerciseIcon : 
                this.tabIcon;
    }

    public override string GetTabString(bool exerciseRunning)
    {
        return 
            exerciseRunning ?
                "Exercise" :
                this.tabLabel;
    }
}
