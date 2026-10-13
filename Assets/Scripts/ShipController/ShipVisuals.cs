using System;
using UnityEngine;
using VContainer;

namespace ShipRecruitmentTask.Ship
{
    //as all method will be private, this interface right now does not have any methods
    public interface IShipVisuals
    {

    }

    /// <summary>
    /// Controls visuals of the ship. Rotates the sail to match the current sail rotation
    /// reported by <see cref="ShipManager"/> via its change events.
    /// </summary>
    public class ShipVisuals : MonoBehaviour, IShipVisuals
    {
        [Inject]
        IShipDataContainer _shipData;

        [Tooltip("The sail GameObject whose local Y rotation matches SailRot (degrees).")]
        [SerializeField]
        Transform sailTransform;

        [Tooltip("The rudder/tiller GameObject whose local Y rotation matches CurrentTurnAngle (degrees).")]
        [SerializeField]
        Transform rudderTransform;

        void OnEnable()
        {
            if (_shipData == null)
            {
                Debug.LogError($"{name} does not have ship manager attached");
            }

            _shipData.SailRotChanged += OnSailRotChanged;
            _shipData.SailFlurChanged += OnSailFlurChanged;
            _shipData.TurnChanged += OnTurnChanged;
        }

        void OnDisable()
        {
            _shipData.SailRotChanged -= OnSailRotChanged;
            _shipData.SailFlurChanged -= OnSailFlurChanged;
            _shipData.TurnChanged -= OnTurnChanged;
        }

        void Start()
        {
            // Apply the current state immediately so the visual matches at spawn.
            OnSailRotChanged(_shipData.SailRot);
            OnSailFlurChanged(_shipData.SailFlur);
            OnTurnChanged(_shipData.CurrentTurnAngle);
        }

        void OnSailRotChanged(float rot)
        {
            if (sailTransform != null)
                sailTransform.localRotation = Quaternion.Euler(0f, rot, 0f);
        }

        void OnSailFlurChanged(float furl)
        {
            //I don't have model for this
        }

        void OnTurnChanged(float angle)
        {
            if (rudderTransform != null)
                rudderTransform.localRotation = Quaternion.Euler(0f, -angle, 0f);
        }
    }
}
