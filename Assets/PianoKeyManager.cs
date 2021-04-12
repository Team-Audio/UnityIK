using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeanTween;
public class PianoKeyManager : MonoBehaviour
{
    public List<Transform> TestOctave = new List<Transform>(13);
    public Transform GetKey(int i = 0)
    {
        if (i < 0 || i > TestOctave.Count)
        {
            return null;
        }
        else return TestOctave[i];
    }
}

public static class KeyAnimator
{
    private static float m_tweenSpeed = 0.1f;
    private static LeanTween.LeanTweenType m_PressKeyTweenType = LeanTweenType.easeOutExpo;
    private static LeanTween.LeanTweenType m_ReleaseKeyTweenType = LeanTweenType.easeInExpo;

    public static void PressKey(Transform key, float force = 1.0f)
    {
        if (key == null || force <= 0) return;

        Animator anim = key.GetComponent<Animator>();
        if (anim == null) return;
        float duration = m_tweenSpeed * force;
        //tween value and set blend shape Value
        LeanTween.LeanTween.value(anim.gameObject,0,1,duration).setEase(m_PressKeyTweenType).setOnUpdate((float value)=>
        {
            anim.SetFloat("Blend", value); 
        });
    }

    public static void ReleaseKey(Transform key, float force = 1.0f)
    {
        if (key == null || force <= 0) return;

        Animator anim = key.GetComponent<Animator>();
        if (anim == null) return;
        float duration = m_tweenSpeed * force;
        //tween value and set blend shape Value
        LeanTween.LeanTween.value(anim.gameObject, 1, 0, duration).setEase(m_ReleaseKeyTweenType).setOnUpdate((float value) =>
        {
            anim.SetFloat("Blend", value);
        });
    }
}
