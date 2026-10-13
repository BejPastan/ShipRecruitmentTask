using ShipRecruitmentTask.Wind;
using UnityEngine;
using VContainer;

namespace ShipRecruitmentTask.Ship
{
    //as all method will be private, this interface right now does not have any methods
    public interface IShipPhysics
    {

    }


    public class ShipPhysics : MonoBehaviour, IShipPhysics
    {
        [Inject]
        private readonly IWindController _windController;
        [Inject]
        private readonly Rigidbody _rb;
        [Inject]
        private readonly IShipDataContainer _shipData;

        [Header("Speed")]
        [SerializeField]
        float maxSpeed = 10f;

        [Header("Drag")]
        [Tooltip("Resists sideways (leeway) motion, like a keel.")]
        [SerializeField]
        float sideDragCoef = 2f;

        [Header("Turn")]
        [SerializeField]
        [Range(0f, 1f)]
        float turnCoef = 1;

        [Tooltip("Distance behind the center of mass where the rudder force is applied (yaw authority).")]
        [SerializeField]
        float sternOffset = 1f;

        //calculated on Start, from max wind speed and maxSpeed
        private float forwardDragCoef;

        private void Start()
        {
            if(_windController == null)
            {
                Debug.LogError($"{name} does not have wind controller attached");
            }
            if (_rb == null)
            {
                Debug.LogError($"{name} does not have rigidbody attached");
            }
            if (_shipData == null)
            {
                Debug.LogError($"{name} does not have ship manager attached");
            }

            forwardDragCoef = maxSpeed > 0.0001f
                ? (_windController.WindMaxForce * Time.fixedDeltaTime) / (maxSpeed)
                : 0f;
        }

        private void FixedUpdate()
        {
            ApplyWind();
            ApplySteering();
        }

        private void ApplyWind()
        {
            Vector3 localWind = _rb.transform.InverseTransformDirection(_windController.Wind);
            localWind.y = 0f;

            float windSpeed = localWind.magnitude;
            if (windSpeed <= 0.0001f) return;

            float sailRad = _shipData.SailRot * Mathf.Deg2Rad;
            Vector3 sailNormal = new Vector3(Mathf.Cos(sailRad), 0f, -Mathf.Sin(sailRad));

            float normalComponent = Mathf.Max(Vector3.Dot(localWind, sailNormal),0);//force of wind working forward

            Vector3 sailForce = sailNormal * (normalComponent * _shipData.SailForceCoef);

            Vector3 localVel = _rb.transform.InverseTransformDirection(_rb.linearVelocity);
            localVel.y = 0f;

            float forwardDrag = forwardDragCoef * localVel.x;
            float sideDrag = sideDragCoef * localVel.z;

            float netForward = sailForce.x - forwardDrag;
            float netLateral = sailForce.z - sideDrag;

            // Back to world space and apply at the center of mass.
            Vector3 netWorld = _rb.transform.TransformDirection(new Vector3(netForward, 0, netLateral));

            _rb.AddForce(netWorld, ForceMode.Impulse);
        }

        private void ApplySteering()
        {
            // Based on ShipManager's commanded turn angle and the ship's current speed.
            float turnAngleRad = _shipData.CurrentTurnAngle * Mathf.Deg2Rad;
            if (Mathf.Approximately(turnAngleRad, 0f)) return;

            float speed = _rb.linearVelocity.magnitude;
            if (speed <= 0.0001f) return; // can't steer without moving through the water

            float lateralForce = speed * Mathf.Sin(turnAngleRad) * turnCoef;

            Vector3 forward = _rb.transform.forward;
            Vector3 right = _rb.transform.right;

            // Point at the back of the ship so the force generates rotation
            Vector3 sternPoint = _rb.worldCenterOfMass - forward * sternOffset;

            //-right, to make steering right, move right
            _rb.AddForceAtPosition(-right * lateralForce, sternPoint, ForceMode.Force);
        }
    }
}

