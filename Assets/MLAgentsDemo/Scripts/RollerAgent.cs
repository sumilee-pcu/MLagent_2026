using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MLAgent2026.Demo
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BehaviorParameters))]
    [RequireComponent(typeof(DecisionRequester))]
    public sealed class RollerAgent : Agent
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform area;
        [SerializeField] private float moveForce = 12f;
        [SerializeField] private float resetRange = 4f;

        private Rigidbody body;

        public override void Initialize()
        {
            ConfigureAgentComponents();
            body = GetComponent<Rigidbody>();
        }

        private void Reset()
        {
            ConfigureAgentComponents();
        }

        private void OnValidate()
        {
            ConfigureAgentComponents();
        }

        public override void OnEpisodeBegin()
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            transform.localPosition = RandomPosition();

            if (target != null)
            {
                target.localPosition = RandomPosition();
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            Vector3 agentPosition = transform.localPosition;
            Vector3 targetPosition = target != null ? target.localPosition : Vector3.zero;
            Vector3 velocity = body.linearVelocity;

            sensor.AddObservation(agentPosition.x / resetRange);
            sensor.AddObservation(agentPosition.z / resetRange);
            sensor.AddObservation(targetPosition.x / resetRange);
            sensor.AddObservation(targetPosition.z / resetRange);
            sensor.AddObservation(velocity.x / moveForce);
            sensor.AddObservation(velocity.z / moveForce);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            controlSignal.z = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
            body.AddForce(controlSignal * moveForce);

            AddReward(-0.001f);

            if (target == null)
            {
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);
            if (distanceToTarget < 1.25f)
            {
                AddReward(1f);
                EndEpisode();
            }

            if (transform.localPosition.y < -1f || Mathf.Abs(transform.localPosition.x) > resetRange + 2f || Mathf.Abs(transform.localPosition.z) > resetRange + 2f)
            {
                AddReward(-1f);
                EndEpisode();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                continuousActions[0] = 0f;
                continuousActions[1] = 0f;
                return;
            }

            float horizontal = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }

            float vertical = 0f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                vertical -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                vertical += 1f;
            }

            continuousActions[0] = horizontal;
            continuousActions[1] = vertical;
#else
            continuousActions[0] = Input.GetAxis("Horizontal");
            continuousActions[1] = Input.GetAxis("Vertical");
#endif
        }

        private Vector3 RandomPosition()
        {
            return new Vector3(Random.Range(-resetRange, resetRange), 0.5f, Random.Range(-resetRange, resetRange));
        }

        private void ConfigureAgentComponents()
        {
            if (TryGetComponent(out BehaviorParameters behavior))
            {
                behavior.BehaviorName = "RollerAgent";
                behavior.BehaviorType = BehaviorType.Default;
                behavior.BrainParameters.VectorObservationSize = 6;
                behavior.BrainParameters.NumStackedVectorObservations = 1;
                behavior.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(2);
            }

            if (TryGetComponent(out DecisionRequester requester))
            {
                requester.DecisionPeriod = 5;
                requester.TakeActionsBetweenDecisions = true;
            }
        }
    }
}
