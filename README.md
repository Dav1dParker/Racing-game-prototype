# Racing Game Prototype (Unity)

A arcade-style racing game prototype built in Unity 6, featuring realistic car physics, drifting, boost mechanics, surface-based grip, checkpoints, and lap timing.

[Video preview](https://youtu.be/ZP93yhlo0S8)

[Russian readme](READMEru.md)

---

## Features

### Car Physics
- Physically simulated vehicle using Rigidbody and WheelColliders
- Configurable drive types: FWD, RWD, AWD
- Variable steering angle based on speed
- Dynamic aerodynamic drag and rolling resistance
- Drift detection based on lateral G-force
- Adjustable grip per surface (asphalt, dirt, ice, etc.)

### Boost System
- Temporary boost mechanic with adjustable force, duration, and cooldown
- Boost can be recharged via pickups
- Controller vibration feedback during boost
- UI boost bar for cooldown progress

### Lap System
- Checkpoint-based lap tracking with forward/reverse direction support
- Lap timer UI showing current and best times
- Persistent best lap times stored in a local JSON file

### Audio System
- Contextual sound effects for boost start/end, recharge, and lap finish

### Surface Handling
- Custom SurfaceGripData defining friction multipliers per surface tag
- Automatic detection via wheel contact tags

### Visuals
- Cinemachine camera for smooth follow behavior and FOV increase at speed
- Brake lights that illuminate when braking
- Floating pickable items (boost refills, recharge pickups) with respawn timers

---

## Project Structure

```
Assets/
└── _RacingGamePrototype/
    ├── Scripts/
    │   ├── Audio/               # Sound and music system
    │   ├── Car/                 # Vehicle control and physics
    │   ├── Input/               # Unity Input System bindings
    │   ├── LapSystem/           # Checkpoints and lap tracking
    │   ├── UI/                  # In-game HUD and timers
    │   └── World/               # Managers, pickups, and surfaces
    ├── Materials/
    ├── Prefabs/
    ├── Scenes/
    └── Sounds/
```

---

## Tech Stack

| Component | Description |
|------------|-------------|
| Engine | Unity 6 (6000.2.9f1) |
| Input | Unity New Input System |
| Camera | Cinemachine |
| Physics | WheelColliders + Rigidbody |
| Persistence | JSON via System.IO |

---



## Save Data

- Best lap times are stored in a JSON file:

  ```"RacingGamePrototype_Data\BestLapTimes.json"```
- Automatically created on first launch
- Persists between sessions and updates whenever a new record is set

---

## Controls

| Action | Keyboard | Gamepad |
|--------|-----------|---------|
| Accelerate | W     | RT |
| Brake / Reverse | S | LT |
| Steer | A / D | Left Stick |
| Boost | Shift | A |
| Respawn | R | start |
| Reverse Track | O | Y |

---

## How to Run

1. **Clone the repository**

    ```git clone https://github.com/Dav1dParker/Racing-game-prototype```

2. Open the project in **Unity 6000.2.9f1** or newer.
3. Open the scene:

   ```Assets/_CardGamePrototype/Scenes/Track.unity```
4. Press **Play**.

### OR

1. [Download archive](https://disk.360.yandex.ru/d/wLqe-QuUWAdMvA)

2. Unpack the archive
3. Run ```RacingGamePrototype.exe```

