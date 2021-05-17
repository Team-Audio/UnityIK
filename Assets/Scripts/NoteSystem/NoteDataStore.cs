using System.Collections.Generic;

namespace NoteSystem
{
    public static class NoteDataStore
    {
        private static List<NoteData> m_data = new List<NoteData>();

        private static void SetData(List<NoteData> data)
        {
            m_data = data;
            m_data.Sort((lhs, rhs) => lhs.TimeSinceStart.CompareTo(rhs.TimeSinceStart));
        }

        public static List<NoteData> Data
        {
            get => m_data;
            set => SetData(value);
        }
    }
}