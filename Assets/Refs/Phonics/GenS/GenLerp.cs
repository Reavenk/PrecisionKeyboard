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

namespace PxPre
{
    namespace Phonics
    {
        public class GenLerp : GenBase
        { 
            GenBase gma;
            GenBase gmb;
            GenBase gmFactor;

            public GenLerp(GenBase gma, GenBase gmb, GenBase gmFactor)
                : base(0.0f, 0)
            { 
                this.gma = gma;
                this.gmb = gmb;
                this.gmFactor = gmFactor;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                FPCM fa = pcmFactory.GetZeroedFPCM(start, size);
                FPCM fb = pcmFactory.GetZeroedFPCM(start, size);
                FPCM ff = pcmFactory.GetZeroedFPCM(start, size);

                float [] a = fa.buffer;
                float [] b = fb.buffer;
                float [] f = ff.buffer;

                this.gma.Accumulate(        a, start, size, prefBuffSz, pcmFactory);
                this.gmb.Accumulate(        b, start, size, prefBuffSz, pcmFactory);
                this.gmFactor.Accumulate(   f, start, size, prefBuffSz, pcmFactory);

                for(int i = start; i < start + size; ++i)
                    data[i] = a[i] + (b[i] - a[i]) * f[i];
            }

            public override PlayState Finished()
            {
                PlayState psA = this.gma.Finished();
                PlayState psB = this.gmb.Finished();

                // If we're modulating and one is signaling it's just going to
                // only return 0.0 from now one, we're done.
                if (psA == PlayState.Finished || psB == PlayState.Finished)
                    return PlayState.Finished;

                if (psA == PlayState.NotStarted && psB == PlayState.NotStarted)
                    return PlayState.NotStarted;

                if (psA == PlayState.Constant || psB == PlayState.Constant)
                    return PlayState.Constant;

                return PlayState.Playing;
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.gma);
                lst.Add(this.gmb);
                lst.Add(this.gmFactor);
            }
        }
    }
}