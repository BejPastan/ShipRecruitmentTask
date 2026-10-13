using System;
using System.ComponentModel;
using Unity.Properties;
using UnityEngine;
using VContainer;

namespace ShipRecruitmentTask.Ship
{
    [CreateAssetMenu(fileName = "Ship", menuName ="Ship")]
    /// <summary>
    /// Orchestrates the ship's elements and implements the input-facing
    /// <see cref="IShipController"/> interface.
    /// </summary>
    /// 
    [Preserve]
    public class ShipManager : ScriptableObject, IShipController, INotifyPropertyChanged, IShipDataContainer
    {
        [SerializeField]
        float maxSailAngle = 45f;
        [SerializeField]
        float maxSailFlur = 100f;

        float sailRot = 0;
        float sailFlur = 0;

        [SerializeField]
        float sailSize = 1;

        [SerializeField]
        float sailRotationSpeed;
        [SerializeField]
        float sailFlurSpeed;
        [SerializeField]
        float turnAngle;

        float currentTurnAngle = 0f;

        [CreateProperty]
        public float SailRot
        {
            get
            {
                return sailRot;
            }
            private set
            {
                sailRot = Mathf.Max(Mathf.Min(value, maxSailAngle), -maxSailAngle);
            }
        }

        [CreateProperty]
        public float SailFlur
        {
            get
            {
                return sailFlur;
            }
            private set
            {
                sailFlur = Mathf.Max(Mathf.Min(value, maxSailFlur), 0);
            }
        }

        /// <summary>
        /// coefficante to scale force working on Sail
        /// </summary>
        public float SailForceCoef
        {
            get
            {
                return sailSize * sailFlur/ maxSailFlur;
            }
        }

        /// <summary>
        /// Current turn angle (turnAngle * direction, range -turnAngle..turnAngle).
        /// Read by ShipPhysics when steering is implemented.
        /// </summary>
        public float CurrentTurnAngle
        {
            get { return currentTurnAngle; }
        }

        /// <summary>Raised when the sail rotation changes; passes the new SailRot (degrees).</summary>
        public event Action<float> SailRotChanged;

        /// <summary>Raised when the sail furl changes; passes the new SailFlur.</summary>
        public event Action<float> SailFlurChanged;

        /// <summary>Raised when the commanded turn changes; passes the new CurrentTurnAngle (degrees).</summary>
        public event Action<float> TurnChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void ChangeSailRot(float change)
        {
            SailRot += change * sailRotationSpeed * Time.deltaTime;
            SailRotChanged?.Invoke(SailRot);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SailRot)));
        }

        public void ChangeSailFurl(float change)
        {
            SailFlur += change * sailFlurSpeed * Time.deltaTime;
            SailFlurChanged?.Invoke(SailFlur);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SailFlur)));

        }

        public void TurnShip(float change)
        {
            currentTurnAngle = turnAngle * change;
            TurnChanged?.Invoke(currentTurnAngle);
        }

        private void OnDestroy()
        {
            WhenDestroying?.Invoke();
        }

        /// <summary>
        /// event to call when this object is destoryed, to unregister callbacks etc
        /// </summary>
        public event Action WhenDestroying;
    }
}
