//struct for information about what key and how the key is played

using System;

namespace NoteSystem {
    [Serializable]
    public struct NoteData
    {
        public float Velocity;
        public float Duration;
        public float TimeSinceStart;
        public int KeyIndex;
        public bool WasPlayed;
    }
}