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
        /// Generate white noise PCM data.
        /// </summary>
        public class GenWhite : GenBase
        {
            const int NoiseBufferSz = 44100;

            int noiseIt = 0;
            static float [] bakedNoise = null;

            float amplitude = 1.0f;

            public static void BakeNoise()
            { 
                if(bakedNoise != null)
                    return;

                bakedNoise = new float[NoiseBufferSz];
                for(int i = 0; i < NoiseBufferSz; ++i)
                    bakedNoise[i] = Random.value;
            }

            // We throw away the frequency parameter, it does nothing for us.
            public GenWhite(float amplitude)
                : base(0.0, 0)
            { 
                this.amplitude = amplitude;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {

                int i = start;
                while(size > 0)
                { 
                    if(noiseIt >= NoiseBufferSz)
                        this.noiseIt = 0;

                    int sz = Mathf.Min(NoiseBufferSz - this.noiseIt, size); 
                    int end = i + sz;
                
                    for (; i < end; ++i)
                        data[i] = bakedNoise[i] * this.amplitude;
                    
                    noiseIt += sz;
                    size -= sz;
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