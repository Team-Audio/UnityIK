using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using NoteSystem;

public struct PianoHistory
{
    public int keyIndex;
    public int FingerIndex;
    public bool lefthand;

}
public class MovementManager : MonoBehaviour
{
    public int BufferSize = 5;
    public float m_speed = 1.0f;
    [Header("----finger movement parameters----")]

    [SerializeField] private float m_reachBackDuration = 0.075f;
    [SerializeField] private float m_height = 0.015f;
    [SerializeField] private List<AnimationCurve> m_FingerForceCurves = new List<AnimationCurve>(3);

    [Header("----references----")]
    [SerializeField] private PianoKeyManager m_pianoManager;
    [SerializeField] private List<Transform> m_leftHandTargets;
    [SerializeField] private List<Transform> m_rightHandTargets;
    [SerializeField] private Transform m_leftHand;
    [SerializeField] private Transform m_rightHand;
    private float m_forceThreshold = 0.33f;


    private List<PianoHistory> m_history = new List<PianoHistory>();

    private void Start()
    {
        if (m_leftHandTargets.Count <= 0) return;
        Transform t = m_leftHandTargets[2];
        m_forceThreshold = 1.0f / (float)m_FingerForceCurves.Count;
        m_history = new List<PianoHistory>();
    }
    public void UpdateHandPosition(List<NoteData> pianoKeys, float t)
    {
        //get size of list if it is smaller than bufferSize
        int tempBufferSize = math.min(BufferSize, pianoKeys.Count);
        if (tempBufferSize <= 0) return;
        //get the average piano key of the next [buffersize] keys
        int averageKey = 0;
        for (int i = 0; i < tempBufferSize; i++)
        {
            averageKey += pianoKeys[i].KeyIndex;
        }
        averageKey = averageKey / tempBufferSize;
        Transform averageTrans = m_pianoManager.GetKey(averageKey);
        float timeToNextNote = pianoKeys[0].TimeSinceStart - t;
        LeanTween.LeanTween.moveZ(m_leftHand.gameObject, averageTrans.position.z, m_speed);
    }
    //handles all the key playing animation
    //should be called in an update loop that gets the key pressed data from the ML algorithm
    public void PlayKey(int keyIndex, float duration = 1.0f, float velocity = 0.001f, int fingerIndex = -1)
    {
        Debug.Log(keyIndex);
        //get the the transform of the piano key to animate
        Transform keyTransform = m_pianoManager.GetKey(keyIndex);

        if (!keyTransform)
        {
            Debug.LogError($"No Piano Key transform was found With index{keyIndex}");
            return;
        }
        //get the finger to move, for now we just chose the closest finger 
        Transform targetTransform = closestTarget(keyTransform);
        //move target to key
        if (!targetTransform)
        {
            Debug.LogError("No Finger transform to animate was found");
            return;
        }
        TargetController tC = targetTransform.GetComponent<TargetController>();
        if (!tC)
        {
            Debug.LogError("'TargetController' on finger to animate could not be found");
            return;
        }

        //determine if the key is black or white
        bool isBlack = keyTransform.CompareTag("Black");


        //figure outr which animation curve to pick for pressing the key
        AnimationCurve curve;
        //pick animation curve based on the passed in force

        // Debug.Log($"Velocity = {velocity} , threshold = {m_forceThreshold}");
        int force = Mathf.RoundToInt(velocity / m_forceThreshold);
        //   Debug.Log($"force = {force}");
        if (force > m_FingerForceCurves.Count || force < 0)
        {
            curve = m_FingerForceCurves[0];
        }
        else curve = m_FingerForceCurves[force];
        //actually play the key
        tC.PlayKey(keyTransform, curve, m_pianoManager.GetPianoForward(), duration, m_reachBackDuration, isBlack, m_height);
        m_history.Add(new PianoHistory() { keyIndex = keyIndex, FingerIndex = tC.FingerIndex });
    }


    //can be used to approximate the most likely finger to press the key based on the closest finger position to the key
    private Transform closestTarget(Transform key)
    {
        //Determine which hand is closest to the key
        //get the distance to the hand root transforms
        float distR = math.length(key.transform.position - m_rightHand.transform.position);
        float distL = math.length(key.transform.position - m_leftHand.transform.position);
        //pick the targets based on the distance to the hand
        List<Transform> targetList;
        //chose left or right hand
        if (distR > distL)
        {
            targetList = m_leftHandTargets;
        }
        else
        {
            targetList = m_rightHandTargets;
        }

        //find the target closest to the key
        float dist = float.MaxValue;
        int currentIndex = -1;
        for (int i = 0; i < targetList.Count; i++)
        {
            //get the distance and check if it is smaller than the so far smallest distance
            float currentDist = math.abs(key.transform.position.z - targetList[i].transform.position.z);
            if (currentDist < dist)
            {
                //set index as the current index and store the new smallest distance
                currentIndex = i;
                dist = currentDist;
            }
        }
        if (currentIndex < 0 || currentIndex > targetList.Count) return null;
        //return the closest transform found
        return targetList[currentIndex];
    }

}
