using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
namespace ShipRecruitmentTask.Wind
{
    public interface IWindController
    {
        public float WindMaxForce { get; }

        /// <summary>
        /// Raised on the main thread whenever the wind direction and/or force changes
        /// (including each step of a smooth transition).
        /// </summary>
        public event Action<Vector3> WindChanged;

        /// <summary>
        /// Direction and power of wind (Direction * Force).
        /// </summary>
        public Vector3 Wind
        {
            get;
        }

        /// <summary>Normalized direction of the wind.</summary>
        public Vector3 Direction
        {
            get;
        }

        /// <summary>Current power (speed) of the wind.</summary>
        public float Force
        {
            get;
        }
    }

    /// <summary>
    /// Owns the current wind direction and force and notifies subscribers whenever they change.
    /// The interpolation math runs on a background task;
    /// </summary>
    [Preserve]
    public class WindController : IDisposable, IWindController
    {
        private readonly object syncRoot = new object();

        private Vector3 wind;
        private float windMaxForce;

        private CancellationTokenSource changeCts;

        // Captured on the main thread so background work can post back onto it.
        private SynchronizationContext mainThreadContext;

        /// <summary>
        /// Time, in seconds, it takes to smoothly change the wind direction.
        /// </summary>
        private float DirectionChangeDuration { get; set; }

        /// <summary>
        /// Time, in seconds, it takes to smoothly change the wind force.
        /// </summary>
        private float ForceChangeDuration { get; set; }

        public float WindMaxForce { get { return windMaxForce; }}

        /// <summary>
        /// Raised on the main thread whenever the wind direction and/or force changes
        /// (including each step of a smooth transition).
        /// </summary>
        public event Action<Vector3> WindChanged;

        /// <summary>
        /// Direction and power of wind (Direction * Force).
        /// </summary>
        public Vector3 Wind
        {
            get
            {
                lock (syncRoot)
                {
                    return wind;
                }
            }
        }

        /// <summary>Normalized direction of the wind.</summary>
        public Vector3 Direction
        {
            get
            {
                lock (syncRoot)
                {
                    return wind.normalized;
                }
            }
        }

        /// <summary>Current power (speed) of the wind.</summary>
        public float Force
        {
            get
            {
                lock (syncRoot)
                {
                    return wind.magnitude;
                }
            }
        }

        public WindController(
            Vector2 windDirection,
            float windForce,
            float directionChangeDuration,
            float forceChangeDuration,
            float windMaxSpeed)
        {
            windDirection = windDirection.normalized;
            wind.x = windDirection.x;
            wind.z = windDirection.y;
            wind.y = 0;
            wind *= windForce;
            DirectionChangeDuration = Mathf.Max(0f, directionChangeDuration);
            ForceChangeDuration = Mathf.Max(0f, forceChangeDuration);
            windMaxForce = windMaxSpeed;
            mainThreadContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Smoothly changes the wind direction and/or force over time. The interpolation runs on a
        /// background task; each computed value is delivered on the main thread through
        /// <see cref="ApplyWindChange"/>. Pass <c>null</c> to leave a value unchanged. Any change
        /// already in progress is cancelled.
        /// </summary>
        public Task ChangeWindAsync(Vector3? targetDirection = null, float? targetForce = null)
        {
            if (!targetDirection.HasValue && !targetForce.HasValue)
            {
                return Task.CompletedTask;
            }

            var newCts = new CancellationTokenSource();
            var previousCts = Interlocked.Exchange(ref changeCts, newCts);
            if (previousCts != null)
            {
                previousCts.Cancel();
                previousCts.Dispose();
            }

            var token = newCts.Token;

            var startDirection = Direction;
            var startForce = Force;

            float directionDuration = targetDirection.HasValue ? Mathf.Max(0f, DirectionChangeDuration) : 0f;
            float forceDuration = targetForce.HasValue ? Mathf.Max(0f, ForceChangeDuration) : 0f;
            targetForce = targetForce.HasValue ? Mathf.Max(0f, targetForce.Value) : startForce;
            targetDirection = targetDirection.HasValue
                ? NormalizeDirection(targetDirection.Value)
                : startDirection;



            return Task.Run(async () =>
            {
                await InterpolateWindChange(token, directionDuration, forceDuration, startDirection, targetDirection.Value, startForce, targetForce.Value); 
            }, token);
        }

        private async Task InterpolateWindChange(CancellationToken token, float directionDuration, float forceDuration, Vector3 startDir, Vector3 endDir, float startForce, float endForce)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    float elapsed = (float)stopwatch.Elapsed.TotalSeconds;

                    float directionT = directionDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / directionDuration);
                    float forceT = forceDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / forceDuration);

                    Vector3 newDirection = InterpolateDirection(startDir, endDir, directionT);
                    float newForce = Mathf.Lerp(startForce, endForce, forceT);



                    // Hand the freshly computed value to the main thread; state is only
                    // mutated and the event only fired from there.
                    ApplyWindChange(newDirection * newForce);

                    if (directionT >= 1f && forceT >= 1f)
                    {
                        break;
                    }

                    await Task.Delay(16, token);
                }
            }
            catch (OperationCanceledException)
            {
                // A newer change superseded this one (or the scope was disposed) - exit quietly.
            }
        }

        /// <summary>
        /// Applies a new wind direction and force and raises <see cref="WindChanged"/> on the main
        /// thread. Safe to call from a background thread: the work is marshalled onto the thread
        /// the controller was created on.
        /// </summary>
        private void ApplyWindChange(Vector3 direction)
        {
            mainThreadContext.Post(_ =>
            {
                lock (syncRoot)
                {
                    wind = direction;
                }

                WindChanged?.Invoke(direction);
            }, null);
        }

        private static Vector3 InterpolateDirection(Vector3 from, Vector3 to, float t)
        {
            if (t <= 0f) return from;
            if (t >= 1f) return to;

            var maxRadians = Vector3.Angle(from, to) * Mathf.Deg2Rad * t;
            return Vector3.RotateTowards(from, to, maxRadians, 0f);
        }

        private static Vector3 NormalizeDirection(Vector2 direction)
        {
            var dir = new Vector3(direction.x, 0f, direction.y);
            return dir.sqrMagnitude < 1e-6f ? Vector3.forward : dir.normalized;
        }

        public void Dispose()
        {
            var cts = Interlocked.Exchange(ref changeCts, null);
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}


