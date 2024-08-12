// <copyright file="MIDIOutput.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>A class to wrap the representation of a MIDI Output device.</summary>

public interface MIDIOutput
{ 
    bool Equivalent(MIDIOutput output);
    int Port();
    string DeviceName();
    string Manufacture();
    string Product();
    bool Activate();
    bool Deactivate();

    bool SendKeyPress(MIDIMgr mgr, int midinote, int channel, sbyte velocity);
    bool SendKeyRelease(MIDIMgr mgr, int midinote, int channel);
}