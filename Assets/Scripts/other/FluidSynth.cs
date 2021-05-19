
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class FluidSynth
{
    private IntPtr synth_handle;
    
    private static class Import
    {
        public const string lib = "fluidsynth.dll";
    }

    [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr new_fluid_settings();

    [DllImport(Import.lib)]
    private static extern int fluid_settings_setnum(IntPtr settings, [MarshalAs(UnmanagedType.LPStr)] string name,
        double val);
    
    [DllImport(Import.lib)]
    private static extern int fluid_settings_setint(IntPtr settings, [MarshalAs(UnmanagedType.LPStr)] string name,
        int val);
    
    [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr new_fluid_synth(IntPtr settings);

    [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int fluid_synth_sfload(IntPtr synth, [MarshalAs(UnmanagedType.LPStr)] string filename,
        int update_midi_presets);
    
    [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int fluid_synth_program_select(IntPtr synth, int chan, int sfid, int bank,int preset);

    [DllImport(Import.lib,CallingConvention = CallingConvention.Cdecl)]
    private static extern int fluid_synth_noteon(IntPtr synth, int chan, int key,int vel);

    [DllImport(Import.lib,CallingConvention = CallingConvention.Cdecl)]
    private static extern int fluid_synth_noteoff(IntPtr synth, int chan, int key);

    [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int fluid_synth_write_float(IntPtr synth, int len, [In,Out] float[] lout, int loff, int lincr,
        [In,Out] float[] rout, int roff, int rincr);
    
    public FluidSynth(double gain = 0.2, double sampleRate = 44100, int channels = 256)
    {
        var st = new_fluid_settings();
        fluid_settings_setnum(st, "synth.gain", gain);
        fluid_settings_setnum(st, "synth.sample-rate", sampleRate);
        fluid_settings_setint(st, "synth.midi-channels", channels);

        synth_handle = new_fluid_synth(st);
    }

    public int SFLoad(string soundfont, int UpdateMidiPreset = 0)
    {
        return fluid_synth_sfload(synth_handle, soundfont, UpdateMidiPreset);
    }

    public int ProgramSelect(int channel, int soundfont, int bank, int preset)
    {
        return fluid_synth_program_select(synth_handle, channel, soundfont, bank, preset);
    }

    public bool NoteOn(int channel, int key, int velocity)
    {
        if (key < 0 || key > 128) return false;
        if (channel < 0) return false;
        if (velocity < 0 || velocity > 128) return false;

        return fluid_synth_noteon(synth_handle,channel,key,velocity) != 0;

    }

    public bool NoteOff(int channel, int key)
    {
        if (key < 0 || key > 128) return false;
        if (channel < 0) return false;

        return fluid_synth_noteoff(synth_handle,channel,key) != 0;
    }

    public (float[],float[]) GetSamples(int amount)
    {
        float[] left = new float[amount];
        float[] right = new float[amount];
        if(fluid_synth_write_float(synth_handle, amount, left, 0, 1, right, 0, 1) != 0)
            Debug.LogWarning("Unable to synthesize some tunes!");
        return (left,right);
    }

    public float[] GetSamplesInterleaved(int amount)
    {
        float[] samples = new float[amount*2];
        if(fluid_synth_write_float(synth_handle, amount, samples, 0, 2, samples, 1, 2) != 0)
            Debug.LogWarning("Unable to synthesize some tunes!");
        return samples;
    }
    
}
