using UnityEngine;
using Unity.Mathematics;
using Extensions;
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
        private Transform[] m_bones;
        private Vector3[] m_positions;
        private Vector3[] m_startDirectionSucc;
        private Quaternion[] m_startRotationBone;
        private Quaternion m_startRotationTarget;
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
            m_projectionNormal = m_projectionNormal.normalized;
            //initial array
            m_bones = new Transform[m_chainLength + 1];
            m_positions = new Vector3[m_chainLength + 1];
            m_bonesLength = new float[m_chainLength];
            m_startDirectionSucc = new Vector3[m_chainLength + 1];
            m_startRotationBone = new Quaternion[m_chainLength + 1];

            //find root
            m_rootTransform = transform;
            for (var i = 0; i <= m_chainLength; i++)
            {
                if (m_rootTransform == null)
                    throw new UnityException("The chain value is longer than the ancestor chain!");
                m_rootTransform = m_rootTransform.parent;
            }

            //init target
            if (m_target == null)
            {
                m_target = new GameObject(gameObject.name + " Target").transform;
                SetPositionRootSpace(m_target, GetPositionRootSpace(transform));
            }
            m_startRotationTarget = GetRotationRootSpace(m_target);


            //init data
            var current = transform;
            m_completeLength = 0;
            for (var i = m_bones.Length - 1; i >= 0; i--)
            {
                m_bones[i] = current;
                m_startRotationBone[i] = GetRotationRootSpace(current);

                if (i == m_bones.Length - 1)
                {
                    //leaf
                    m_startDirectionSucc[i] = GetPositionRootSpace(m_target) - GetPositionRootSpace(current);
                }
                else
                {
                    //mid bone
                    m_startDirectionSucc[i] = GetPositionRootSpace(m_bones[i + 1]) - GetPositionRootSpace(current);
                    m_bonesLength[i] = m_startDirectionSucc[i].magnitude;
                    m_completeLength += m_bonesLength[i];
                }

                current = current.parent;
            }
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
            for (int i = 0; i < m_bones.Length; i++)
                m_positions[i] = GetPositionRootSpace(m_bones[i]);
            //get target position and orientation
            Vector3 targetPosition = GetPositionRootSpace(m_target);
            Quaternion targetRotation = GetRotationRootSpace(m_target);


            //is the target possible to reach?
            if ((targetPosition - GetPositionRootSpace(m_bones[0])).sqrMagnitude >= m_completeLength * m_completeLength)
            {
                //just strech it
                var direction = (targetPosition - m_positions[0]).normalized;
                //set everything after root
                for (int i = 1; i < m_positions.Length; i++)
                    m_positions[i] = m_positions[i - 1] + direction * m_bonesLength[i - 1];
            }
            else
            {
                //pre caluclate the rotation axis and the world normal which are used in each iteration
                Vector3 dirToTarget = math.normalize(targetPosition - m_positions[0]);
                Vector3 planeNormal = math.normalize(math.cross(dirToTarget, m_rootTransform.rotation * m_projectionNormal.normalized));
                Vector3 rotationAxis = m_rootTransform.rotation * math.normalize(m_rotationAxis);

                //reset position
                for (int i = 0; i < m_positions.Length - 1; i++)
                    m_positions[i + 1] = Vector3.Lerp(m_positions[i + 1], m_positions[i] + m_startDirectionSucc[i], m_snapBackStrength);


                //iterate through fabrik
                for (int iteration = 0; iteration < steps; iteration++)
                {
                    ForwardLoop(targetPosition, planeNormal);
                    BackwardsLoop(targetPosition, planeNormal, rotationAxis);

                    ////close enough to the target? => exit loop early
                    if ((m_positions[m_positions.Length - 1] - targetPosition).magnitude < m_threshold) break;
                }

                SetNewPositions(targetRotation);
            }

        }

        private void SetNewPositions(Quaternion targetRotation)
        {
            //set position & rotation of hinge objects
            for (int i = 0; i < m_positions.Length; i++)
            {
                if (i == m_positions.Length - 1)
                    SetRotationRootSpace(m_bones[i], Quaternion.Inverse(targetRotation) * m_startRotationTarget * Quaternion.Inverse(m_startRotationBone[i]));
                else
                    SetRotationRootSpace(m_bones[i], Quaternion.FromToRotation(m_startDirectionSucc[i], m_positions[i + 1] - m_positions[i]) * Quaternion.Inverse(m_startRotationBone[i]));
                SetPositionRootSpace(m_bones[i], m_positions[i]);
            }
        }
        private void ForwardLoop(Vector3 targetPosition, Vector3 projectionNormal)
        {
            //forward reaching iterate from the end effector (last index) to the second joint
            for (int i = m_positions.Length - 1; i > 0; i--)
            {
                //check if current index is the end effector, end effectors position is set to the target position
                if (i == m_positions.Length - 1)
                {
                    m_positions[i] = targetPosition;
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
                        Vector3 posProjected = Extensions.MathExtensions.ProjectOnPlane(m_positions[i], targetPosition, planeNormal);
                        //calculate new direction with the projected point
                        dir = math.normalize(m_positions[i + 1] - posProjected);
                    }
                    //for other joints the dir is simply the direction between current and last bone
                    else dir = m_positions[i] - m_positions[i + 1];
                    //set new position, previous point + direction scaled by the bone length
                    m_positions[i] = m_positions[i + 1] + dir * m_bonesLength[i];

                }
            }
        }
        private void BackwardsLoop(Vector3 targetPosition, Vector3 projectionNormal, Vector3 rotationAxis)
        {
            for (int i = 1; i < m_positions.Length; i++)
            {
                //handle second and first bone which are hinge joints
                //this time around we also limit the angle of the hinge joint rotation
                //doing this only in the backwards phase(&not in the forward phase) gives the best results
                if (i == 1 || i == 2)
                {
                    //get the normal
                    float3 planeNormal = projectionNormal;
                    //project the current point and the next point onto the plane with the normal
                    Vector3 PosProjected = Extensions.MathExtensions.ProjectOnPlane(m_positions[i], targetPosition, planeNormal);
                    Vector3 PosPlus1Projected = Extensions.MathExtensions.ProjectOnPlane(m_positions[i + 1], targetPosition, planeNormal);


                    //get direction from previous point to current point and from current point to next point
                    Vector3 dir = math.normalize(PosProjected - m_positions[i - 1]);
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

                    m_positions[i] = m_positions[i - 1] + dir.normalized * m_bonesLength[i - 1];
                }
                //set new position in traditional fabrik if no hinge joint
                else
                {
                    m_positions[i] = m_positions[i - 1] + (m_positions[i] - m_positions[i - 1]).normalized * m_bonesLength[i - 1];
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

