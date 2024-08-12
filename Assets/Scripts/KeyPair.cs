// <copyright file="KeyPiar.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>A pairing of a music key and octave to represent a note.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct KeyPair
{
    public PxPre.Phonics.WesternFreqUtils.Key key;
    public int octave;

    public KeyPair(PxPre.Phonics.WesternFreqUtils.Key key, int octave)
    {
        this.key = key;
        this.octave = octave;
    }

    public static bool Matches(KeyPair kpA, KeyPair kpB)
    {
        return 
            kpA.key == kpB.key && 
            kpA.octave == kpB.octave;
    }
}