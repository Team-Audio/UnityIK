using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace NoteSystem
{

    public class NotePlayback : MonoBehaviour
    {
        //events for triggering for example piano sounds or vfx
        public UnityEvent<NoteData> NotePlayedDetailed;
        public UnityEvent<NoteData> NoteReleasedEventDetailed;
        public UnityEvent SongFinished;
        public UnityEvent SongStarted;

        [SerializeField] private bool m_triggerReleasedEvent = true;
        [SerializeField] private MovementManager m_movementManager;
        private List<NoteData> m_songData;

        //parameters for playing
        private bool m_playing = false;
        private float m_Time = 0;
        private int m_startingPosition = 0;
        private float m_threshold = 0.1f;

        void Start()
        {
            //make sure the events exists
            if (NotePlayedDetailed == null) NotePlayedDetailed = new UnityEvent<NoteData>();
            if (NoteReleasedEventDetailed == null) NoteReleasedEventDetailed = new UnityEvent<NoteData>();

        }

        public void DebugEvent()
        {
            Debug.Log("EVENT TRIGGERED!");
        }
        private void Update()
        {
            if (!m_playing) return;
            if (m_songData == null) return;
            if (m_songData.Count == 0) return;
            m_Time += Time.deltaTime;
            for (int i = m_startingPosition; i < m_songData.Count; i++)
            {
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
                    m_startingPosition = i;
                }

                //break loop if the current note was far enough off the current time, since the data should be sorted we can assume all following notes will not yet have to be played
                if (m_songData[i].TimeSinceStart > m_Time + m_threshold) break;
            }

            if (m_Time > m_songData[m_songData.Count - 1].TimeSinceStart) Stop();
        }

        private void PlayNote(NoteData note)
        {
            if (!m_movementManager) return;
            //trigger events
            NotePlayedDetailed.Invoke(note);
            if (m_triggerReleasedEvent) StartCoroutine(ReleaseNote(note));

            //trigger animation
            int index = m_songData.IndexOf(note);
            m_movementManager.PlayKey(note.KeyIndex, note.Duration, note.Velocity);
            m_movementManager.UpdateHandPosition(m_songData.GetRange(index, m_songData.Count - index - 1), m_Time);
        }

        public void Stop()
        {
            SongFinished.Invoke();
            m_playing = false;
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
            SongStarted.Invoke();
            m_playing = true;
            m_songData = NoteDataStore.Data;
            m_Time = 0;
            m_startingPosition = 0;
        }
        private IEnumerator ReleaseNote(NoteData note, float timeStep = 0.1f)
        {
            float counter = note.Duration;
            while (counter > 0)
            {
                counter -= timeStep;
                yield return new WaitForSeconds(timeStep);

            }
            NoteReleasedEventDetailed.Invoke(note);
        }
    }
}