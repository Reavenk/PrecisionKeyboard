using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScale
{
    public string name;
    public int [] offsets;

    public MusicScale(string name, params int [] offsets)
    { 
        this.name = name;
        this.offsets = offsets;
    }

    public MusicScale(MusicScale ms)
    { 
        this.name = ms.name;
        this.offsets = ms.offsets;
    }

    public static MusicScale ScaleNone              = new MusicScale("None");
    public static MusicScale ScaleChromatic         = new MusicScale("Chromatic",           0);
    public static MusicScale ScaleMajor             = new MusicScale("Major",               0, 2, 4, 5, 7, 9, 11);
    public static MusicScale ScaleMajorPent         = new MusicScale("Major Pentatonic",    0, 2, 4, 7, 9);
    public static MusicScale ScaleMinor             = new MusicScale("Minor",               0, 2, 3, 5, 7, 8, 10);
    public static MusicScale ScaleMinorPent         = new MusicScale("Minor Pentatonic",    0, 2, 3, 7, 8);
    public static MusicScale ScaleHungarianMinor    = new MusicScale("Hungarian Minor",     0, 2, 3, 6, 7, 8, 11);
    public static MusicScale ScaleArabic            = new MusicScale("Arabic",              0, 1, 4, 5, 7, 8, 11);
    public static MusicScale ScalePersian           = new MusicScale("Persian",             0, 1, 3, 4, 5, 7, 10);

}
