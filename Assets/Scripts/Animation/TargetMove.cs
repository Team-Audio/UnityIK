using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public class TargetMove : MonoBehaviour
{
    [SerializeField] private bool m_move = true;
    [SerializeField] private float m_range = 0.1f;
    [SerializeField] private Vector3 m_dir = new Vector3(0, 1, 0);
    [SerializeField] private float m_speed = 1.0f;

    private Vector3 m_startPos;
    
    void Start()
    {
        m_startPos = transform.position;
    }

    void Update()
    {
        if (!m_move) return;

        Vector3 pos = transform.position;
        //invert direction if far enough away from the start position
        if (math.length(pos - m_startPos) >= m_range) m_speed *= -1;
        //add direction scaled by speed
        pos += m_dir * (m_speed * Time.deltaTime);
        //Set new pos
        transform.position = pos;
    }
}
