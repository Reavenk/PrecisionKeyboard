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

using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        /// <summary>
        /// Generates a PCM for a sine wave.
        /// </summary>
        public class GenSineWave : GenWave
        {
            /// <summary>
            /// The amount to scale the ellapsed time to 
            /// </summary>
            private double timeToFreq;
            public double TimeToFreq { get { return this.timeToFreq; } }

            public GenSineWave(float freq, int samplesPerSec, float amplitude)
                : base(freq, samplesPerSec, amplitude)
            {
                this.timeToFreq = dTau * freq;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                double tIt = this.CurTime;
                double incr = this.TimePerSample;
                for (int i = start; i < start + size; ++i)
                {
                    data[i] = Mathf.Sin((float)(tIt * this.TimeToFreq)) * this.amplitude;
                    tIt += incr;
                }
            }

            public override PlayState Finished()
            {
                return PlayState.Constant;
            }

            public override void ReportChildren(List<GenBase> lst)
            {}
        }
    }
}