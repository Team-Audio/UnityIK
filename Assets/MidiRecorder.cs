using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class MidiRecorder : MonoBehaviour
{
    private Dictionary<int, List<NoteData>> m_data;
    private List<NoteData> m_sortedRecordData;

    private float m_recordingStartTime;
    private bool m_recording = false;
    public List<NoteData> GetData() => m_sortedRecordData;

    public void StartRecording()
    {
        Debug.Log("Start Recording");
        m_recordingStartTime = Time.realtimeSinceStartup;
        m_recording = true;
        m_data = new Dictionary<int, List<NoteData>>();
        for (int i = 0; i < 88; i++)
        {
            m_data.Add(i, new List<NoteData>());
        }
    }
    public void PlayKey(int KeyIndex, float velocity = 1.0f)
    {
        if (!m_recording) return;
        //   Debug.Log("Recorded Key");
        float TimeSinceStart = Time.realtimeSinceStartup - m_recordingStartTime;
        // Debug.Log(TimeSinceStart);

        NoteData newNote = new NoteData()
        {
            Duration = -1,
            TimeSinceStart = TimeSinceStart,
            Velocity = velocity,
            KeyIndex = KeyIndex,
            WasPlayed = false
        };
        List<NoteData> keyData = m_data[KeyIndex];
        keyData.Add(newNote);
    }
    public void ReleaseKey(int keyIndex)
    {
        if (!m_recording) return;
        //      Debug.Log("Recorded Key Release");

        List<NoteData> keyData = m_data[keyIndex];
        NoteData lastNote = keyData[keyData.Count - 1];
        lastNote.Duration = Time.realtimeSinceStartup - m_recordingStartTime - lastNote.TimeSinceStart;
        //Debug.Log(lastNote.Duration);
        m_data[keyIndex][keyData.Count - 1] = lastNote;

    }
    public void StopRecording()
    {
        if (!m_recording) return;
        Debug.Log("Stopped Recording");
        m_recording = false;

        ConvertRecordData();
    }

    //Converts the recorded data into the usable sorted dictionary
    private void ConvertRecordData()
    {
        m_sortedRecordData = new List<NoteData>();
        foreach (var pair in m_data)
        {
            foreach (NoteData note in pair.Value)
            {
                //  Debug.Log("adding note");
                //                Debug.Log(note.Duration);
                //skip notes with uninitialized duration
                if (note.Duration < 0) continue;
                m_sortedRecordData.Add(note);
            }
        }
        //sort the list based on when it was played with the unsorted data
        m_sortedRecordData.Sort((s1, s2) => s1.TimeSinceStart.CompareTo(s2.TimeSinceStart));
    }
}
//struct for information about what key and how the key is played
public struct NoteData
{
    public float Velocity;
    public float Duration;
    public float TimeSinceStart;
    public int KeyIndex;
    public bool WasPlayed;
}
