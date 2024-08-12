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
        /// Linear decay envelope.
        /// </summary>
        public class GenLinDecay : GenBase
        {
            /// <summary>
            /// The PCM stream to apply the decay to.
            /// </summary>
            GenBase gen;

            /// <summary>
            /// The duration of the decay, in seconds.
            /// </summary>
            double duration;

            /// <summary>
            /// The number of audio samples to let pass before starting the envelope.
            /// This should euqla startTime * samplesPerSec.
            /// </summary>
            int offsetSamples = 0;

            /// <summary>
            /// The start time of the envelope.
            /// </summary>
            double startTime;

            /// <summary>
            /// The number of samples left before the decay is finished.
            /// </summary>
            int durationSamples = 0;

            /// <summary>
            /// The number of samples the duration should last. This should equal
            /// samplesPerSec * duration
            /// </summary>
            int totalDurationSamples = 0;

            /// <summary>
            /// The final sustain volume after the decay is finished.
            /// </summary>
            float sustain = 0.0f;

            /// <summary>
            /// The cached inverted value of the sustain value. This should
            /// equal 1.0 - sustain.
            /// </summary>
            float invSustain = 1.0f;


            public GenLinDecay(double startTime, double duration, float sustain, int samplesPerSec, GenBase gen)
               : base(0.0f, samplesPerSec)
            {
                this.startTime = startTime;

                this.gen = gen;
                this.duration = duration;

                this.offsetSamples = (int)(startTime * samplesPerSec);
                this.durationSamples = (int)(duration * samplesPerSec);
                this.totalDurationSamples = this.durationSamples;
                this.sustain = sustain;
                this.invSustain = 1.0f - sustain;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                if(this.durationSamples <= 0)
                {
                    if(this.sustain == 0.0f)
                    {
                        // If duration is over and we don't have sustain, early exit. We account
                        // for what we would have written as a timer for Finished() do it doesn't
                        // exit too early.
                        this.durationSamples -= size;
                    }
                    else
                    {
                        // If duration is over, we're in sustain
                        FPCM fsus = pcmFactory.GetZeroedFPCM(start, size);
                        float[] sus = fsus.buffer;
                        this.gen.Accumulate(sus, start, size, prefBuffSz, pcmFactory);
                        for(int i = start; i < start + size; ++i)
                            data[i] = sus[i] * this.sustain;
                    }
                    return;
                }

                // Are we at a time before the decay is activated?
                // If it's going to be that way for the entire decay, just 
                // relay it.
                if (this.offsetSamples > size)
                {
                    this.offsetSamples -= size;
                    this.gen.Accumulate(data, start, size, prefBuffSz, pcmFactory);
                    return;
                }

                FPCM fa = pcmFactory.GetZeroedFPCM(start, size);
                float[] a = fa.buffer;
                this.gen.Accumulate(a, start, size, prefBuffSz, pcmFactory);

                // Are we at a time before the decay is activated, but 
                // will need to start the decay before we exit?
                if (this.offsetSamples > 0)
                { 
                    int endOS = start + this.offsetSamples;
                    for(int i = start; i < start + endOS; ++i)
                        data[i] = a[i];

                    size -= this.offsetSamples;
                    start += this.offsetSamples;
                    this.offsetSamples = 0;
                }

                // The decay. We have two versions, because if the sustain ramps to zero, we can 
                // avoid some lerp math.
                float total = totalDurationSamples;
                int decSamps = Mathf.Min(size, this.durationSamples);
                int end = start + decSamps;
                if(this.sustain == 0.0f)
                {
                    float durs = (float)this.durationSamples;
                    for (int i = start; i < end; ++i)
                    {
                        float lam = durs / total;
                        data[i] = a[i] * lam;
                        durs -= 1.0f;

                    }
                    durationSamples -= decSamps;
                    
                }
                else
                {

                    float durs = (float)this.durationSamples;
                    for (int i = start; i < end; ++i)
                    {
                        float lam = sustain + durs / total * this.invSustain;
                        data[i] += a[i] * lam;
                        durs -= 1.0f;
                    }
                    start += decSamps;
                    size -= decSamps;
                    this.durationSamples -= decSamps;

                    // If we finish the ramp in the middle, we need to fill the rest with sustain
                    for(int i = start; i < start + size; ++i)
                        data[i] = a[i] * this.sustain;
                }
            }

            public override PlayState Finished()
            {
                if(this.durationSamples <= -this.SamplesPerSec)
                    return PlayState.Finished;

                return this.gen.Finished();
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.gen);
            }
        }
    }
}