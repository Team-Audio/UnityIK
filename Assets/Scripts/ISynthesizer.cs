using Minis;
using UnityEngine;

public abstract class ASynthesizer : MonoBehaviour
{
    public abstract void PlayNote(MidiNoteControl note);
    public abstract void ReleaseNote(MidiNoteControl note);
}
