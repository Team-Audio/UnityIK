using System;
using UnityEngine;
using Minis;

public class FluidSynthReceiver : ASynthesizer
{
    [SerializeField] private string m_soundfont = "C:\\soundfonts\\default.sf2";
    private FluidSynth m_synth;
    [Range(0,10)]
    [SerializeField] private float Amp = 2;

    private void Awake()
    {
        m_synth = new FluidSynth(25);
        int id  = m_synth.SFLoad(m_soundfont);
        m_synth.ProgramSelect(0, id, 0, 0);
    }
    
    public override void PlayNote(MidiNoteControl note, float velocity)
    {
        m_synth.NoteOn(0, note.noteNumber, (int)(velocity*127.0f));
    }
    public override void ReleaseNote(MidiNoteControl note)
    {
        m_synth.NoteOff(0, note.noteNumber);
    }
    
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if(channels == 2)
        {
            int sampleRate = data.Length / channels;
            var src = m_synth.GetSamplesInterleaved(sampleRate);
            for (int i = 0; i < src.Length; i++)
            {
                src[i] = src[i] * Amp;
            }
            Buffer.BlockCopy(src,0,data,0,data.Length * sizeof(float));
        }
        else if (channels == 1)
        {
            int sampleRate = data.Length;
            var (left, _) = m_synth.GetSamples(sampleRate);
            data = left;
        }
    }
}
