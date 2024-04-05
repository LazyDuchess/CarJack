# CarJack
![icon](https://github.com/LazyDuchess/CarJack/assets/42678262/e7f9bac8-6176-4fa4-b80f-b6d14bd45060)

Cruise through New Amsterdam like never before.

## Usage
A new `carjack` app will be available on your phone. Use it to access this mod's features.

## Controls
Below are the controls, their default bindings for both Keyboard + Mouse and Controller and their corresponding inputs in the in-game `Settings -> Input` Menu.

| Action      | KBM               | Controller    | BRC Input (KBM)  | BRC Input (Controller)  |
|-------------|-------------------|---------------|------------------|-------------------------|
| Accelerate  | W                 | Right Trigger | Forward          | Slide                   |
| Reverse     | S                 | Left Trigger  | Back             | Boost                   |
| Steer Right | D                 | Left Stick    | Right            |                         |
| Steer Left  | A                 | Left Stick    | Left             |                         |
| Handbrake   | Space             | A             | Jump             | Jump                    |
| Leave Car   | Q                 | Left Bumper   | Ride movestyle   | Ride movestyle          |
| Look Behind | Left Mouse Button | Y             | Trick 2          | Trick 2                 |
| Horn        | E                 | Right Bumper  | Spray            | Spray                   |
| Lock/Unlock Car Doors       | Right Arrow                | D-Pad Right  | Phone right            | Phone right                   |

### Air Control
On Controller, the left stick lets you adjust the pitch and yaw of your vehicle. Holding handbrake will let you adjust the roll of your car rather than the yaw.

On Keyboard + Mouse, the steering keys let you adjust the yaw of your vehicle. Holding handbrake lets you adjust the pitch with the accelerate and reverse keys and the roll with the steering keys.

## Dependencies
If you're using a mod manager such as r2modman dependencies are automatically installed for you.
* [CommonAPI](https://github.com/LazyDuchess/BRC-CommonAPI)

## Custom Cars (Work In Progress)
While tools aren't currently developed, if you know what you're doing you can clone the repo and open the `CarJack.Editor` project with Unity 2021.3.27f1 to create custom cars by cloning their prefabs and changing their Asset Bundle to a new one. Remember to also change their `Internal Name` so that they show up and sync correctly in SlopCrew. You can have multiple cars in bundles.

You can build the asset bundles with the `CarJack -> Build Asset Bundles` button, then copy the resulting asset bundle into your BepInEx plugins folder or any subfolder with the extension `.carbundle` (e.g. `AE86.carbundle`)

If `DeveloperMode` is enabled in this mod's BepInEx configuration file you can also reload car bundles in-game to test changes.

Keep in mind that this is still a WIP - it's not user friendly and breaking changes to custom cars may happen at anytime.
