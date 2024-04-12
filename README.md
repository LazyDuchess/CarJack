# CarJack
![icon](https://github.com/LazyDuchess/CarJack/assets/42678262/e7f9bac8-6176-4fa4-b80f-b6d14bd45060)

Cruise through New Amsterdam like never before.

## Usage
A new `carjack` app will be available on your phone. Use it to access this mod's features.

## Controls
Below are the controls, their default bindings for both Keyboard + Mouse and Controller and their corresponding inputs in the in-game `Settings -> Input` Menu.

To bring up the phone while in a car on keyboard, use the Dance button. `F` by default.

| Action                | KBM               | Controller    | BRC Input (KBM)  | BRC Input (Controller)  |
|-----------------------|-------------------|---------------|------------------|-------------------------|
| Accelerate            | W                 | Right Trigger | Forward          | Slide                   |
| Reverse               | S                 | Left Trigger  | Back             | Boost                   |
| Steer Right           | D                 | Left Stick    | Right            |                         |
| Steer Left            | A                 | Left Stick    | Left             |                         |
| Handbrake             | Space             | A             | Jump             | Jump                    |
| Leave Car             | Q                 | Left Bumper   | Ride movestyle   | Ride movestyle          |
| Look Behind           | Left Mouse Button | Y             | Trick 2          | Trick 2                 |
| Horn                  | E                 | Right Bumper  | Spray            | Spray                   |
| Lock/Unlock Car Doors | Right Arrow       | D-Pad Right   | Phone right      | Phone right             |

### Air Control
On Controller, the left stick lets you adjust the pitch and yaw of your vehicle. Holding handbrake will let you adjust the roll of your car rather than the yaw.

On Keyboard + Mouse, the steering keys let you adjust the yaw of your vehicle. Holding handbrake lets you adjust the pitch with the accelerate and reverse keys and the roll with the steering keys.

Helicopter control type can be changed from the BepInEx plugin settings.

### Helicopter (Type A)

| Action                       | KBM               | Controller    | BRC Input (KBM)  | BRC Input (Controller)  |
|------------------------------|-------------------|---------------|------------------|-------------------------|
| Lift/Thrust                  | W                 | Right Trigger | Forward          | Slide                   |
| Go Down                      | S                 | Left Trigger  | Back             | Boost                   |
| Roll Right                   | Right Arrow       | Left Stick    | Right            |                         |
| Roll Left                    | Left Arrow        | Left Stick    | Left             |                         |
| Pitch Down                   | Up Arrow          | Left Stick    | Phone up         |                         |
| Pitch Up                     | Down Arrow        | Left Stick    | Phone down       |                         |
| Turn Right                   | D                 | B             | Right            | Trick 3                 |
| Turn Left                    | A                 | X             | Left             | Trick 1                 |
| Leave Helicopter             | Q                 | Left Bumper   | Ride movestyle   | Ride movestyle          |
| Look Behind                  | Left Mouse Button | Y             | Trick 2          | Trick 2                 |
| Lock/Unlock Helicopter Doors | Space             | D-Pad Right   | Jump             | Phone right             |

### Helicopter (Type B)

| Action                       | KBM               | Controller    | BRC Input (KBM)  | BRC Input (Controller)  |
|------------------------------|-------------------|---------------|------------------|-------------------------|
| Lift/Thrust                  | W                 | Right Trigger | Forward          | Slide                   |
| Go Down                      | S                 | Left Trigger  | Back             | Boost                   |
| Roll Right                   | Right Arrow       | B             | Right            | Trick 3                 |
| Roll Left                    | Left Arrow        | X             | Left             | Trick 1                 |
| Pitch Down                   | Up Arrow          | Left Stick    | Phone up         |                         |
| Pitch Up                     | Down Arrow        | Left Stick    | Phone down       |                         |
| Turn Right                   | D                 | Left Stick    | Right            |                         |
| Turn Left                    | A                 | Left Stick    | Left             |                         |
| Leave Helicopter             | Q                 | Left Bumper   | Ride movestyle   | Ride movestyle          |
| Look Behind                  | Left Mouse Button | Y             | Trick 2          | Trick 2                 |
| Lock/Unlock Helicopter Doors | Space             | D-Pad Right   | Jump             | Phone right             |

## Dependencies
If you're using a mod manager such as r2modman dependencies are automatically installed for you.
* [CommonAPI](https://github.com/LazyDuchess/BRC-CommonAPI)

## Custom Cars (Work In Progress)
While tools aren't currently developed, you can clone the repo and open the `CarJack.Editor` project with Unity 2021.3.27f1 to create custom cars.

All default cars are located in the `Assets/Prefabs` folder. You can copy one of them, change their asset bundle to a new bundle with the extension `.carbundle` and work from there. Remember to change the car's `Internal Name` to something unique.

![image](https://github.com/LazyDuchess/CarJack/assets/42678262/1dd54ef6-61a4-4376-a924-73902aef33ac)

You can have multiple car prefabs in a single asset bundle.

You can build the asset bundles with the `CarJack -> Build Asset Bundles` button and find the resulting asset bundles inside `CarJack.Editor/Build`. Copy the `.carbundle` files from here into your BepInEx plugins folder to see them in-game - the other files can be safely ignored.

If `DeveloperMode` is enabled in this mod's BepInEx configuration file you can also reload car bundles in-game to test changes.

Keep in mind that this is still a WIP - it's not user friendly and breaking changes to custom cars may happen at anytime.
