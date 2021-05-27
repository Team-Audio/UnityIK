using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Parallelizer : MonoBehaviour
{
    //the xsens recorded animation wrist position seems to have a slight tilt
    //this script aims to level the rotation to be parallel to the ground

    //rotation target value
    [SerializeField] private float m_targetEulerX = 260;
    //variable to activate/ deactivate 
    [SerializeField] private bool m_changeRotation = true;
    //variables for clamping height
    [SerializeField] private Vector3 m_threshold = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private bool m_clampPosition = true;

    private Vector3 m_previousPos = Vector3.zero;



    private void Start()
    {
        m_previousPos = transform.position;
    }

    //handle rotation in lateUpdate to overwrite the rotation changes made by the animator
    private void LateUpdate()
    {
        ClampRotation();
        ClampHeight();
    }
    private void ClampRotation()
    {
        if (!m_changeRotation) return;
        //get the world rotation, change x value and set new rotation
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        eulerRotation.x = m_targetEulerX;
        transform.rotation = Quaternion.Euler(eulerRotation);
    }
    private void ClampHeight()
    {
        if (!m_clampPosition) return;
        //get difference in all exis between current value and last update value
        float deltaY = Unity.Mathematics.math.abs(transform.position.y - m_previousPos.y);
        float deltaX = Unity.Mathematics.math.abs(transform.position.x - m_previousPos.x);
        float deltaZ = Unity.Mathematics.math.abs(transform.position.z - m_previousPos.z);

        Vector3 newPos = transform.position;
        //check if it does not exceeds a threshold
        if (deltaY < m_threshold.y)
        {
            //difference in height was too small to be significant, therefor set the height to the previously registered height
            newPos.y = m_previousPos.y;
            transform.position = newPos;
        }
        if (deltaX < m_threshold.x)
        {
            newPos.x = m_previousPos.x;
            transform.position = newPos;
        }
        if (deltaZ < m_threshold.z)
        {
            newPos.z = m_previousPos.z;
            transform.position = newPos;
        }

        //apply new position
        transform.position = newPos;

        //store current height
        m_previousPos = transform.position;
    }
}
