using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
public class MovementManager : MonoBehaviour
{
    public float TestDuration = 1.0f;
    public int TestIndex = 1;
    [SerializeField] private PianoKeyManager m_pianoManager;

    [SerializeField] private List<Transform> m_leftHandTargets;
    [SerializeField] private List<Transform> m_rightHandTargets;
    [SerializeField] private Transform m_leftHand;
    [SerializeField] private Transform m_rightHand;
    [SerializeField] private float m_height;


    [SerializeField] private float m_speed = 1.0f;
    [SerializeField] private float m_threshold = 1.0f;

    private float startHeight = 0.0f;

    private void Start()
    {
        if (m_leftHandTargets.Count <= 0) return;
        Transform t = m_leftHandTargets[2];
        startHeight = t.position.y;

    }
    private void Update()
    {
        if (Input.GetKeyDown("x"))
        {
            TargetControler c = m_leftHandTargets[0].GetComponent<TargetControler>();
            //  c?.PressKey();
        }
        if (Input.GetKeyDown("c"))
        {
            TargetControler c = m_leftHandTargets[1].GetComponent<TargetControler>();
            //   c?.PressKey();
        }
        if (Input.GetKeyDown("v"))
        {
            TargetControler c = m_leftHandTargets[2].GetComponent<TargetControler>();
            //  c?.PressKey();
        }
        if (Input.GetKeyDown("b"))
        {
            TargetControler c = m_leftHandTargets[3].GetComponent<TargetControler>();
            //c?.PressKey();
        }
        if (Input.GetKeyDown("n"))
        {
            TargetControler c = m_leftHandTargets[4].GetComponent<TargetControler>();
            //  c?.PressKey();
        }

        if (Input.GetKeyDown("space"))
        {

            if (m_pianoManager == null) return;
            PlayKey(TestIndex);
        }

    }


    //handles all the key playing animation
    //should be called in an update loop that gets the key pressed data from the ML algorithm
    public void PlayKey(int keyIndex, float duration = 1.0f, float force = 1.0f, int fingerIndex = -1)
    {
        //get the key transform
        Transform KeyTransform = m_pianoManager.GetKey(keyIndex);
        //get the target to move
        Transform targetTransform = closestTarget(KeyTransform);
        //move target to key
        if (targetTransform == null) return;
        TargetControler tC = targetTransform.GetComponent<TargetControler>();

        Action rKAction = ReleaseKey;
        tC.PressKey(rKAction);
        //Call helper function to animate
        //get call back when target was reached
        //press key => move key and target
        KeyAnimator.PressKey(KeyTransform, TestDuration);

        //get call back on key released

        //release key
        // KeyAnimator.ReleaseKey(KeyTransform, TestDuration);
    }
    private void ReleaseKey()
    {
        Debug.Log("Releasing key!");
    }
    private Transform closestTarget(Transform key)
    {
        //Determine which hand is closest to the key
        //get the distance to the hand root transforms
        float distR = math.length(key.transform.position - m_rightHand.transform.position);
        float distL = math.length(key.transform.position - m_leftHand.transform.position);
        //pick the targets based on the distance to the hand
        List<Transform> targetList;
        if (distR > distL) targetList = m_leftHandTargets;
        else targetList = m_rightHandTargets;

        //find the target closest to the key
        float dist = float.MaxValue;
        int currentIndex = -1;
        for (int i = 0; i < targetList.Count; i++)
        {
            //get the distance and check if it is smaller than the so far smallest distance
            float currentDist = math.length(key.transform.position - targetList[i].transform.position);
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
