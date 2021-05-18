using System;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using Extensions;
using Helper;

namespace DitzelGames.FastIK
{
    /// <summary>
    /// Fabrik IK Solver
    /// </summary>
    public class FastIKFabric : MonoBehaviour
    {
        [SerializeField] private int m_chainLength = 3;
        [SerializeField] private Transform m_target;

        [Header("Solver Parameters")]
        [SerializeField] private int m_iterations = 10;

        [SerializeField] private float m_threshold = 0.001f;
        
        private float m_snapBackStrength = 0;

        private float[] m_bonesLength;
        private float m_completeLength;


        class IKData
        {
            public Transform Bone { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 StartDirection { get;  set; }
            public Quaternion StartRotation { get; set; }
        }

        private IKData[] m_data;
        
        private Quaternion m_targetStartRotation;
        private Transform m_rootTransform;


        [SerializeField] private Vector3 m_projectionNormal;
        [SerializeField] private Vector3 m_rotationAxis;

        [SerializeField] private float m_upperBoundAngle1 = 150.0f;
        [SerializeField] private float m_lowerBoundAngle1 = -10.0f;
        [SerializeField] private float m_upperBoundAngle2 = 150.0f;
        [SerializeField] private float m_lowerBoundAngle2 = -10.0f;

        void Awake()
        {
            Init();
        }

        void Init()
        {
            //Normalize Projection Normal
            m_projectionNormal = m_projectionNormal.normalized;
            
            //Create array for bones
            m_bonesLength = new float[m_chainLength];
            
            //Create array of IKData
            m_data = new IKData[m_chainLength+1];
            m_data.Generate(() => new IKData());
            
            //find root
            m_rootTransform = transform.FindNthParent(m_chainLength);

            //init target
            if (!m_target)
            {
                throw new NullReferenceException(nameof(m_target) + " was null, this parameter is required!");
            }
            
            //Set the initial rotation of the Target 
            m_targetStartRotation = GetRotationRootSpace(m_target);
            
            //Reset the complete Length to 0
            m_completeLength = 0;

        
            var current = transform;
            
            //Initialize IK and Bone LengthData
            for (var i = m_data.End(); i >= 0; i--)
            {
                InitializeIKDataAtIndex(i, current);
                
                if (i < m_bonesLength.Length)
                {
                    m_completeLength += InitializeBoneLengthAtIndex(i);
                }
                
                current = current.parent;
            }
        }



        private void InitializeIKDataAtIndex(int index, Transform current)
        {
            var ikData = m_data[index];

            ikData.Bone = current;
            ikData.StartRotation = GetRotationRootSpace(current);
            
            var target = index == m_data.End() ? m_target : m_data[index + 1].Bone;
            ikData.StartDirection = GetPositionRootSpace(target) - GetPositionRootSpace(current);
        }
        
        private float InitializeBoneLengthAtIndex(int i)
        {
            m_bonesLength[i] = m_data[i].StartDirection.magnitude;
            return m_bonesLength[i];
        }
        
        void LateUpdate()
        {
            ResolveFABRIK(m_iterations);
        }
        private void ResolveFABRIK(int steps = 1)
        {
            //exit if no target was found
            if (m_target == null) return;
            //init if needed
            if (m_bonesLength.Length != m_chainLength) Init();

            //get positions
            for (int i = 0; i < m_data.Length; i++)
                m_data[i].Position = GetPositionRootSpace(m_data[i].Bone);
            //get target position and orientation
            Vector3 targetPosition = GetPositionRootSpace(m_target);
            Quaternion targetRotation = GetRotationRootSpace(m_target);


            //is the target possible to reach?
            if ((targetPosition - GetPositionRootSpace(m_data[0].Bone)).sqrMagnitude >= m_completeLength * m_completeLength)
            {
                //just strech it
                var direction = (targetPosition - m_data[0].Position).normalized;
                //set everything after root
                for (int i = 1; i < m_data.Length; i++)
                    m_data[i].Position = m_data[i-1].Position + direction * m_bonesLength[i - 1];
            }
            else
            {
                //pre caluclate the rotation axis and the world normal which are used in each iteration
                Vector3 dirToTarget = math.normalize(targetPosition - m_data[0].Position);
                Vector3 planeNormal = math.normalize(math.cross(dirToTarget, m_rootTransform.rotation * m_projectionNormal.normalized));
                Vector3 rotationAxis = m_rootTransform.rotation * math.normalize(m_rotationAxis);

                //reset position
                for (int i = 0; i < m_data.Length - 1; i++)
                    m_data[i+1].Position = Vector3.Lerp(m_data[i+1].Position, m_data[i].Position + m_data[i].StartDirection, m_snapBackStrength);


                //iterate through fabrik
                for (int iteration = 0; iteration < steps; iteration++)
                {
                    ForwardLoop(targetPosition, planeNormal);
                    BackwardsLoop(targetPosition, planeNormal, rotationAxis);

                    ////close enough to the target? => exit loop early
                    if ((m_data[m_data.Length - 1].Position - targetPosition).magnitude < m_threshold) break;
                }
            }
            SetNewPositions(targetRotation);
        }

        private void SetNewPositions(Quaternion targetRotation)
        {
            //set position & rotation of hinge objects
            for (int i = 0; i < m_data.Length; i++)
            {
                if (i == m_data.Length - 1)
                    SetRotationRootSpace(m_data[i].Bone, Quaternion.Inverse(targetRotation) * m_targetStartRotation * Quaternion.Inverse(m_data[i].StartRotation));
                else
                    SetRotationRootSpace(m_data[i].Bone, Quaternion.FromToRotation(m_data[i].StartDirection, m_data[i+1].Position - m_data[i].Position) * Quaternion.Inverse(m_data[i].StartRotation));
                SetPositionRootSpace(m_data[i].Bone, m_data[i].Position);
            }
        }
        private void ForwardLoop(Vector3 targetPosition, Vector3 projectionNormal)
        {
            //forward reaching iterate from the end effector (last index) to the second joint
            for (int i = m_data.Length - 1; i > 0; i--)
            {
                //check if current index is the end effector, end effectors position is set to the target position
                if (i == m_data.Length - 1)
                {
                    m_data[i].Position = targetPosition;
                }
                //handle other joints
                else
                {

                    Vector3 dir;
                    //handle second and first index joints, which are hinge joints
                    if (i == 1 || i == 2)
                    {
                        //hinge joints should only rotate around one axis, and their position is therefor limited to one plane,
                        //project the current point to limit the position
                        float3 planeNormal = projectionNormal;
                        Vector3 posProjected = Extensions.MathExtensions.ProjectOnPlane(m_data[i].Position, targetPosition, planeNormal);
                        //calculate new direction with the projected point
                        dir = math.normalize(m_data[i+1].Position - posProjected);
                    }
                    //for other joints the dir is simply the direction between current and last bone
                    else dir = m_data[i].Position - m_data[i+1].Position;
                    //set new position, previous point + direction scaled by the bone length
                    m_data[i].Position = (m_data[i+1].Position + dir * m_bonesLength[i]);

                }
            }
        }
        private void BackwardsLoop(Vector3 targetPosition, Vector3 projectionNormal, Vector3 rotationAxis)
        {
            for (int i = 1; i < m_data.Length; i++)
            {
                //handle second and first bone which are hinge joints
                //this time around we also limit the angle of the hinge joint rotation
                //doing this only in the backwards phase(&not in the forward phase) gives the best results
                if (i == 1 || i == 2)
                {
                    //get the normal
                    float3 planeNormal = projectionNormal;
                    //project the current point and the next point onto the plane with the normal
                    Vector3 PosProjected = Extensions.MathExtensions.ProjectOnPlane(m_data[i].Position, targetPosition, planeNormal);
                    Vector3 PosPlus1Projected = Extensions.MathExtensions.ProjectOnPlane(m_data[i+1].Position, targetPosition, planeNormal);


                    //get direction from previous point to current point and from current point to next point
                    Vector3 dir = math.normalize(PosProjected - m_data[i-1].Position);
                    Vector3 dir2 = math.normalize(PosPlus1Projected - PosProjected);
                    //get the angle between the two directions
                    float angle = MathExtensions.singedAngleDeg(dir, dir2, math.normalize(planeNormal));
                    //check if direction needs to be calcualted from restricted angles
                    //otherwise use backwardsProjected for dir
                    if (i == 1)
                    {
                        Vector3 newDir = RestrictRotation(angle, m_upperBoundAngle1, m_lowerBoundAngle1, dir, rotationAxis);
                        dir = math.normalize(newDir);
                    }
                    else if (i == 2)
                    {
                        Vector3 newDir = RestrictRotation(angle, m_upperBoundAngle2, m_lowerBoundAngle2, dir, rotationAxis);
                        dir = math.normalize(newDir);
                    }

                    m_data[i].Position = m_data[i-1].Position + dir.normalized * m_bonesLength[i - 1];
                }
                //set new position in traditional fabrik if no hinge joint
                else
                {
                    m_data[i].Position = m_data[i-1].Position + (m_data[i].Position - m_data[i-1].Position).normalized * m_bonesLength[i - 1];
                }
            }
        }

        /// <summary>
        /// root space functions
        /// </summary>
        private Vector3 GetPositionRootSpace(Transform current)
        {
            if (m_rootTransform == null)
                return current.position;
            else
                return Quaternion.Inverse(m_rootTransform.rotation) * (current.position - m_rootTransform.position);
        }
        private void SetPositionRootSpace(Transform current, Vector3 position)
        {
            if (m_rootTransform == null)
                current.position = position;
            else
                current.position = m_rootTransform.rotation * position + m_rootTransform.position;
        }
        private Quaternion GetRotationRootSpace(Transform current)
        {
            if (m_rootTransform == null)
                return current.rotation;
            else
                return Quaternion.Inverse(current.rotation) * m_rootTransform.rotation;
        }
        private void SetRotationRootSpace(Transform current, Quaternion rotation)
        {
            if (m_rootTransform == null)
                current.rotation = rotation;
            else
                current.rotation = m_rootTransform.rotation * rotation;
        }


        private static float3 RestrictRotation(float angle, float upperBound, float lowerBound, float3 inputVec, float3 rotationAxis)
        {
            //rotate clockwise
            if (angle > upperBound)
            {
                float3 newDir = Extensions.MathExtensions.RotateAboutAxisDeg(inputVec, angle - upperBound, rotationAxis);
                return math.normalize(newDir);
            }
            //rotate Counter Clockwise
            else if (angle < lowerBound)
            {
                float3 newDir = Extensions.MathExtensions.RotateAboutAxisDeg(inputVec, lowerBound - angle, rotationAxis);
                return math.normalize(newDir);
            }

            //no rotation was needed
            return inputVec;
        }
    }
}

