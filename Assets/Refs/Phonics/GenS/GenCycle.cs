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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenCycle : GenBase
        {
        
            struct Recording
            { 
                public FPCM buffer;
                public readonly int cachedStart;
                public readonly int cachedEnd;
                public readonly int length;

                public Recording(FPCM buffer, int offset)
                { 
                    this.buffer = buffer;
                    this.cachedStart = offset;
                    this.length = this.buffer.buffer.Length;
                    this.cachedEnd = offset + this.length;
                }
            }

            int offset = 0;
            int recordAmt;
            GenBase input;
            
            // When recording, how much is left on the last recording entry??
            int lastRecordingLeft = 0;
            // The sample count of the last recording's end position when lined up with 
            // all the other buffers;
            int lastRecordingEnd = 0;
            int totalRecordingLeft;

            // When doing playback, the buffer to stream from
            int bufferIdx = 0;
            int playbackIt = 0;
            List<Recording> recordings = new List<Recording>();

            public GenCycle(int offsetSamples, int recordAmt, GenBase input)
                : base(0.0f, 0)
            { 
                this.offset = offsetSamples;
                this.input = input;
                this.recordAmt = recordAmt;
                this.totalRecordingLeft = recordAmt;

                // Kept until ready to playback
                this.playbackIt = 0;
            }
        
            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                if(this.offset > 0)
                {
                    int of = Min(this.offset, size);
                    this.offset -= of;
                    start += of;
                    size -= of;

                    if(size == 0)
                        return;
                }

                while(this.totalRecordingLeft > 0)
                { 
                    if(this.lastRecordingLeft <= 0)
                    { 
                        FPCM fpcm = pcmFactory.GetZeroedGlobalFPCM(0, prefBuffSz);
                        Recording r = new Recording(fpcm, this.lastRecordingEnd);
                        this.recordings.Add(r);
                        this.lastRecordingEnd += r.length;
                        this.lastRecordingLeft = r.length;
                    }

                    Recording rLast = this.recordings[this.recordings.Count - 1];
                    int recordingHead = rLast.length - this.lastRecordingLeft;

                    int saveLen = Min(this.lastRecordingLeft, this.totalRecordingLeft);
                    saveLen = Min(size, saveLen);

                    // Accumulate into the recording
                    float [] lbuf = rLast.buffer.buffer;
                    this.input.Accumulate(lbuf, recordingHead, saveLen, prefBuffSz, pcmFactory);

                    // Not only do we have to save it while recording, we also need to 
                    // output it...
                    for(int i = 0; i < start+saveLen; ++i)
                        data[start + i] = lbuf[recordingHead + i];

                    // And update everything that needs updating
                    size -= saveLen;
                    start += saveLen;
                    this.lastRecordingLeft -= saveLen;
                    this.totalRecordingLeft -= saveLen;

                    this.recordings[this.recordings.Count - 1] = rLast;

                    if(this.totalRecordingLeft == 0)
                    {
                        // If the recording is thin, it's in our best interest to expand it as much as possible
                        // so we can do one loop with cyclical information with the buffer we have instead 
                        // of many small broken loops (in a bigger loop) afterwards.
                        if(this.recordings.Count == 1 && (this.recordAmt / recordings[0].length) > 1)
                        { 
                            int loopsToFit = this.recordAmt / recordings[0].length;
                            float [] rf = recordings[0].buffer.buffer;
                            for(int i = 1; i < loopsToFit; ++i)
                            { 
                                int offset = i * this.recordAmt;
                                for(int j = 0; j < this.recordAmt; ++j)
                                    rf[offset + j] = rf[j];
                            }
                            this.recordAmt *= loopsToFit;
                        }
                    }
                    // If we handled all we could, we'll continue later.
                    if(size == 0)
                        return;
                }

                

                // Dunno how this would happen, but a simple sanity check.
                if(this.recordings.Count == 0)
                    return;

                while(size > 0)
                { 
                    // Time for playback!
                    Recording cur = this.recordings[this.bufferIdx];
                    float [] curBuf = cur.buffer.buffer;

                    // The lowest sample counter between 
                    // - when the current buffer we're reading ends
                    // - when we've reached the end of all playback (which probably won't align with the end buffer count)
                    // - the number we're allowed to write for the requested size.
                    int canPlay = Min(cur.cachedEnd - playbackIt, size);
                    canPlay = Min(canPlay, this.recordAmt - this.playbackIt);

                    int writehead = this.playbackIt - cur.cachedStart;
                    for(int i = 0; i < canPlay; ++i)
                        data[start + i] = curBuf[writehead + i];

                    // Increment and update
                    start += canPlay;
                    size -= canPlay;
                    //
                    this.playbackIt += canPlay;
                    if(this.playbackIt >= cur.cachedEnd)
                    {
                        this.bufferIdx += 1;
                        if(this.bufferIdx >= this.recordings.Count)
                        { 
                            this.bufferIdx = 0;
                            this.playbackIt = 0;
                        }
                    }
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
                foreach(Recording r in this.recordings)
                    r.buffer.Release();
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.input);
            }

        }
    }
}
