using NoteSystem;
using UnityEngine;


public class NoteSerializerProxy : MonoBehaviour
{
    public void Save() => NoteSerializer.SerializeNoteData();
    public void Load() => NoteSerializer.DeserializeNoteData();
}
