using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanTween;
using Tween = LeanTween.LeanTween;
using Unity.Mathematics;

public class TargetController : MonoBehaviour
{
    public int FingerIndex;
    private float m_blackKeyHeight = 0.008f;
    private float m_startingHeight;
    [SerializeField] private float m_fingerHeight = 1.0f;
    private const float m_depth = 0.05f;
    private void Start()
    {
        m_startingHeight = transform.position.y;
    }
    //project the target point on the line from current point with direction lineDir
    private Vector3 GetTargetPoint(Vector3 OtherPos, Vector3 forward)
    {
        return OtherPos + forward * m_depth;
    }
    //function handles playing of key, gets called by the finger movement manager
    public void PlayKey(Transform targetTransform, AnimationCurve curve, Vector3 forward, float duration = 1.0f,
                        float reachBackDuration = 0.05f, bool blackKey = false, float height = 1.0f)
    {
        //get the target position
        Vector3 transformPosition = transform.localPosition;

        //get the position
        Vector3 pos = GetTargetPoint(targetTransform.position, forward);
        //add the height 
        pos.y += height;
        //move target position closer to the piano if the key is black
        if (blackKey) pos += forward * 0.01f;
        //new Vector3(transformPosition.x + (blackKey ? 0.01f : 0), transformPosition.y + height, targetTransform.position.z);
        //pos 
        //separately animate on all axis
        //Tween.moveLocalY(gameObject, pos.y, reachBackDuration)
        //         .setEase(curve);
        //Tween.moveLocalZ(gameObject, pos.z, reachBackDuration)
        //         .setEase(LeanTweenType.easeInExpo);

        Tween.move(gameObject, pos, reachBackDuration)
                 .setEase(LeanTweenType.easeInExpo)
                 .setOnComplete(() => PressKey(duration - reachBackDuration, targetTransform, blackKey));
    }

    private void PressKey(float duration, Transform key, bool blackKey)
    {
        KeyAnimator.PressKey(key);
        float y = transform.position.y - m_fingerHeight;

        Tween.moveLocalY(gameObject, y, duration)
             .setEase(LeanTweenType.easeOutExpo)
             .setOnComplete(() => ReleaseKey(key, blackKey));

    }

    private void ReleaseKey(Transform key, bool blackKey)
    {
        KeyAnimator.ReleaseKey(key);

        float y = m_startingHeight + (blackKey ? m_blackKeyHeight : 0);

        Tween.moveLocalY(gameObject, y, 0.05f)
             .setEase(LeanTweenType.easeInExpo);
    }
}
