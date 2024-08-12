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

namespace PxPre
{
    namespace Phonics
    {
        /// <summary>
        /// Utility class for generating western music notes.
        /// 12-semitone temperment, with A4 matched to 440hz.
        /// </summary>
        public static class WesternFreqUtils
        {
            /// <summary>
            /// The keys in an octave.
            /// </summary>
            public enum Key
            {
                A, As,
                B,
                C, Cs,
                D, Ds,
                E,
                F, Fs,
                G, Gs
            }

            public static Key RandomKey()
            { 
                switch(Random.Range(0, 12))
                { 
                    case 0: return Key.A;
                    case 1: return Key.As;
                    case 2: return Key.B;
                    case 3: return Key.C;
                    case 4: return Key.Cs;
                    case 5: return Key.D;
                    case 6: return Key.Ds;
                    case 7: return Key.E;
                    case 8: return Key.F;
                    case 9: return Key.Fs;
                    case 10: return Key.G;
                    case 11: return Key.Gs;
                }

                return Key.C; // Arbitrary, shouldn't ever get here though
            }

            /// <summary>
            /// The inversions of GetNote().
            /// </summary>
            /// <param name="note">The note value</param>
            /// <param name="n">The key that the note id evaluates to.</param>
            /// <param name="octave">The octave that the note id evaluates to.</param>
            public static void GetNoteInfo(int note, out Key k, out int octave)
            {
                octave = note / 12;
                int st = note % 12;

                switch (st)
                {
                    case 0:
                        k = Key.C;
                        break;

                    case 1:
                        k = Key.Cs;
                        break;

                    case 2:
                        k = Key.D;
                        break;

                    case 3:
                        k = Key.Ds;
                        break;

                    case 4:
                        k = Key.E;
                        break;

                    case 5:
                        k = Key.F;
                        break;

                    case 6:
                        k = Key.Fs;
                        break;

                    case 7:
                        k = Key.G;
                        break;

                    case 8:
                        k = Key.Gs;
                        break;

                    case 9:
                        k = Key.A;
                        break;

                    case 10:
                        k = Key.As;
                        break;

                    case 11:
                        k = Key.B;
                        break;

                    default:
                        k = Key.C;
                        octave = -1;
                        break;
                }
            }

            /// <summary>
            /// Given a key and octave, return an integer id representing its semitone
            /// value on a relative scale.
            /// </summary>
            /// <param name="k">The key of the note.</param>
            /// <param name="octave">The octave of the note.</param>
            /// <returns>An integer value representing the semitone. The actual number isn't
            /// as important is its relationship to other results.</returns>
            public static int GetNote(Key k, int octave)
            {
                int octbase = octave * 12;

                switch (k)
                {
                    case Key.C:
                        return octbase + 0;

                    case Key.Cs:
                        return octbase + 1;

                    case Key.D:
                        return octbase + 2;

                    case Key.Ds:
                        return octbase + 3;

                    case Key.E:
                        return octbase + 4;

                    case Key.F:
                        return octbase + 5;

                    case Key.Fs:
                        return octbase + 6;

                    case Key.G:
                        return octbase + 7;

                    case Key.Gs:
                        return octbase + 8;

                    case Key.A:
                        return octbase + 9;

                    case Key.As:
                        return octbase + 10;

                    case Key.B:
                        return octbase + 11;

                    default:
                        return -1;
                }
            }

            /// <summary>
            /// Given a note described by a key and octave, return the
            /// frequency of the note.
            /// </summary>
            /// <param name="k">The key of the note.</param>
            /// <param name="octave">The octave of the note.</param>
            /// <returns>The frequency of the note.</returns>
            public static float GetFrequency(Key k, int octave)
            {
                int baseline = GetNote(Key.A, 4);
                int key = GetNote(k, octave);
                int diff = key - baseline;

                float A4Fr = 440.0f;
                return A4Fr * Mathf.Pow(2.0f, (float)diff / 12.0f);
            }

            /// <summary>
            /// Given a note id, return the frequency of the note.
            /// </summary>
            /// <param name="note">A note id, generated by GetNote()</param>
            /// <returns>The frequency of the note.</returns>
            public static float GetFrequency(int note)
            {
                Key k;
                int o;
                GetNoteInfo(note, out k, out o);

                return GetFrequency(k, o);
            }
        }
    }
}
