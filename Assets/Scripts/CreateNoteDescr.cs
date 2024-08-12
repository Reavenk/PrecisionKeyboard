using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CreateNoteDescr
{
    public int octaveOffset;
    public PxPre.Phonics.WesternFreqUtils.Key key;

    // The offset of where it make the key from the 
    // start of the octave. See creation of black keys 
    // in CreateKeyboard() for more information.
    public int offset;
    public bool black;
    public string noteName;

    public CreateNoteDescr(int octaveOffset, PxPre.Phonics.WesternFreqUtils.Key key, string noteName, bool black, int offset)
    {
        this.octaveOffset = octaveOffset;
        this.key = key;
        this.noteName = noteName;

        this.black = black;
        this.offset = offset;
    }
}