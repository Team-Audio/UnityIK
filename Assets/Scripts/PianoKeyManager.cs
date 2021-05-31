using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanTween;
using Unity.Mathematics;
public class PianoKeyManager : MonoBehaviour
{
    [Header("--Transforms to determine finger position along the piano--")]
    [SerializeField] private Transform m_pianoBeginning;
    [SerializeField] private Transform m_pianoEnd;


    [Header("--Transforms to acces the piano and its keys--")]
    [SerializeField] private Transform m_pianoTransform;
    [SerializeField] private Transform m_rootTransform;
    [SerializeField] private List<Transform> m_keysList = new List<Transform>(13);
    private void Update()
    {
        Debug.DrawRay(m_pianoTransform.position, m_pianoTransform.forward);
    }
    public void Awake()
    {
        FindKeys();
    }
    public Vector3 GetPianoForward()
    {
        return m_pianoTransform.forward;
    }
    public Vector3 GetPianoRight()
    {
        //Check if transforms are setup
        if (!m_pianoEnd || !m_pianoBeginning) return Vector3.zero;
        //return normalized direction vector from beginning to end of the piano
        return (m_pianoEnd.position - m_pianoBeginning.position);
    }
    public Vector3 GetPianoBeginningPos() => m_pianoBeginning.position;
    public Vector3 ProjectPointAlongPiano(Vector3 A)
    {
        Vector3 B = m_pianoBeginning.position;
        //translate input point onto same height as the piano keys
        A.y = B.y;
        //get direction vectors
        Vector3 dirAB = (A - B);
        Vector3 lineDir = GetPianoRight();
        //Calculate the dot product, between the direction vector and the vector of the line to project onto
        float dotProduct = math.dot(dirAB, lineDir);
        //get the squared length of the line
        float len2 = math.lengthsq(lineDir);
        //scale the line dir based on the calculated dot product
        Vector3 p = B + lineDir * dotProduct / len2;
        return p;
      
    }
    private void FindKeys()
    {
        //Debug.Log("finding keys");
        m_keysList = new List<Transform>(88);
        for (int i = 0; i < 88; i++) m_keysList.Add(null);


        foreach (Transform currentTransform in m_rootTransform)
        {
            //handle octaves
            if (currentTransform.CompareTag("Octave"))
            {
                //calculate the base index of the first octave key
                int baseIndex = OctNameToInt(currentTransform.name) * 12 + 3;
                foreach (Transform keyTransform in currentTransform)
                {
                    int keyIndex = KeyNameToInt(keyTransform.name);
                    int listIndex = baseIndex + keyIndex;
                    m_keysList[listIndex] = keyTransform;
                }

            }
            else
            {
                //handle keys outside of main octaves
                if (currentTransform.name == "SinB1") m_keysList[1] = currentTransform;
                else if (currentTransform.name == "SinW1") m_keysList[0] = currentTransform;
                else if (currentTransform.name == "SinW2") m_keysList[2] = currentTransform;
                else if (currentTransform.name == "SinW3") m_keysList[87] = currentTransform;
            }
        }
    }
    private int KeyNameToInt(string name)
    {
        name = name.Remove(0, 4);
        //match the name of the key to the according index in the current octave
        if (name == "W1") return 0;
        if (name == "B1") return 1;
        if (name == "W2") return 2;
        if (name == "B2") return 3;
        if (name == "W3") return 4;
        if (name == "W4") return 5;
        if (name == "B3") return 6;
        if (name == "W5") return 7;
        if (name == "B4") return 8;
        if (name == "W6") return 9;
        if (name == "B5") return 10;
        if (name == "W7") return 11;
        return -1;
    }
    private int OctNameToInt(string name)
    {
        //match the octave index name to the index starting at 0
        if (name == "Oct1") return 0;
        if (name == "Oct2") return 1;
        if (name == "Oct3") return 2;
        if (name == "Oct4") return 3;
        if (name == "Oct5") return 4;
        if (name == "Oct6") return 5;
        if (name == "Oct7") return 6;
        return -1;

    }
    public Transform GetKey(int i)
    {
        // Debug.Log("getting key: " + i);
        if (i < 0 || i > m_keysList.Count)
        {
            return null;
        }
        else
        {
            //m_keysList[i].gameObject.SetActive(false);
            return m_keysList[i];
        }
    }
}

public static class KeyAnimator
{
    private static float m_tweenSpeed = 0.1f;
    private static LeanTween.LeanTweenType m_PressKeyTweenType = LeanTweenType.easeOutExpo;
    private static LeanTween.LeanTweenType m_ReleaseKeyTweenType = LeanTweenType.easeInExpo;

    public static void PressKey(Transform key, float force = 1.0f)
    {
        if (key == null) return;

        Animator anim = key.GetComponent<Animator>();
        if (anim == null) return;
        float duration = m_tweenSpeed * force;
        key.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);
        //tween value and set blend shape Value
        LeanTween.LeanTween.value(anim.gameObject, 0, 1, duration).setEase(m_PressKeyTweenType).setOnUpdate((float value) =>
           {
               anim.SetFloat("Blend", value);
           });
    }

    public static void ReleaseKey(Transform key, float force = 1.0f)
    {
        if (key == null) return;

        Animator anim = key.GetComponent<Animator>();
        if (anim == null) return;
        float duration = m_tweenSpeed * force;
        //tween value and set blend shape Value
        LeanTween.LeanTween.value(anim.gameObject, 1, 0, duration).setEase(m_ReleaseKeyTweenType).setOnUpdate((float value) =>
        {
            anim.SetFloat("Blend", value);
        });
        if (key.CompareTag("White")) key.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
        else key.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.black);
    }
}
