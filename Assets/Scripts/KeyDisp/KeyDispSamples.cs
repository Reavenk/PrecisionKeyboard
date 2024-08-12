// <copyright file=".cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDispSamples : KeyDispBase
{
    public GameObject [] bufferSizeUIObjects;

    public override bool AvailableDuringExercise(PaneKeyboard.ExerciseDisplayMode exDisp)
    {
        return true;
    }

    public override void Init(Application app, PaneKeyboard keyboard)
    { 
        foreach(GameObject go in this.bufferSizeUIObjects)
            go.SetActive(false);
    }
}
