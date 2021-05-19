using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeFluidSynth : MonoBehaviour
{
    private FluidSynth Synth;
    
    // Start is called before the first frame update
    void Start()
    {
        Synth = new FluidSynth();
        int soundfont = Synth.SFLoad("C:\\soundfonts\\default.sf2");
        Synth.ProgramSelect(0, soundfont, 0, 0);

        Synth.NoteOn(0, 69, 80);
        var( left,right) = Synth.GetSamples(44100);
        
        for (int i = 0; i < 44100; ++i)
        {
            if (i >= 0 && left.Length > i) Debug.Log(left[i]);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
