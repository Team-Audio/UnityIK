using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanTween;
public class TargetControler : MonoBehaviour
{
    [SerializeField] private float m_fingerHeight = 1.0f;
    [SerializeField] private LeanTweenType tweenType;

    [SerializeField] private AnimationCurve m_PlayCurve;

    private LTDescr descr;
    private Transform m_Trans;

    private void Start()
    {
        m_Trans = transform;
        descr = new LTDescr();
        descr.setEase(m_PlayCurve);
    }

    public void PressKey(float force = 1.0f, float duration = 1.0f, float pressSpeed = 0.1f)
    {
        Debug.Log("pressing key!");
        LeanTween.LeanTween.moveY(gameObject, m_Trans.position.y + m_fingerHeight, duration).setEase(m_PlayCurve);


    }


}
