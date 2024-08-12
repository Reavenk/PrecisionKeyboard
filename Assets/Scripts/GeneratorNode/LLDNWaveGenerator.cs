// MIT License
//
// Copyright(c) 2020 Pixel Precision LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;

public abstract class LLDNWaveGenerator : LLDNBase
{
    /// <summary>
    /// The extra gain to the volume
    /// </summary>
    protected ParamFloat gain;

    /// <summary>
    /// The parameter that controls the frequency of the wave.
    /// </summary>
    protected ParamTimeLen timeLen;

    static EnumVals enumValsSync = null;
    public static EnumVals GetSyncEnumVals()
    { 
        if(enumValsSync == null)
        { 
            enumValsSync = 
                new EnumVals(
                    new EnumVals.Entry("note",      0, "Note Oct"),
                    new EnumVals.Entry("mul",       1, "Note Mul"),
                    new EnumVals.Entry("beat",      2, "BPM Oct"),
                    new EnumVals.Entry("beatmul",   3, "BPM Mul"),
                    new EnumVals.Entry("const",     4, "Const"));

        }
        return enumValsSync;
    }

    public static float GetFreqFromSync(int e, float input, float freq, float beatsPerSec)
    { 
        switch(e)
        { 
            case 0:
                return freq * Mathf.Pow(2.0f, input);

            case 1:
                return freq * input;

            case 2:
                return beatsPerSec * Mathf.Pow(2.0f, input);

            case 3:
                return beatsPerSec * input;

            case 4:
                return input;

        }

        return 0.0f;
    }

    public static float GetEnvLenFromSync(int e, float input, float freq, float beatsPerSec)
    {
        switch (e)
        {
            case 0:
                return 1.0f / (freq * Mathf.Pow(2.0f, input));

            case 1:
                if(input == 0.0f)
                    return 1.0f / freq;

                if(input > 0.0f)
                    return 1.0f / (freq * (1.0f + input));
                else
                    return 1.0f / (freq / (1.0f - input));

            case 2:
                return beatsPerSec * Mathf.Pow(2.0f, input);

            case 3:
                if (input == 0.0f)
                    return 1.0f / beatsPerSec;

                if (input > 0.0f)
                    return 1.0f / (beatsPerSec * (1.0f + input));
                else
                    return 1.0f / (beatsPerSec / (1.0f - input));

            case 4:
                return Mathf.Abs(input);

        }

        return 0.0f;
    }

    public LLDNWaveGenerator()
        : base()
    { }

    public LLDNWaveGenerator(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.gain = new ParamFloat("Gain", "Volume", 0.8f, 0.0f, 1.0f, "clampeddial");
        this.gain.description = "The amplitude and volume of the wave.";
        this.genParams.Add(this.gain);

        this.timeLen = new ParamTimeLen("Freq", 1.0f, ParamTimeLen.TimeLenType.FrequencyMul, ParamTimeLen.WidgetFreq);
        this.timeLen.description = "The frequency of the audio signal, depending on the settings of the parameter.";
        this.genParams.Add(this.timeLen);
    }
}
