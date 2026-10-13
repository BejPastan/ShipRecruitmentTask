using System;

namespace ShipRecruitmentTask.Ship
{
    /// <summary>
    /// Input-facing controls for a ship, fed by Player Input.
    /// </summary>
    public interface IShipController
    {
        void ChangeSailRot(float change);
        void ChangeSailFurl(float change);
        void TurnShip(float change);
    }

    public interface IShipDataContainer
    {
        public float SailRot { get; }
        public float SailFlur { get; }
        public float SailForceCoef { get; }
        public float CurrentTurnAngle { get; }

        /// <summary>Raised when the sail rotation changes; passes the new SailRot (degrees).</summary>
        public event Action<float> SailRotChanged;

        /// <summary>Raised when the sail furl changes; passes the new SailFlur.</summary>
        public event Action<float> SailFlurChanged;

        /// <summary>Raised when the commanded turn changes; passes the new CurrentTurnAngle (degrees).</summary>
        public event Action<float> TurnChanged;
    }
}
