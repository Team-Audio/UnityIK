using Minis;
using UnityEngine;

public abstract class ASynthesizer : MonoBehaviour
{
    public abstract void PlayNote(MidiNoteControl note, float velocity);
    public abstract void ReleaseNote(MidiNoteControl note);
}
