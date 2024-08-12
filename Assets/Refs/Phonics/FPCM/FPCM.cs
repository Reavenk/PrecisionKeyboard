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
        public class FPCM
        {
            public FPCMFactory factory;
            public float [] buffer;

            public FPCM(FPCMFactory factory, float [] buffer)
            {
                this.factory = factory;
                this.buffer = buffer;
            }

            public bool CheckMinsize(int size)
            { 
                if(size < 1)
                    return false;

                if(this.buffer.Length < size)
                {
                    this.buffer = new float[size];
                    return true;
                }

                return false;
            }

            public bool Release()
            { 
                if(this.factory == null)
                    return false;

                return this.factory.ReturnFPCM(this);
            }

            public void Zero()
            { 
                this.Zero(this.buffer.Length);
            }

            public void Zero(int samples)
            { 
                for(int i = 0; i < samples; ++i)
                    this.buffer[i] = 0.0f;
            }

            public void Zero(int start, int samples)
            { 
                for(int i = start; i < samples; ++i)
                    this.buffer[i] = 0.0f;
            }
        }
    }
}