using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoteSystem
{
    public class MidiRecorder : MonoBehaviour
    {
        private Dictionary<int, List<NoteData>> m_data;
        private float m_recordingStartTime;
        private bool m_recording;
        public bool IsRecording => m_recording;
        public event Action OnStartRecording;
        public event Action OnStopRecording;
        
        public void StartRecording()
        {
            Debug.Log("<b>[Note Manager]</b> Start Recording");
            
            PrimeRecording();
            DeleteData();
            OnStartRecording?.Invoke();
        }

        private void PrimeRecording()
        {
            m_recordingStartTime = Time.unscaledTime;
            m_recording = true;
        }
        
        private void DeleteData()
        {
            m_data = new Dictionary<int, List<NoteData>>();
            for (int i = 0; i < 88; i++)
            {
                m_data[i] = new List<NoteData>();
            }
        }
        public void StopRecording()
        {
            Debug.Log("<b>[Note Manager]</b> Stopping Recording");
            
            if (IsRecording)
            {
                // Stop recording
                m_recording = false;
                
                // 
                NoteDataStore.Data = ConvertRecordData();
                OnStopRecording?.Invoke();
            }
        }

        //Converts the recorded data into the usable sorted dictionary
        private List<NoteData> ConvertRecordData()
        {
            List<NoteData> recordData = new List<NoteData>();
            foreach (var pair in m_data)
            {
                foreach (NoteData note in pair.Value)
                {
                    //skip notes with uninitialized duration
                    if (note.Duration < 0) continue;
                    recordData.Add(note);
                }
            }

            return recordData;
        }
        
        public void PressKey(int KeyIndex, float velocity = 1.0f)
        {
            //Early out
            if (!IsRecording) return;
            
            Debug.Log("<b>[Note Manager]</b> Key Pressed");
            
            NoteData newNote = new NoteData
            {
                Duration = -1,
                TimeSinceStart = Time.unscaledTime - m_recordingStartTime,
                Velocity = velocity,
                KeyIndex = KeyIndex,
            };
            
            m_data[KeyIndex].Add(newNote);
        }
        public void ReleaseKey(int keyIndex)
        {
            //Early out
            if (!IsRecording) return;
            
            Debug.Log("<b>[Note Manager]</b> Key Released");


            List<NoteData> keyData = m_data[keyIndex];
            NoteData lastNote = keyData[keyData.Count - 1];
            lastNote.Duration = Time.realtimeSinceStartup - m_recordingStartTime - lastNote.TimeSinceStart;
            m_data[keyIndex][keyData.Count - 1] = lastNote;

        }
    }

}