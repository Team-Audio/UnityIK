using NoteSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BasicScaleEffect : MonoBehaviour, IReactive
{
    [SerializeField] private LeanTween.LeanTweenType m_tweenType = LeanTween.LeanTweenType.linear;
    [SerializeField] private int m_KeyThrehsold = 40;
    [SerializeField] private float m_duration = 2.0f;
    [SerializeField] private float m_startScale = 0.1f;
    [SerializeField] private float m_endScale = 2.0f;
    private const float m_timeThreshold = 0.05f;
    private float m_timeSinceLastTrigger = 0;
    public void OnNotePlayed(NoteData note)
    {
        if (note.KeyIndex > m_KeyThrehsold) return;
        if (m_timeSinceLastTrigger > m_timeThreshold)
        {

            this.transform.localScale = m_startScale.ToVec3();

            LeanTween.LeanTween.value(m_startScale, m_endScale, m_duration).setEase(m_tweenType).setOnUpdate(value => { transform.localScale = value.ToVec3(); });

            m_timeSinceLastTrigger = 0;
        }
    }

    void Start()
    {
        transform.localScale = m_endScale.ToVec3();
    }
    private void Update()
    {
        m_timeSinceLastTrigger += Time.deltaTime;
    }
}
