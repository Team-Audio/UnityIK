using System.Collections.Generic;
using UnityEngine;


namespace NoteSystem
{

    [RequireComponent(typeof(MidiRecorder))]
    public class NotePlayback : MonoBehaviour
    {
        [SerializeField] private MovementManager m_movementManager;
        public List<NoteData> m_songData;
        private MidiRecorder m_recorder;
        private bool m_playing = false;

        private float m_Time = 0;
        private int startingPosition = 0;
        private float threshold = 0.1f;

        void Start()
        {
            m_recorder = GetComponent<MidiRecorder>();
        }

        private void Update()
        {
            if (!m_playing) return;
            Debug.Log(m_songData.Count);
            m_Time += Time.deltaTime;
            for (int i = startingPosition; i < m_songData.Count; i++)
            {

                Debug.Log(m_songData[i].TimeSinceStart);
                if (m_songData[i].WasPlayed) continue;

                if (m_songData[i].TimeSinceStart < m_Time)
                {
                    var note = m_songData[i];
                    note.WasPlayed = true;
                    //write back that the note was played
                    m_songData[i] = note;
                    //trigger this after it has been written back to the list
                    PlayNote(note);

                    //store the index as a starting index so that we can skip ahead for the next update loop
                    startingPosition = i;
                }

                //break loop if the current note was far enough off the current time, since the data should be sorted we can assume all following notes will not yet have to be played
                if (m_songData[i].TimeSinceStart > m_Time + threshold) break;
            }

            if (m_Time > m_songData[m_songData.Count - 1].TimeSinceStart) Stop();
        }

        private void PlayNote(NoteData note)
        {
            if (!m_movementManager) return;

            int index = m_songData.IndexOf(note);
            m_movementManager.PlayKey(note.KeyIndex, note.Duration, note.Velocity);
            m_movementManager.UpdateHandPosition(m_songData.GetRange(index, m_songData.Count - index - 1), m_Time);
        }

        public void Stop()
        {
            m_playing = false;
            Debug.Log("stoped Playing");
            //return if no song was initialized yet
            if (m_songData == null) return;
            for (int i = 0; i < m_songData.Count; i++)
            {
                var data = m_songData[i];
                data.WasPlayed = false;
                m_songData[i] = data;
            }
        }

        public void Play()
        {
            Debug.Log("Started Playing");
            m_playing = true;
            m_songData = NoteDataStore.Data;
            m_Time = 0;
            startingPosition = 0;
        }
    }
}