using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ShipRecruitmentTask.Wind
{
    public class WindInjector : LifetimeScope
    {
        [Header("Initial Wind")]
        [SerializeField]
        Vector2 WindInitialDirection = Vector2.right;

        [SerializeField]
        float windInitialForce = 5f;

        [SerializeField]
        [Tooltip("Max speed of wind")]
        private float windMaxForce;

        [Header("Smooth Change")]
        [Tooltip("Seconds it takes to smoothly rotate the wind direction when it changes.")]
        [SerializeField]
        float WindDirectionChangeDuration = 3f;

        [Tooltip("Seconds it takes to smoothly change the wind force when it changes.")]
        [SerializeField]
        float WindForceChangeDuration = 3f;

        IWindController _windController;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("Wind Injector start");
            _windController = new WindController(WindInitialDirection, windInitialForce, WindDirectionChangeDuration, WindForceChangeDuration, windMaxForce);
            builder.RegisterInstance<IWindController>(_windController);
        }
    }
}

