using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IKTrigger : MonoBehaviour
{

    [SerializeField] private List<FastIKFabric> m_IKSolver = new List<FastIKFabric>();
    [SerializeField] private bool m_ActiveFromStart = true;


    private bool m_active = false;

    private void Start()
    {
        if (m_ActiveFromStart) StartIK();
    }
    public void StartIK()
    {
        foreach (FastIKFabric solver in m_IKSolver) solver.Activate();
        m_active = true;
    } 
    public void StopIK()
    {
        foreach (FastIKFabric solver in m_IKSolver) solver.Deactivate();
        m_active = false;
    }
    void Update()
    {
        //for testing, if space is pressed, activate or deactivate IK based on c
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (m_active) StopIK();
            else StartIK();

        }
    }
}
