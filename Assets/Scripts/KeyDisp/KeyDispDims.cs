// <copyright file=".cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDispDims : KeyDispBase
{
    public List<GameObject> disabledDuringExercises;

    public override bool AvailableDuringExercise(PaneKeyboard.ExerciseDisplayMode exDisp)
    {
        return 
            (exDisp == PaneKeyboard.ExerciseDisplayMode.KeyboardExercise);
    }

    public override void OnShow(bool exercise)
    { 
        if(this.disabledDuringExercises != null)
        { 
            foreach(GameObject go in this.disabledDuringExercises)
                go.SetActive(!exercise);
        }

        base.OnShow(exercise);
    }
}
