using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanTween;
using System;
public class TargetControler : MonoBehaviour
{
    public int FingerIndex = 0;
    private float m_blackKeyHeight = 0.008f;
    private float m_startingHeight = 0;
    [SerializeField] private float m_fingerHeight = 1.0f;
    [SerializeField] private LeanTweenType tweenType;
    private float m_duration = 0.1f;
    //[SerializeField] private AnimationCurve m_PlayCurve;

    private Transform m_Trans;

    private void Start()
    {
        m_startingHeight = this.transform.position.y;
        m_Trans = transform;
    }
    //function handles playing of key, gets called by the finger movement manager
    public void PlayKey(Transform targetTransform, AnimationCurve curve, float duration = 1.0f, float reachBackDuration = 0.05f, bool blackKey = false, float height = 1.0f)
    {
        //get the target position

        Vector3 pos = new Vector3(transform.position.x, transform.position.y, targetTransform.position.z);
        if (blackKey) pos.x += 0.01f;
        pos.y += height;


        //seperately animate on all axis
        LeanTween.LeanTween.moveY(gameObject, pos.y, reachBackDuration).setEase(curve);
        LeanTween.LeanTween.moveZ(gameObject, pos.z, reachBackDuration).setEase(LeanTweenType.easeInExpo);
        LeanTween.LeanTween.moveX(gameObject, pos.x, reachBackDuration).setEase(LeanTweenType.easeInExpo).setOnComplete(() => PressKey(duration - reachBackDuration, targetTransform, blackKey));
    }
    private void PressKey(float duration, Transform key, bool blackKey)
    {
        KeyAnimator.PressKey(key);

        float y = transform.position.y - m_fingerHeight;
        //   if (blackKey) y += m_blackKeyHeight;

        LeanTween.LeanTween.moveY(gameObject, y, duration).setEase(LeanTweenType.easeOutExpo).setOnComplete(() => ReleaseKey(duration, key, blackKey));

    }
    //releases the key, finger is moved back up, also triggers the key to move back up
    private void ReleaseKey(float duration, Transform key, bool blackKey)
    {
        KeyAnimator.ReleaseKey(key, 1.0f);
        float y = m_startingHeight;
        if (blackKey) y += m_blackKeyHeight;
        //ReleaseKey.Invoke(key);
        LeanTween.LeanTween.moveY(gameObject, y, 0.05f).setEase(LeanTweenType.easeInExpo);
        //  LeanTween.LeanTween.moveY(gameObject, m_Trans.position.y + m_fingerHeight, duration).setEase(m_PlayCurve).setOnComplete(() => callback?.Invoke(key));
    }
}
