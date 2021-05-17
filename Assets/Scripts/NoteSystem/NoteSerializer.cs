using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace NoteSystem
{
    public static class NoteSerializer
    {
        public static string FileName = @"note-data.csv";
        public static void SerializeNoteData()
        {
            string repr = MakeHeader();
            foreach (NoteData noteData in NoteDataStore.Data)
            {
                repr += SerializeSingleRow(noteData) + EndRow();
            }
            
            using var file = new StreamWriter(Path.Combine(Application.streamingAssetsPath, FileName));
            file.Write(repr);
        }

        private static string MakeHeader() => "sep=,\n";

        private static string SerializeSingleRow(NoteData d)
        {
            var cult = CultureInfo.InvariantCulture;
            return String.Join(",",
                d.Duration.ToString(cult),
                d.Velocity.ToString(cult),
                d.KeyIndex.ToString(),
                d.TimeSinceStart.ToString(cult)
            );
        }

        private static string EndRow() => "\n";


        public static void DeserializeNoteData()
        {
            List<NoteData> data = new List<NoteData>();
            bool headerDiscarded = false;


            using var file = new StreamReader(Path.Combine(Application.streamingAssetsPath, FileName));

            for (string line = file.ReadLine(); line != null; line = file.ReadLine())
            {
                //Check if a header exists that needs to be discarded
                if (DiscardHeader(line, ref headerDiscarded)) continue;
                
                // try to split the line into 4 values
                if (ExtractValues(line, out var values))
                {
                    // try to deserialize the split values
                    if (DeserializeSingleRow(values, out var duration,
                        out var velocity, out var keyIndex, out var timedelta))
                    {
                        //create new dataset from that data
                        data.Add(new NoteData{
                            Duration = duration, KeyIndex = keyIndex,
                            TimeSinceStart = timedelta, Velocity = velocity
                        });
                        
                    }
                    else Debug.LogWarning("syntax error in note data: \"{line}\" ");
                } 
                else  Debug.LogWarning($"insufficient data in note data: \"{line}\"");
            }

            NoteDataStore.Data = data;

        }



        private static bool DiscardHeader(string line, ref bool headerDiscarded)
        {
            //check if the first line starts with the sep=, header and discard it if it does
            if (!headerDiscarded)
            {
                headerDiscarded = true;
                if (line.StartsWith("sep=,")) return true;
            }
            return false;
        }



        private static bool ExtractValues(string line, out string[] values)
        {
            values = line.Split(',');
            if (values.Length != 4)
            {
                return false;
            }

            return true;
        }

        private static bool DeserializeSingleRow(string[] values, out float duration, out float velocity,
            out int keyIndex, out float timedelta)
        {
            var cult = CultureInfo.InvariantCulture;
            var style = NumberStyles.Any;
            return float.TryParse(values[0],style,cult, out duration) &
                   float.TryParse(values[1],style,cult, out velocity) &
                   int.  TryParse(values[2],style,cult, out keyIndex) &
                   float.TryParse(values[3],style,cult, out timedelta);
        }
    }
}