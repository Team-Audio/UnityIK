using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public class MovementManager : MonoBehaviour
{
    [SerializeField] private List<Transform> m_leftHandTargets;
    [SerializeField] private List<Transform> m_rightHandTargets;

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
            c?.PressKey();
        }
        if (Input.GetKeyDown("c"))
        {
            TargetControler c = m_leftHandTargets[1].GetComponent<TargetControler>();
            c?.PressKey();
        }
        if (Input.GetKeyDown("v"))
        {
            TargetControler c = m_leftHandTargets[2].GetComponent<TargetControler>();
            c?.PressKey();
        }
        if (Input.GetKeyDown("b"))
        {
            TargetControler c = m_leftHandTargets[3].GetComponent<TargetControler>();
            c?.PressKey();
        }
        if (Input.GetKeyDown("n"))
        {
            TargetControler c = m_leftHandTargets[4].GetComponent<TargetControler>();
            c?.PressKey();
        }
        return;
        TestAnimation();
    }
    private void TestAnimation()
    {
        if (m_leftHandTargets.Count <= 0) return;
        Transform t = m_leftHandTargets[2];
        t.position += Vector3.up * m_speed * Time.deltaTime;

        if (math.abs(t.position.y - startHeight) > m_threshold) m_speed *= -1;
    }

    private void PressCurentKey()
    {
    }



}
