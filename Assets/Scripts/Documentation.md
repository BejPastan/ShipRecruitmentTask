# ShipRecruitmentTask — Documentation

## Overview
Sailing-ship prototype (Unity + VContainer + Input System).
- `Wind` owns the global wind vector with smooth interpolated transitions.
- `ShipController` owns ship state (`ShipManager` ScriptableObject), physics, visuals, input, and DI wiring.
- `UI/ShipStateUI` binds ship state to UIToolkit labels.
- Input defined in `Resources/ShipInput.inputactions`, accessed via generated `Resources/ShipInput.cs`.

DI scopes:
- `WindInjector` -> registers `IWindController` singleton.
- `ShipInjector` (ship prefab scope) → instantiates + registers `ShipManager` as `IShipController` / `IShipDataContainer`, plus `Rigidbody`, `ShipPhysics`, `ShipVisuals`, `PlayerInputController`, `ShipStateUI`.


## Elements

### WindInjector (`Scripts/Wind/WindInjector.cs`)
Inject wind dependency

### WindController (`Scripts/Wind/WindController.cs`)
Owns wind state and it's updates

### ShipPhysics (`Scripts/ShipController/ShipPhysics.cs`)
Calculate forces for ship, and apply them
set ship max speed
set ship side friction(to slow ship lateral movement)

### ShipInjector (`Scripts/ShipController/ShipInjector.cs`)
Injecting dependencies into ship modules

### ShipManager (`Scripts/ShipController/ShipManager.cs`)
Manage ship, hold data and recieve inputs

  maxSailAngle - max angle the sail could rotate from center
  maxSailFlur - max value of flur of sail(does not effect max speed or force)
  sailRotationSpeed - speed in deg/1 of rotation of sail
  sailFlurSpeed - speed of fluring sail in units/s
  turnAngle - angle in which ship turn

### PlayerInputController (`Scripts/ShipController/PlayerInputController.cs`)
Test Player binding between InputActions and ShipManager

### ShipStateUI (`Scripts/UI/ShipStateUI.cs`)
Manage bindign beteween UI and state of ship

### ShipVisuals (`Scripts/ShipController/ShipVisuals.cs`)
Manage animations and visualization of ship state
