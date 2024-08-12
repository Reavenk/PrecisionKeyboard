using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Application : MonoBehaviour
{
    const string prefMetroVolume = "metrovol";

    public enum MetroType
    { 
        Silent = -1,
        Click,
        Stick,
        Kick,
        Clap
    }

    [System.Serializable]
    public struct MetronomePattern
    { 
        public string name;
        public MetroType [] pattern;
    }

    [System.Serializable]
    public struct MetronomeInstr
    { 
        public AudioClip audioClip;

        [System.NonSerialized]
        public float [] samples;

        [System.NonSerialized]
        // This is going to be redundant because they're 
        // all expected to be the same samplerate.
        public int samplesPerSec;

        [System.NonSerialized]
        public int sampleCt;
    }

    public MetronomeInstr [] metronomeInstruments;

    /// <summary>
    /// The AudioSource for the tap sound when the user clicks the BPM tap.
    /// </summary>
    public AudioSource tapAudioSource;

    public const float validTapTime = 5.0f;
    List<float> bpmTapTimes = new List<float>();

    int metroWrite = 0;
    int metroNextStart = 0;
    int metroTick = 0;

    float metronomeVolume = 0.7f;

    public int metronomeIdx = 0;
    public MetronomePattern [] metronomePatterns;

    MetroType [] metroTypes = 
        new MetroType[]
        { 
            MetroType.Kick,
            MetroType.Silent,
            MetroType.Click,
            MetroType.Silent,
            MetroType.Click,
            MetroType.Silent,
            MetroType.Click
        };

    int beat = 0;

    /// <summary>
    /// The AudioSource to play the metronome from.
    /// </summary>
    public AudioSource metronomeSource;

    public bool IsMetronomePlaying()
    { 
        return this.metronomeSource.isPlaying;
    }

    public bool ToggleMetronome()
    { 
        if(this.metronomeSource.isPlaying == false)
        { 
            this.StartMetronome();
            return true;
        }

        this.StopMetronome();
        return false;
    }

  
    public void StartMetronome()
    {
        if(this.metronomeSource.isPlaying == true)
            return;

        this.metroWrite = 0;
        this.metroNextStart = 0;
        this.metroTick = 0;
        this.beat = -1;

        AudioClip ac = AudioClip.Create(
            "Metro", 
            4096, 
            1, 
            this.metronomeInstruments[0].samplesPerSec, true, this.PCMReaderCallback);

        this.metronomeSource.clip = ac;
        this.metronomeSource.Play();
        this.metronomeSource.volume = this.metronomeVolume;

        foreach (TabRecord tr in this.tabs)
            tr.pane.OnMetronomeStart();
    }

    public void SetMetronomeVolume(float vol)
    { 
        vol = Mathf.Clamp01(vol);
        if(this.metronomeVolume == vol)
            return;

        this.metronomeVolume = vol;

        if(this.metronomeSource != null)
            this.metronomeSource.volume = vol;

        PlayerPrefs.SetFloat(prefMetroVolume, vol);

        foreach(TabRecord tr in this.tabs)
            tr.pane.OnMetronomeVolumeChange(vol);
    }

    public float GetMetronomeVolume()
    { 
        return this.metronomeVolume;
    }

    void PCMReaderCallback(float[] data)
    { 
        int i = 0;
        int sz = data.Length;

        int samplesPerSec = this.metronomeInstruments[0].samplesPerSec;

        while(i < sz)
        {
            // We do the + length part to account for when we start, we start
            // with a beat of -1 and a sacraficial write of nothing that instantly
            // increments to a beat of 1.
            int idx = (beat + this.metroTypes.Length) % this.metroTypes.Length;

            MetroType mt = this.metroTypes[idx];

            if(mt == MetroType.Silent)
            { 
                this.metroWrite = 0;
            }
            else
            {
                MetronomeInstr mi = this.metronomeInstruments[(int)mt];

                float [] samps = mi.samples;

                if(this.metroWrite < mi.sampleCt)
                { 
                    int writeAmt = Mathf.Min(sz - i, mi.sampleCt - this.metroWrite, metroNextStart);
                    int writeEnd = this.metroWrite + writeAmt;
                    for( ; this.metroWrite < writeEnd; ++this.metroWrite)
                    { 
                        data[i] = samps[this.metroWrite];
                        ++i;
                    }
                    metroNextStart -= writeAmt;
                }
            }

            if(this.metroNextStart <= 0)
            { 
                ++beat;
                this.metroWrite = 0;

                double bps = (double)this.BeatsPerMinute() / 60.0;
                bps *= 4.0f; // We're going to play sound at double pace
                long thisTick = (long)(samplesPerSec / bps * this.metroTick);
                ++this.metroTick;
                long nextTick = (long)(samplesPerSec / bps * this.metroTick);

                this.metroNextStart = (int)(nextTick - thisTick);

                continue;
            }

            int skipLeft = Mathf.Min(sz - i, this.metroNextStart);
            int skipEnd = i + skipLeft;
            for(; i < skipEnd; ++i)
                data[i] = 0.0f;
            
            this.metroNextStart -= skipLeft;
        }
    }

    public void StopMetronome()
    {
        this.metroWrite = 0;
        this.metroNextStart = 0;

        this.metronomeSource.Stop();

        foreach(TabRecord tr in this.tabs)
            tr.pane.OnMetronomeStop();
    }

    public void SetMetronomeButtonImage(UnityEngine.UI.Image img, bool state)
    { 
        if(state == true)
        {
            img.color = Color.green;
        }
        else
        {
            img.color = Color.white;
        }
    }

    public string GetBPMString()
    { 
        return this.BeatsPerMinute().ToString("0.000");
    }

    public bool PushTapMeter(bool playClick)
    {
        this.bpmTapTimes.Add(Time.time);
        float validTime = Time.time - validTapTime;
        this.bpmTapTimes.RemoveAll((x) => { return x < validTime; });

        // Don't play if metronome is playing since it's going to
        // restart that sound.
        if(playClick == true && this.IsMetronomePlaying() == false)
            this.tapAudioSource.Play();

        if (this.bpmTapTimes.Count < 2)
            return false;

        float ct = 0.0f;
        float accum = 0.0f;

        for (int i = 0; i < this.bpmTapTimes.Count - 1; ++i)
        {
            ct += 1.0f;
            accum += this.bpmTapTimes[i + 1] - this.bpmTapTimes[i];
        }

        this.SetBPM(ct / accum * 60.0f);
        return true;
    }

    public void SetMetronomeID(int idx)
    { 
        this.metroTypes = this.metronomePatterns[idx].pattern;
        this.metronomeIdx = idx;

        if(this.IsMetronomePlaying() == true)
        { 
            this.StopMetronome();
            this.StartMetronome();
        }
    }
}
