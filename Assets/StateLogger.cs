using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class StateLogger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TextContainer;
    public void SetStateRecording()
    {
        SetText("Recording");
    }

    public void StartRecord()
    {
        SetText("Recording");
    }
    public void StartPlaying()
    {
        SetText("Playing");
    }
    public void Stop()
    {
        SetText("State");
    }
    public void Save()
    {
        SetText("saved recording");
    }
    public void Load()
    {
        SetText("loaded recording");
    }

    private void SetText(string message)
    {
        m_TextContainer?.SetText(message);
    }
}
