# Elements
## ShipInjector
class for injecting into ShipPhysics and ShipVisuals ShipManager

## WindController
control direction and power of wind
## Player Input
handle inputs from player, and put them into right IShipController
## ShipPhysics
Calculate 
## ShipManager
orchestrate elements of ship, like ShipSphysics, ShipVisuals, and implement IShipController interface
## ShipVisuals
Control visuals of the ship, like animations

# IShipController
void ChangeSailRot(float change)
void ChangeSailFurl(float change)