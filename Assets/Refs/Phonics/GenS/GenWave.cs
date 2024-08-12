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

namespace PxPre
{ 
    namespace Phonics
    { 
        /// <summary>
        /// The base class for wave generators.
        /// </summary>
        public abstract class GenWave : GenBase
        {
            /// <summary>
            /// The frequency of the wave.
            /// </summary>
            private float freq;
            public float Freq { get { return this.freq; } }

            /// <summary>
            /// The volume to generate the wave at.
            /// </summary>
            public float amplitude = 1.0f;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="freq">The frequency of the wave.</param>
            /// <param name="samplesPerSec">The hertz of the audio sample being generated</param>
            /// <param name="amplitude">The volume of the wave.</param>
            protected GenWave(float freq, int samplesPerSec, float amplitude)
                : base(0.0, samplesPerSec)
            {
                this.freq = freq;
                
                this.amplitude = amplitude;

                
            }
        }
    }
}