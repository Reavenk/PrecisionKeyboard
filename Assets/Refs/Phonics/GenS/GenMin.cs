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
        /// <summary>
        /// Return the minimum value per-sample between two PCM streams.
        /// </summary>
        public class GenMin : GenBase
        {
            /// <summary>
            /// One of the PCM streams.
            /// </summary>
            GenBase gma;

            /// <summary>
            /// The other PCM stream.
            /// </summary>
            GenBase gmb;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="gma">One of the PCM streams.</param>
            /// <param name="gmb">The other PCM stream.</param>
            public GenMin(GenBase gma, GenBase gmb)
                : base(0.0f, 0)
            {
                this.gma = gma;
                this.gmb = gmb;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                FPCM fa = pcmFactory.GetZeroedFPCM(start, size);
                FPCM fb = pcmFactory.GetZeroedFPCM(start, size);

                float[] a = fa.buffer;
                float[] b = fb.buffer;

                gma.Accumulate(a, start, size, prefBuffSz, pcmFactory);
                gmb.Accumulate(b, start, size, prefBuffSz, pcmFactory);

                for (int i = start; i < start + size; ++i)
                    data[i] = (a[i] < b[i]) ? a[i] : b[i];
            }

            public override PlayState Finished()
            {
                return ResolveTwoFinished(this.gma, this.gmb);
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.gma);
                lst.Add(this.gmb);
            }
        }
    }
}