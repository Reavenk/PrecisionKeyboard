// <copyright file="IMIDIPollCountering.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>
// A management interface to be used with MIDIOutputPollScope and MIDIInputPollScope
// to track when polling should happen because something is watching the MIDI input 
// devices or MIDI output devices.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMIDIPollCountering
{
    bool IncrementInputPollCounter();
    bool DecrementInputPollCounter();

    bool IncrementOutputPollCounter();
    bool DecrementOutputPollCounter();
}
