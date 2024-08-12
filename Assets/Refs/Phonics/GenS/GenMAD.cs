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
        /// Performs a multiple of a PCM stream to a constant, followed
        /// by an addition of another constant.
        /// </summary>
        public class GenMAD : GenBase
        {
            /// <summary>
            /// The PCM stream to perform the MAD on.
            /// </summary>
            GenBase input;

            /// <summary>
            /// The multiplication value.
            /// </summary>
            float mul;

            /// <summary>
            /// The addition value.
            /// </summary>
            float add;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="input">The PCM stream to perform the MAD on.</param>
            /// <param name="mul">The multiplication value.</param>
            /// <param name="add">The addition value.</param>
            public GenMAD(GenBase input, float mul, float add)
                : base(0.0f, 0)
            { 
                this.input = input;
                this.mul = mul;
                this.add = add;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                FPCM fa = pcmFactory.GetZeroedFPCM(start, size);
                float[] a = fa.buffer;
                this.input.Accumulate(a, start, size, prefBuffSz, pcmFactory);

                for (int i = start; i < start + size; ++i)
                    data[i] = a[i] * this.mul + this.add;
            }

            public override PlayState Finished()
            {
                if(input == null)
                    return PlayState.Finished;

                return this.input.Finished();
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.input);
            }
        }

    }
}