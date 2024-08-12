// <copyright file="MIDIInput.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>A class to wrap the representation of a MIDI Input device.</summary>

public interface MIDIInput
{ 
    bool Equivalent(MIDIInput input);
    bool Activate();
    bool Deactivate();
    string DeviceName();
    string Description();
}
