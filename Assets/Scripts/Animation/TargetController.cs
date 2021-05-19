using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanTween;
using Tween = LeanTween.LeanTween;
using System;
public class TargetController : MonoBehaviour
{
    public int FingerIndex;
    private float m_blackKeyHeight = 0.008f;
    private float m_startingHeight;
    [SerializeField] private float m_fingerHeight = 1.0f;

    private void Start()
    {
        m_startingHeight = transform.position.y;
    }
    
    //function handles playing of key, gets called by the finger movement manager
    public void PlayKey(Transform targetTransform, AnimationCurve curve, float duration = 1.0f,
                        float reachBackDuration = 0.05f, bool blackKey = false, float height = 1.0f)
    {
        //get the target position
        var transformPosition = transform.position;
        
        var pos = new Vector3(
            transformPosition.x + (blackKey ? 0.01f : 0),
            transformPosition.y + height,
            targetTransform.position.z
        );
        
        //separately animate on all axis
        Tween.moveY(gameObject, pos.y, reachBackDuration)
             .setEase(curve);
        
        Tween.moveZ(gameObject, pos.z, reachBackDuration)
             .setEase(LeanTweenType.easeInExpo);
        
        Tween.moveX(gameObject, pos.x, reachBackDuration)
             .setEase(LeanTweenType.easeInExpo)
             .setOnComplete(() => PressKey(duration - reachBackDuration, targetTransform, blackKey));
    }
    
    private void PressKey(float duration, Transform key, bool blackKey)
    {
        KeyAnimator.PressKey(key);
        float y = transform.position.y - m_fingerHeight;
        
        Tween.moveY(gameObject, y, duration)
             .setEase(LeanTweenType.easeOutExpo)
             .setOnComplete(() => ReleaseKey(key, blackKey));

    }
    
    private void ReleaseKey(Transform key, bool blackKey)
    {
        KeyAnimator.ReleaseKey(key);
        
        float y = m_startingHeight + (blackKey ? m_blackKeyHeight : 0);
        
        Tween.moveY(gameObject, y, 0.05f)
             .setEase(LeanTweenType.easeInExpo);
    }
}
