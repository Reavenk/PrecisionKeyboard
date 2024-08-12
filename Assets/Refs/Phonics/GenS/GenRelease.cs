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
        public class GenRelease : GenBase
        { 
            GenBase input;

            int maxLen;
            int curLen;
            bool active = false;

            public GenRelease(int samplesPerSec, GenBase input, float seconds)
                : base(0.0f, samplesPerSec)
            { 
                this.input = input;

                this.maxLen = (int)(samplesPerSec * seconds);
                this.maxLen = Mathf.Max(this.maxLen, 1);
                this.curLen = this.maxLen;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                if(this.active == false)
                { 
                    // If not ready, just relay as if we weren't here
                    this.input.Accumulate(data, start, size, prefBuffSz, pcmFactory);
                    return;
                }

                // if we've passed the end of release, we're still going to add silent
                // data for a while and record for how long we've been doing that.
                //
                // See this.Finished() for more information.
                if(this.curLen <= 0)
                {
                    this.curLen -= size;
                    return;
                }

                FPCM fa = pcmFactory.GetZeroedFPCM(start, size);
                float[] a = fa.buffer;
                this.input.Accumulate(a, start, size, prefBuffSz, pcmFactory);

                float c = this.curLen;
                int sz = Mathf.Min(this.curLen, size);
                this.curLen -= sz;

                float fmax = (float)this.maxLen;
                for (int i = start; i < start + sz; ++i)
                { 
                    c -= 1.0f;
                    float lam = c / fmax;
                    lam = lam * lam;
                    data[i] = lam * a[i];
                }
            }

            public override PlayState Finished()
            { 
                if(this.input == null)
                    return PlayState.Finished;

                PlayState psInput = this.input.Finished();

                if(this.active == false)
                    return psInput;

                if(psInput == PlayState.Finished)
                    return PlayState.Finished;

                // We don't check if it's equal than, because we want 
                // AccumulateImpl() to be reached one more time before finishing
                // so we know the buffer written right before was played.
                    if (this.curLen < -this.SamplesPerSec)
                    return PlayState.Finished;

                return PlayState.Playing;
            }

            public override void Release()
            {
                this.active = true;
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.input);
            }
        }
    }
}
