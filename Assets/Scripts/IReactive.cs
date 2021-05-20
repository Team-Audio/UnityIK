using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoteSystem;
public interface IReactive
{
    public void OnNotePlayed(NoteData note);
}
