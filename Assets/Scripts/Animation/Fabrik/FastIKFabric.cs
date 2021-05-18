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
        [Header("Solver Setup")]
        [Tooltip("How many bones per finger")]
        [SerializeField] private int m_chainLength = 3;
        
        [Tooltip("The point that is targeted by the finger")]
        [SerializeField] private Transform m_target;

        [Header("Solver Parameters")]
        
        [Tooltip("Determines how many tries the solver will take before presenting the answer")]
        [SerializeField] private int m_iterations = 10;
        
        [Tooltip("Determines the accuracy threshold that is considered \"close enough\" to present as an answer")]
        [SerializeField] private float m_threshold = 0.001f;
        
        [Tooltip("")]
        [SerializeField] private Vector3 m_projectionNormal;
        
        [Tooltip("")]
        [SerializeField] private Vector3 m_rotationAxis;
        
        [Header("Bounds")]
        [SerializeField] private float m_upperBoundAngle1 = 150.0f;
        [SerializeField] private float m_lowerBoundAngle1 = -10.0f;
        [SerializeField] private float m_upperBoundAngle2 = 150.0f;
        [SerializeField] private float m_lowerBoundAngle2 = -10.0f;
        
        private float m_snapBackStrength = 0;
        private float m_completeLength;

        private struct IKData
        {
            public Transform Bone { get; private set; }
            public void SetBone(Transform bone)
            {
                Bone = bone;
            }
            public Vector3 Position { get; private set; }

            public void SetPosition(Vector3 position)
            {
                Position = position;
            }
            
            public Vector3 StartDirection { get; private set; }

            public void SetStartDirection(Vector3 startDirection)
            {
                StartDirection = startDirection;
            }
            
            public Quaternion StartRotation { get; private set; }

            public void SetStartRotation(Quaternion startRotation)
            {
                StartRotation = startRotation;
            }
            
            public static bool operator ==(IKData lhs, IKData rhs)
            {
                return lhs.Bone == rhs.Bone;
            }
            public static bool operator !=(IKData lhs, IKData rhs)
            {
                return lhs.Bone != rhs.Bone;
            }
        }

        private IKData[] m_data;
        
        
        private float[] m_bonesLength;
        private Quaternion m_startRotationTarget;
        private Transform m_rootTransform;


        void Awake()
        {
            Init();
        }

        void Init()
        {
            m_projectionNormal = m_projectionNormal.normalized;
            //initial array
            m_bonesLength = new float[m_chainLength];
            
            //create array of IKData initialized with new IKData with size chainLength +1
            m_data = Enumerable.Repeat(new IKData(), m_chainLength + 1).ToArray();


            FindRoot();
            InitTarget();
            

            
            IKData last = m_data[m_data.Length-1];
                                                      
            last.SetStartDirection(GetPositionRootSpace(m_target) - GetPositionRootSpace(transform));
            last.SetBone(transform);

            m_data[m_data.Length - 1] = last;
            
            //init data
            var current = transform.parent;
            m_completeLength = 0;
            for (var i = m_data.Length - 2; i >= 0; i--)
            {
                var data = m_data[i];
               
                data.SetBone(current);
                data.SetStartRotation(GetRotationRootSpace(current));
                
                //mid bone
                data.SetStartDirection(GetPositionRootSpace(m_data[i+1].Bone) - GetPositionRootSpace(current));

                m_data[i] = data;
                
                m_bonesLength[i] = data.StartDirection.magnitude;
                m_completeLength += m_bonesLength[i];
                current = current.parent;
            }
        }

        private void FindRoot()
        {
            //find root
            m_rootTransform = transform;
            for (var i = 0; i <= m_chainLength; i++)
            {
                if (!m_rootTransform)
                    throw new ArgumentException("The chain value is longer than the ancestor chain!");
                m_rootTransform = m_rootTransform.parent;
            }
        }

        private void InitTarget()
        {
            //init target
            if (!m_target)
            {
                m_target = new GameObject(gameObject.name + " Target").transform;
                SetPositionRootSpace(m_target, GetPositionRootSpace(transform));
            }

            m_startRotationTarget = GetRotationRootSpace(m_target);
        }

        void LateUpdate()
        {
            ResolveFABRIK(m_iterations);
        }
        private void ResolveFABRIK(int steps = 1)
        {
            //exit if no target was found
            if (!m_target) return;
            
            //init if needed
            if (m_bonesLength.Length != m_chainLength) Init();

            //get positions
            foreach (var data in m_data)
                data.SetPosition(GetPositionRootSpace(data.Bone));
       
            //get target position and orientation
            Vector3 targetPosition = GetPositionRootSpace(m_target);
            Quaternion targetRotation = GetRotationRootSpace(m_target);


            var first = m_data[0];
            var last = m_data[m_data.Length - 1];
            
            //is the target possible to reach?
            if ((targetPosition - GetPositionRootSpace(first.Bone)).sqrMagnitude >= m_completeLength * m_completeLength)
            {
                //just strech it
                var direction = (targetPosition - first.Position).normalized;
                //set everything after root
                for (int i = 1; i < m_data.Length; i++)
                    m_data[i].SetPosition(m_data[i - 1].Position + direction * m_bonesLength[i - 1]);
            }
            else
            {
                //pre calculate the rotation axis and the world normal which are used in each iteration
                Quaternion rootRotation = m_rootTransform.rotation;
                Vector3 dirToTarget = math.normalize(targetPosition - first.Position);
                Vector3 planeNormal = math.normalize(math.cross(dirToTarget, rootRotation * m_projectionNormal.normalized));
                Vector3 rotationAxis = rootRotation * math.normalize(m_rotationAxis);

                ResetPositions();


                //iterate through fabrik
                for (int iteration = 0; iteration < steps; ++iteration)
                {
                    ForwardLoop(targetPosition, planeNormal);
                    BackwardLoop(targetPosition, planeNormal, rotationAxis);
                    
                    Vector3 directionVec = last.Position - targetPosition;
                    
                    //close enough to the target? => exit loop early
                    if (directionVec.magnitude < m_threshold) break;
                }

                SetNewPositions(targetRotation);
            }
        }

        private void ResetPositions()
        {
            //reset position
            for (int i = 0; i < m_data.Length - 1; i++)
            {
                var current = m_data[i];
                m_data[i + 1].SetPosition(Vector3.Lerp(m_data[i + 1].Position, current.Position + current.StartDirection, m_snapBackStrength));
            }
        }

        private void SetNewPositions(Quaternion targetRotation)
        {
            //set position & rotation of hinge objects
            for (int i = 0; i < m_data.Length - 1; i++)
            {
                SetJointPosition(ref m_data[i],m_data[i+1].Position);
            }

            var last = m_data[m_data.Length - 1];
            
            var inverseRotation = last.StartRotation.Inverse();
            
            SetRotationRootSpace(last.Bone, targetRotation.Inverse() * m_startRotationTarget * inverseRotation);
            SetPositionRootSpace(last.Bone, last.Position);

            m_data[m_data.Length - 1] = last;

        }

        private void SetJointPosition(ref IKData data, Vector3 nextPosition)
        {
            // get the rotation towards the next joint
            var scaledDirection = nextPosition - data.Position;
            var rotationToNextJoint = Quaternion.FromToRotation(data.StartDirection, scaledDirection);
            
            // get the inverse rotation of the original rotation
            var inverseRotation = data.StartRotation.Inverse();
        
            // Set the translated rotation
            SetRotationRootSpace(data.Bone, rotationToNextJoint * inverseRotation);
            SetPositionRootSpace(data.Bone, data.Position);
        }

        private void ForwardLoop(Vector3 targetPosition, Vector3 projectionNormal)
        {
            //forward reaching iterate from the end effector (last index) to the second joint
            for (int i = m_data.Length - 1; i > 0; i--)
            {
                //check if current index is the end effector, end effectors position is set to the target position
                if (i == m_data.Length - 1)
                {
                    m_data[i].SetPosition(targetPosition);
                }
                //handle other joints
                else
                {
                    var currentPosition = m_data[i].Position;
                    var nextPosition = m_data[i + 1].Position;
                    
                    Vector3 dir;
                    //handle second and first index joints, which are hinge joints
                    if (i == 1 || i == 2)
                    {
                        //hinge joints should only rotate around one axis, and their position is therefor limited to one plane,
                        //project the current point to limit the position
                        float3 planeNormal = projectionNormal;
                        Vector3 posProjected = MathExtensions.ProjectOnPlane(currentPosition, targetPosition, planeNormal);
                        //calculate new direction with the projected point
                        dir = math.normalize(nextPosition - posProjected);
                    }
                    //for other joints the dir is simply the direction between current and last bone
                    else dir = currentPosition - nextPosition;
                    //set new position, previous point + direction scaled by the bone length
                    m_data[i].SetPosition(nextPosition + dir * m_bonesLength[i]);
                }
            }
        }
        private void BackwardLoop(Vector3 targetPosition, Vector3 projectionNormal, Vector3 rotationAxis)
        {
            for (int i = 1; i < m_data.Length; i++)
            {
                
                var currentPosition = m_data[i].Position;
                var prevPosition = m_data[i - 1].Position;
                
                //handle second and first bone which are hinge joints
                //this time around we also limit the angle of the hinge joint rotation
                //doing this only in the backwards phase(&not in the forward phase) gives the best results
                if (i == 1 || i == 2)
                {
                    var nextPosition = m_data[i + 1].Position;
                    //get the normal
                    float3 planeNormal = projectionNormal;
                    //project the current point and the next point onto the plane with the normal
                    Vector3 PosProjected = MathExtensions.ProjectOnPlane(currentPosition, targetPosition, planeNormal);
                    Vector3 PosPlus1Projected = MathExtensions.ProjectOnPlane(nextPosition, targetPosition, planeNormal);


                    //get direction from previous point to current point and from current point to next point
                    Vector3 dir = math.normalize(PosProjected - prevPosition);
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
                    m_data[i].SetPosition(prevPosition + dir.normalized * m_bonesLength[i - 1]);
                }
                //set new position in traditional fabrik if no hinge joint
                else
                {
                    m_data[i].SetPosition(prevPosition + (currentPosition - prevPosition).normalized * m_bonesLength[i - 1]);
                }
            }
        }

        /// <summary>
        /// root space functions
        /// </summary>
        private Vector3 GetPositionRootSpace(Transform current)
        {
            if(current == null) Debug.LogError("<b> BUG HERE </b>");
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
                float3 newDir = MathExtensions.RotateAboutAxisDeg(inputVec, angle - upperBound, rotationAxis);
                return math.normalize(newDir);
            }
            
            //rotate counter-clockwise
            if (angle < lowerBound)
            {
                float3 newDir = MathExtensions.RotateAboutAxisDeg(inputVec, lowerBound - angle, rotationAxis);
                return math.normalize(newDir);
            }

            //no rotation was needed
            return inputVec;
        }
    }
}

