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
        public class GenDelay : GenBase
        {
            int voices;
            GenBase input;

            public struct BufferRef
            { 
                /// <summary>
                /// The total number of samples that have been written
                /// </summary>
                public int totalOffset;

                /// <summary>
                /// The sample we left off at in the entry buffers[this.idx]
                /// </summary>
                public int sampleIdx;

                /// <summary>
                /// The index into buffers that we're concerned about.
                /// </summary>
                public int idx;


                /// <summary>
                /// cached volume attenuation.
                /// </summary>
                public float atten;
            }

            public struct BufferedFPCM
            { 
                public readonly FPCM buffer;
                public readonly int cachedLen;
                public readonly int cachedEnd;
                public readonly int offset;

                public int readLeft;

                public BufferedFPCM(FPCM fpcm, int offset)
                { 
                    this.buffer = fpcm;
                    this.offset = offset;
                    this.cachedLen = fpcm.buffer.Length;
                    this.cachedEnd = offset + this.cachedLen;
                    this.readLeft = this.cachedLen;
                }
            }

            /// <summary>
            /// The next offset to pass into BufferedFPCM constructor.
            /// </summary>
            int nextOffset = 0;

            List<BufferedFPCM> buffers = new List<BufferedFPCM>();

            /// <summary>
            /// An entry for every voice, of where they're reading from.
            /// </summary>
            BufferRef [] rbuffRef = null;

            public GenDelay(int samplesPerSec, GenBase input, int voices, float offsetTime, float volAtten)
                : base(0.0f, samplesPerSec)
            { 
                this.voices = Mathf.Max(voices, 1);

                int sampleDist = (int)(samplesPerSec * offsetTime);
                sampleDist = Mathf.Max(1, sampleDist);

                this.input = input;

                this.rbuffRef = new BufferRef[this.voices];
                float voiceVol = 1.0f;
                for(int i = 0; i < this.voices; ++i)
                {
                    this.rbuffRef[i] = new BufferRef();
                    this.rbuffRef[i].idx = 0;
                    this.rbuffRef[i].sampleIdx = 0;
                    this.rbuffRef[i].totalOffset = -sampleDist * i;
                    this.rbuffRef[i].atten = voiceVol;

                    voiceVol *= volAtten;
                }
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBufSz, FPCMFactoryGenLimit pcmFactory)
            {
                //  BYPASS IF NO POINT NOT TO
                ////////////////////////////////////////////////////////////////////////////////
                if(this.voices <= 1)
                { 
                    // This will probably rarely ever happen because it defeats the purpose of using
                    // this node, but I'll take the optimization where I can get it.
                    this.input.Accumulate(data, start, size, prefBufSz, pcmFactory);
                }

                //  READ IN NEW INFORMATION BY APPENDING BUFFERS
                ////////////////////////////////////////////////////////////////////////////////
                int readAmt = size;
                while(readAmt > 0)
                { 
                    if(buffers.Count == 0)
                    { 
                        FPCM firstEntry = pcmFactory.GetZeroedGlobalFPCM(0, prefBufSz);
                        BufferedFPCM bfpmFirst = new BufferedFPCM(firstEntry, this.nextOffset);
                        this.nextOffset += firstEntry.buffer.Length;

                        this.buffers.Add(bfpmFirst);
                    }

                    int lastIdx = this.buffers.Count - 1;
                    BufferedFPCM last = this.buffers[lastIdx];

                    int readBufAmt = Min(readAmt, last.readLeft);
                    if(readBufAmt != 0)
                    { 
                        int readHead = last.cachedLen - last.readLeft;
                        // Read the new stuff requested
                        this.input.Accumulate(
                            last.buffer.buffer, 
                            readHead, 
                            readBufAmt, 
                            prefBufSz, 
                            pcmFactory);

                        // Update and save it back
                        last.readLeft -= readBufAmt;
                        this.buffers[lastIdx] = last;
                        readAmt -= readBufAmt;
                    }

                    if(readAmt <= 0)
                        break;

                    // If it's not enough, create another buffer and continue. We 
                    // only create it here, because it will be filled the next 
                    // round-about in this loop.
                    //
                    // Exact same as above.
                    FPCM newEntry = pcmFactory.GetZeroedGlobalFPCM(0, prefBufSz);
                    BufferedFPCM bfpm = new BufferedFPCM(newEntry, this.nextOffset);
                    this.nextOffset += newEntry.buffer.Length;
                    this.buffers.Add(bfpm);
                }

                //  WRITE INTO OUTPUT BUFFER
                ////////////////////////////////////////////////////////////////////////////////
                
                int lowestIndexVal = int.MaxValue;

                for( int buffIt = 0; buffIt < this.rbuffRef.Length; ++buffIt)
                { 
                    int bsize = size;
                    int bstart = start;

                    BufferRef br = this.rbuffRef[buffIt];

                    while(bsize > 0)
                    { 
                        if(br.totalOffset < 0)
                        { 
                            int toSkip = Min(-br.totalOffset, bsize);
                            bsize -= toSkip;
                            bstart += toSkip;
                            br.totalOffset += toSkip;

                            if(bsize <= 0)
                                break;
                        }

                        int endOffset = br.totalOffset + bsize;
                        BufferedFPCM buffer = this.buffers[br.idx];
                        float [] a = buffer.buffer.buffer; // Hmm, tic tac toe
                        int endBuffer = buffer.cachedEnd;
                        // Either going to read to the end of the buffer, or the 
                        // end of the requested stream read, whichever comes first
                        int canReadLeft = Min(endOffset, endBuffer) - br.totalOffset;

                        int startingOffset = br.totalOffset - buffer.offset;
                        for(int i = 0; i < canReadLeft; ++i)
                            data[bstart + i] += br.atten * a[startingOffset + i];

                        bstart += canReadLeft;
                        bsize -= canReadLeft;
                        br.totalOffset += canReadLeft;

                        // If there's still more to read, that needs to be done
                        // through the next buffer.
                        if(bsize > 0)
                            ++br.idx;

                    }

                    lowestIndexVal = Min(lowestIndexVal, br.idx);
                    this.rbuffRef[buffIt] = br;
                }
                // MAINTENENCE, GET RID OF OLD GRODY STUFF
                ////////////////////////////////////////////////////////////////////////////////
                if(lowestIndexVal != 0)
                { 
                    for(int i = 0; i < lowestIndexVal; ++i)
                        this.buffers[i].buffer.Release();
                
                    this.buffers.RemoveRange(0, lowestIndexVal);
                
                    for( int i = 0; i < this.rbuffRef.Length; ++i)
                        this.rbuffRef[i].idx -= lowestIndexVal;
                }
            }

            public override PlayState Finished()
            { 
                if(this.input == null)
                    return PlayState.Finished;

                return this.input.Finished();
            }

            public override void Deconstruct()
            {
                foreach(BufferedFPCM b in this.buffers)
                    b.buffer.Release();

                this.buffers.Clear();
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.input);
            }
        }
    }
}
