using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKActivator : MonoBehaviour
{

    private List<FastIKFabric> m_IKData;
    private void Start()
    {
        m_IKData = new List<FastIKFabric>();
        //find all taged IK objects
        var IKGameObjects = GameObject.FindGameObjectsWithTag("IK");
        //iterate objects and store the IK script
        foreach(GameObject currentObj in IKGameObjects)
        {
            FastIKFabric currentIK = currentObj.GetComponent<FastIKFabric>();
            if (currentIK == null) continue;
            m_IKData.Add(currentIK);
        }
    }
    public void ActivateAll()
    {
        foreach (FastIKFabric ik in m_IKData) ik.Activate();
    }
    public void DeactivateAll()
    {
        foreach (FastIKFabric ik in m_IKData) ik.Deactivate();
    }
}
