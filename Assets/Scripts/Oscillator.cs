using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Helper;
using Minis;
[System.Serializable]
public struct Envelope
{
    public double Attack;
    public double Decay;

    public double Release;
    public double Amplitude;
    public double StartAmp;

    public double timePressed;
    public double timeReleased;
    public bool IsOn;

    public Envelope(double pAttack = 0.1, double pDecay = 0.1, double pStartAmp = 1.0, double pAmp = 0.8, double pRelease = 0.2, bool pOn = false, double pPressed = 0, double pReleased = 0)
    {
        Attack = pAttack;
        Decay = pDecay;
        StartAmp = pStartAmp;
        Amplitude = pAmp;
        Release = pRelease;

        timePressed = pPressed;
        timeReleased = pReleased;
        IsOn = pOn;
    }


    public static Envelope MakeOn(Envelope copyEnvelope, double oT)
    {
        Envelope ret = copyEnvelope;
        if (oT > 0)
        {
            ret.IsOn = true;
            ret.timePressed = oT;
        }

        return ret;
    }
    
    public static Envelope MakeOff(Envelope copyEnvelope, double rT)
    {
        Envelope ret = copyEnvelope;
        if (rT > 0)
        {
            ret.IsOn = false;
            ret.timeReleased = rT;
        }

        return ret;
    }

    public double GetAmplitude(double t)
    {
        double returnAmp = 0;

        double lifetime = t - timePressed;
        //attack, gradient from start to attack
        if (lifetime <= Attack)
        {
            returnAmp = (lifetime / Attack) * StartAmp;
            //        Debug.Log("Attack");
            //       Debug.Log(returnAmp);

        }
        //decay, gradient from attack and to decay end 
        else if (lifetime <= Decay + Attack)
        {
            returnAmp = ((lifetime - Attack) / Decay) * (Amplitude - StartAmp) + StartAmp;

        }
        //sustain, constant amplitude
        else
        {
            //if key is still pressed just return the amplitude
            if (IsOn) returnAmp = Amplitude;
            //key has been released, calculate amplitude based on release
            else
            {
                double endTime = t - timeReleased;
                //0-1, 0 on key release, 1 on completely faded
                double r = endTime / Release;
                //if 0 amplitude stays the same, as r reaches 1 amplitude decreases
                returnAmp = Amplitude - Amplitude * r;
            }
        }
        if (returnAmp <= 0.001f)
        {
            returnAmp = 0;
        }
        return returnAmp;
    }
}


public class Oscillator : ASynthesizer
{
    [Range(0, 1)] public double noise_gain = 0.1f;
    public enum WaveType
    {
        TRIANGLE,
        SQUARE,
        SINE,
        SAW,
        NOISE_UNITY,
        NOISE_SYSTEM
    }
    private Dictionary<int, Envelope> m_activeNotes;
    [SerializeField] private Envelope m_Envelope;

    [SerializeField] private WaveType m_waveType = WaveType.SINE;
    [SerializeField] private double m_frequency = 440.0;
    [SerializeField] [Range(0, 1.0f)] float m_baseGain = 0.1f;
    [SerializeField] float m_LFOAmplitude = 0.01f;
    [SerializeField] float m_LFOFrequ = 0.1f;
    private double m_gain = 0.0;

    private double increment;
    private double m_phase;
    private double m_sampling_frequency = 48000.0;

    private bool playing = false;
    private float noiseRand = 0;
    //  private Random cSHarpRand;
    private Unity.Mathematics.Random m_Mrand;
    private System.Random m_Srand;
    private double m_Time;

    private Dictionary<int, double> m_noteReleaseBuffer;
    private void Awake()
    {
        m_noteReleaseBuffer = new Dictionary<int, double>();
        m_activeNotes = new Dictionary<int, Envelope>();
        m_phase = 0;
        m_Mrand = new Unity.Mathematics.Random(1);
        m_Srand = new System.Random();
        m_Envelope = new Envelope(0.8, 0.1, 1.0, 0.8, 0.2);
    }
    private void Update()
    {
        m_Time = Time.time;
        noiseRand = UnityEngine.Random.value;
        if (!playing) m_gain = 0;
    }
    public override void PlayNote(MidiNoteControl note, float velocity)
    {
        Debug.Log(AudioSettings.dspTime);
        Envelope env = Envelope.MakeOn(m_Envelope,AudioSettings.dspTime);
        
        //store new envelope for key if it already is stored, else store it 
        if (m_activeNotes.ContainsKey(note.noteNumber)) m_activeNotes[note.noteNumber] = env;
        else m_activeNotes.Add(note.noteNumber, env);
        playing = true;
        m_frequency = GetNoteFrequency(note.noteNumber);
        m_gain = m_baseGain;
    }
    public override void ReleaseNote(MidiNoteControl note)
    {
        m_noteReleaseBuffer.Add(note.noteNumber, AudioSettings.dspTime);
    }
    private double GetNoteFrequency(int index)
    {
        //a4 is at frequency 444.0 with key index 69
        int deltaA4 = index - 69;
        return 440.0 * math.pow(1.059463094359, deltaA4);
    }

    private double Osc(double frequency, double time, WaveType waveType, double LFOfreq = 0.0, double LFOAmp = 0.0)
    {

        double baseFreq = HzToVel(frequency) * time + LFOAmp * frequency * math.sin(HzToVel(LFOfreq) * time);
        switch (waveType)
        {
            case WaveType.SINE:
                {
                    return math.sin(baseFreq);
                }
            case WaveType.TRIANGLE:
                {
                    return math.asin(math.sin(baseFreq)) * 2.0 / math.PI_DBL;
                }
            case WaveType.SQUARE:
                {
                    if (math.sin(baseFreq) >= 0)
                        return 0.6f;
                    else
                        return -0.6f;
                }
            case WaveType.SAW:
                {
                    return (2.0 / math.PI_DBL) * (frequency * math.PI_DBL * math.fmod(time, 1.0 / frequency) - (math.PI_DBL / 2.0));
                }
            case WaveType.NOISE_UNITY:
                {
                    return (2.0 * m_Mrand.NextDouble() - 1.0) * noise_gain;
                }
            case WaveType.NOISE_SYSTEM:
                {
                    return (2.0 * m_Srand.NextDouble() - 1.0) * noise_gain;
                }
        }
        return 0;

    }
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (m_baseGain <= 0) return;
        //trigger all notes off that have been released
        foreach (var pair in m_noteReleaseBuffer)
        {
            m_activeNotes[pair.Key] = Envelope.MakeOff(m_activeNotes[pair.Key], pair.Value);
        }
        
        //clear buffer afterwards
        m_noteReleaseBuffer.Clear();

        foreach (var (noteID,noteData) in m_activeNotes)
        {
           // if(!noteData.IsOn) continue;

            //    Debug.Log("playing sound ");
            double frequency = GetNoteFrequency(noteID);
            increment = 1 / m_sampling_frequency;
            // m_phase = 0;

            for (int i = 0; i < data.Length; i++)
            {
                m_phase += increment;
                double amp = noteData.GetAmplitude(AudioSettings.dspTime);
                data[i] += (float)(m_gain * amp * Osc(frequency, m_phase, m_waveType, m_LFOFrequ, m_LFOAmplitude));
                if (m_phase > math.PI_DBL * 2) m_phase = 0;
            }
        }

    }

    private double HzToVel(double Hz) => 2.0 * math.PI_DBL * Hz;
    private float HzToVel(float Hz) => 2.0f * math.PI * Hz;

}
