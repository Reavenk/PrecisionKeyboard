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
        /// Linear Attack/Decay/Sustain/Release envelope on a PCM stream.
        /// </summary>
        public class GenADSR : GenBase
        {
            GenBase input;

            /// <summary>
            /// The amount of samples left before the attack kicks on
            /// </summary>
            int offset;

            /// <summary>
            /// The current sample iterator value of the attack we're on
            /// </summary>
            int attackIt;

            /// <summary>
            /// The total number of attack samples to process.
            /// </summary>
            int attackTotal;

            /// <summary>
            /// The number of decay samples left
            /// </summary>
            int decayLeft;

            /// <summary>
            /// The total number of decay samples 
            /// </summary>
            int decayTotal;

            /// <summary>
            /// The number of release samples left
            /// </summary>
            int releaseLeft;

            /// <summary>
            /// The total number of release samples to process
            /// </summary>
            int releaseTotal;
            float sustain = 0.5f;

            /// <summary>
            /// A value of how many samples we've gone past a finished release
            /// as a timer to make sure the device has time to play the buffered
            /// release.
            /// </summary>
            int saftey;

            /// <summary>
            /// If true, we've been released, if not, we're still in attack, 
            /// decay, or sustain.
            /// </summary>
            bool released = false;

            public GenADSR(
                GenBase input,
                int samplesPerSec, 
                float offset, 
                float attackTime,
                float decayTime,
                float sustainValue,
                float releaseTime)
                : base(0.0f, samplesPerSec)
            { 
                this.input = input;
                this.sustain = sustainValue;

                this.offset = (int)(offset * samplesPerSec);
                this.attackIt = 0;
                this.attackTotal = (int)(attackTime * samplesPerSec);
                this.decayTotal = (int)(decayTime * samplesPerSec);
                this.decayLeft = this.decayTotal;

                if (releaseTime <= 0)
                { 
                    this.releaseTotal = 0;
                    this.releaseLeft = 0;
                    this.saftey = 0;
                }
                else
                { 
                    this.releaseTotal = (int)(releaseTime * samplesPerSec);
                    this.releaseLeft = this.releaseTotal;
                    this.saftey = samplesPerSec;
                }
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBufSz, FPCMFactoryGenLimit pcmFactory)
            {
                // Early release
                if (this.released == true)
                {
                    if (this.releaseLeft > 0)
                    {
                        FPCM fpcmRl = pcmFactory.GetZeroedFPCM(start, size);
                        float[] fprl = fpcmRl.buffer;

                        // We could probably optimize this by only calling AccumulateImpl_NonReleasePart()
                        // when we're not in sustain yet.
                        this.AccumulateImpl_NonReleasePart(fprl, start, size, prefBufSz, pcmFactory);

                        float total = this.releaseTotal;
                        float left = this.releaseLeft;
                        int sampsCt = Mathf.Min(this.releaseLeft, size);

                        for (int ei = start; ei < start + sampsCt; ++ei)
                        {
                            data[ei] = fprl[ei] * (left / total);
                            left -= 1.0f;
                        }
                        this.releaseLeft -= sampsCt;
                        return;
                    }
                    else if (this.saftey > 0)
                        this.saftey -= size;
                }
                else
                {
                    this.AccumulateImpl_NonReleasePart(data, start, size, prefBufSz, pcmFactory);
                }
            }

            public void AccumulateImpl_NonReleasePart(float [] data, int start, int size, int prefBufSz, FPCMFactoryGenLimit pcmFactory)
            { 
                FPCM fpcm = pcmFactory.GetZeroedFPCM(start, size);
                float[] fp = fpcm.buffer;
                this.input.Accumulate(fp, start, size, prefBufSz, pcmFactory);

                if(this.offset > 0)
                { 
                    int ofrm = Mathf.Min(this.offset, size);
                    this.offset -= ofrm;
                    start += ofrm;
                    size -= ofrm;
                    
                    if(size <= 0)
                        return;
                }

                // ATTACK
                if (this.attackIt < this.attackTotal)
                { 
                    int atrm = Mathf.Min(this.attackTotal - this.attackIt, size);
                    int end = start + atrm;
                    float totalAt = this.attackTotal;
                    float at = this.attackIt;
                    for(int i = start; i < end; ++i)
                    {
                        data[i] = fp[i] * (at/totalAt);
                        at += 1.0f;
                    }

                    this.attackIt += atrm;

                    size -= atrm;
                    start += atrm;
                    if(size <= 0)
                        return;
                }

                // DECAY
                if(this.decayLeft > 0)
                { 
                    int dcrm = Mathf.Min(this.decayLeft, size);
                    int dcend = start + dcrm;
                    float totalDc = this.decayTotal;
                    float cd = this.decayLeft;
                    float susDiff = 1.0f - this.sustain;
                    for(int i = start; i < dcend; ++i)
                    { 
                        data[i] = fp[i] * (this.sustain + (cd / totalDc) * susDiff);
                        cd -= 1.0f;
                    }

                    this.decayLeft -= dcrm;

                    size -= dcrm;
                    start += dcrm;
                    if(size <= 0)
                        return;
                }

                // If we're still here, all that's left is sustain
                for( int i = start; i < start + size; ++i)
                    data[i] = fp[i] * this.sustain;
            }

            public override PlayState Finished()
            { 
                if(this.input == null)
                    return PlayState.Finished;

                if(this.released == false)
                    return this.input.Finished();

                if(this.saftey <= 0)
                    return PlayState.Finished;

                return PlayState.Playing;

            }

            public override void ReportChildren(List<GenBase> lst)
            { 
                if(this.input != null)
                    lst.Add(this.input);
            }

            public override void Release()
            { 
                this.released = true;
            }
        }
    }
}