﻿// MIT License
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
        /// Raises a PCM stream to the third power.
        /// </summary>
        public class GenCube : GenBase
        {
            // The PCM stream
            GenBase input;

            /// <summary>
            /// The input PCM stream.
            /// </summary>
            /// <param name="input"></param>
            public GenCube(GenBase input)
                : base(0.0f, 0)
            {
                this.input = input;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                FPCM fa = pcmFactory.GetZeroedFPCM(start, size);
                float[] a = fa.buffer;
                this.input.Accumulate(a, start, size, prefBuffSz, pcmFactory);

                for (int i = start; i < start + size; ++i)
                    data[i] = a[i] * a[i] * a[i];
            }

            public override PlayState Finished()
            {
                if (this.input == null)
                    return PlayState.Finished;

                return this.input.Finished();
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                if (this.input != null)
                    lst.Add(this.input);
            }
        }
    }
}
