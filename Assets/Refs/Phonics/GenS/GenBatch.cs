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
        /// Combines the output of several GenBases.
        /// </summary>
        public class GenBatch : GenBase
        {
            /// <summary>
            /// The PCM streams to combine.
            /// </summary>
            GenBase [] batch;

            /// <summary>
            /// The PCM streams to combine.
            /// </summary>
            /// <param name="batch"></param>
            public GenBatch(GenBase [] batch)
                : base(0.0f, 0)
            {
                this.batch = batch;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                if(this.batch.Length == 0)
                    return;

                float inv = 1.0f / this.batch.Length;

                FPCM fpcm = null;
                foreach(GenBase gb in this.batch)
                {
                    if(fpcm == null)
                        fpcm = pcmFactory.GetZeroedFPCM(start, size);
                    else
                        fpcm.Zero(start, size);

                    float [] rf = fpcm.buffer;

                    gb.Accumulate(rf, start, size, prefBuffSz, pcmFactory);

                    for(int i = start; i < start + size; ++i)
                        data[i] += rf[i] * inv;
                }
                
            }

            public override PlayState Finished()
            {
                PlayState ret = PlayState.Finished;

                foreach(GenBase gb in this.batch)
                { 
                    PlayState ps = gb.Finished();

                    if(ps == PlayState.Playing)
                        return PlayState.Playing;

                    if(ps == PlayState.Constant)
                        ret = PlayState.Constant;
                }

                return ret;
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                foreach(GenBase gb in this.batch)
                    lst.Add(gb);
            }
        }
    }
}
