using ShipRecruitmentTask.UI;
using ShipRecruitmentTask.Wind;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ShipRecruitmentTask.Ship
{
    public class ShipInjector : LifetimeScope
    {
        [Inject]
        private readonly WindController _windController;
        [SerializeField]
        ShipManager shipManager;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log($"Ship Manager is null {shipManager == null}");
            shipManager = Instantiate(shipManager);
            Debug.Log($"Ship Manager is null {shipManager == null}");
            // The Rigidbody and ShipPhysics live on this ship's GameObject.
            builder.RegisterComponentInHierarchy<Rigidbody>();

            builder.RegisterInstance(shipManager);
            builder.RegisterInstance<IShipController>(shipManager);
            builder.RegisterInstance<IShipDataContainer>(shipManager);

            builder.RegisterComponentInHierarchy<ShipPhysics>();

            // Rotates the sail visual to match the ship's sail state.
            builder.RegisterComponentInHierarchy<ShipVisuals>();

            // Maps player input to the ship and drives it each frame.
            builder.RegisterComponentInHierarchy<PlayerInputController>();
            builder.RegisterComponentInHierarchy<ShipStateUI>();
        }
    }
}
