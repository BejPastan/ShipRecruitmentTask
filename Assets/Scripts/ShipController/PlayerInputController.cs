using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;


namespace ShipRecruitmentTask.Ship
{
    /// <summary>
    /// Reads player input from the generated <see cref="ShipInput"/> asset and forwards it to an
    /// <see cref="IShipController"/>. Sail rotation and furling are "hold-to-act" coroutines; ship
    /// turning sets the turn value directly (-1..1) and returns to 0 when released.
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        [Inject]
        private readonly IShipController _ship;
        ShipInput _input;

        // Running coroutine handles so each action can be stopped independently.
        Coroutine _sailRotRoutine;
        Coroutine _sailFurlRoutine;


        void Start()
        {
            Debug.Log("Player Input start");
            _input = new ShipInput();
            var shipControl = _input.ShipControl;

            shipControl.SailRotating.started  += OnSailRotatingStarted;
            shipControl.SailRotating.canceled += OnSailRotatingCanceled;

            shipControl.SailFluring.started   += OnSailFurlingStarted;
            shipControl.SailFluring.canceled  += OnSailFurlingCanceled;

            shipControl.ShipTurning.started   += OnShipTurning;
            shipControl.ShipTurning.performed += OnShipTurning;
            shipControl.ShipTurning.canceled  += OnShipTurningCanceled;

            _input.Enable();
        }

        void OnDestroy()
        {
            if (_input == null) return;

            var shipControl = _input.ShipControl;

            shipControl.SailRotating.started  -= OnSailRotatingStarted;
            shipControl.SailRotating.canceled -= OnSailRotatingCanceled;

            shipControl.SailFluring.started   -= OnSailFurlingStarted;
            shipControl.SailFluring.canceled  -= OnSailFurlingCanceled;

            shipControl.ShipTurning.started   -= OnShipTurning;
            shipControl.ShipTurning.performed -= OnShipTurning;
            shipControl.ShipTurning.canceled  -= OnShipTurningCanceled;

            _input.Disable();
            _input.Dispose();
            _input = null;
        }

        #region Sail rotation
        void OnSailRotatingStarted(InputAction.CallbackContext ctx)
        {
            Debug.Log("Rotating Sail");
            _sailRotRoutine ??= StartCoroutine(SailRotateRoutine());
        }

        void OnSailRotatingCanceled(InputAction.CallbackContext ctx)
        {
            if (_sailRotRoutine != null)
            {
                StopCoroutine(_sailRotRoutine);
                _sailRotRoutine = null;
            }
        }

        IEnumerator SailRotateRoutine()
        {
            while (true)
            {
                _ship.ChangeSailRot(_input.ShipControl.SailRotating.ReadValue<float>());
                yield return null;
            }
        }
        #endregion


        #region Sail flur
        void OnSailFurlingStarted(InputAction.CallbackContext ctx)
        {
            Debug.Log("Fluring Sail");
            _sailFurlRoutine ??= StartCoroutine(SailFurlRoutine());
        }

        void OnSailFurlingCanceled(InputAction.CallbackContext ctx)
        {
            if (_sailFurlRoutine != null)
            {
                StopCoroutine(_sailFurlRoutine);
                _sailFurlRoutine = null;
            }
        }

        IEnumerator SailFurlRoutine()
        {
            while (true)
            {
                _ship.ChangeSailFurl(_input.ShipControl.SailFluring.ReadValue<float>());
                yield return null;
            }
        }
        #endregion

        #region Sail turning
        void OnShipTurning(InputAction.CallbackContext ctx)
        {
            Debug.Log("Ship turn");
            _ship.TurnShip(ctx.ReadValue<float>());
        }

        void OnShipTurningCanceled(InputAction.CallbackContext ctx)
        {
            _ship.TurnShip(0f);
        }
        #endregion
    }
}

