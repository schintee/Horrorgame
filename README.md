# [Hide and Hunt]

A first-person stealth / survival horror game built in **Unity**. Explore a dark, door-filled facility with only a battery-powered lantern, collect the mysterious objects scattered around the level, avoid (or fight) patrolling enemies, and escape before you get caught.

---

## Gameplay Overview

You wake up inside a maze of corridors and locked rooms. Your goal is to gather all **5 Mysterious Objects** hidden in the level. Along the way you will need to find colored keys to unlock doors, manage your stamina and lantern battery, and deal with enemies that patrol the halls and hunt you on sight.

A timer runs the whole time, and your **best time** is tracked, so every run is a race against your own record.

### Win / Lose Conditions

| Outcome | What happens |
|---|---|
| **Win** | Collect all 5 objects to reach the **"YOU WIN!"** screen. |
| **Lose** | If an enemy catches you, a **"CAUGHT BY ENEMY!"** screen appears and you are sent back to the respawn point / the level restarts after a short delay. |

---

## Features

- **First-person controller** with walking, sprinting, jumping, mouse look and footstep audio.
- **Stamina system** – sprinting drains stamina and it regenerates when you slow down.
- **Lantern with limited battery** – the light flickers as the battery runs low, and a low-battery warning is shown on the HUD.
- **Enemy AI (NavMesh-based)**
  - Patrols between waypoints.
  - Detects the player with a field-of-view cone (140°) and a detection radius (15 m).
  - Chases the player and catches them if they stay within range.
  - Enemies can be fought back: they have health, hit reactions (red flash), and death sounds.
- **Melee combat** – attack enemies with a short-range melee strike (2 hits to defeat a standard enemy).
- **Key & door system** – colored keys (**Grey**, **Red**, **Blue**) open matching locked doors. Using the wrong door shows a *"You don't own the key for this door!"* message.
- **Collectibles** – pick up items to progress toward the win condition (`Objects: x/5`).
- **Safe zones** – green-lit areas where enemies can't hurt you and where **stamina and battery slowly recharge**.
- **HUD** – battery bar, stamina bar, object counter, timer, best time, enemy name and health bar, on-screen notifications.
- **Screens** – Game Over, Victory and "Wrong Key" overlays.
- **Post-processing** and background music for atmosphere.

---

## Controls

| Action | Input |
|---|---|
| Move | `W` `A` `S` `D` |
| Look | Mouse |
| Sprint | Hold `Shift` *(default – verify in the Input Actions asset)* |
| Jump | `Space` *(default – verify in the Input Actions asset)* |
| Open / close door | `E` |
| Pick up item / key | `P` |
| Attack | Left Mouse Button |
| Toggle lantern | *(see the lantern script / Input Actions asset)* |

---

## Default Gameplay Values

These are the values currently set in the `MainLevel` scene (all editable in the Inspector).

| System | Setting | Value |
|---|---|---|
| Player | Walk speed / Sprint speed | 3 / 6 |
| Player | Jump height | 1.5 |
| Player | Max stamina | 100 (drain 20/s, regen 15/s) |
| Lantern | Max battery | 100 (drain 5/s) |
| Combat | Attack damage / cooldown / range | 50 / 0.5 s / 2.5 m |
| Enemy | Max health | 100 |
| Enemy | Patrol speed / Chase speed | 2.2 / 2 |
| Enemy | Detection radius / Field of view | 15 m / 140° |
| Enemy | Catch distance / Catch hold time | 2 m / 1 s |
| Safe zone | Radius | 5 m |
| Safe zone | Stamina / Battery restore rate | 20/s / 15/s |
| Game | Objects to collect | 5 |
| Game | Restart delay after game over | 2 s |

---

## Project Structure

```
Assets/
└── Scenes/
    └── MainLevel.unity      # Main (and currently only) playable level
```

### Key objects in `MainLevel`

| GameObject | Purpose |
|---|---|
| `Player` | First-person controller, stamina, inventory/keys, interaction, melee attack |
| `Lantern` | Battery-powered light with flicker effect |
| `GameManager` | Timer, best time, object counter, win/lose flow |
| `PuzzleManager` | Puzzle tracking (reserved for puzzle logic) |
| `Enemy` objects (×7) | NavMesh agents with patrol/chase AI and health |
| `Waypoint1–4` | Patrol path points |
| `SafeZones` / `SafeLight` | Recharge areas |
| `Door`, `Door_Frame`, `DoorD_V2`, `Door_V1` … | Interactable doors (some locked by key) |
| `Collectible1–3` / `CollectiblePuzzle` | Collectable objects |
| `HUD`, `GameOverScreen`, `VictoryScreen`, `Wrongkey` | UI canvases |
| `NavMesh` | Baked navigation surface for enemy AI |
| `Global Volume` | Post-processing profile |
| `BackgroundMusic` | Ambient music |

---

## Tech Stack

- **Engine:** Unity *(add your exact version, e.g. 2022.3 LTS / Unity 6)*
- **Render pipeline:** Universal Render Pipeline (URP)
- **Input:** Unity Input System package
- **UI:** Unity UI (uGUI) with Legacy Text
- **AI / Navigation:** Unity NavMesh (`NavMeshSurface` + `NavMeshAgent`)
- **Language:** C#

---

## Getting Started

### Requirements

- Unity Hub and the Unity version listed above
- Universal RP and Input System packages (installed automatically via Package Manager)

### Run the project

1. Clone the repository:
   ```bash
   git clone <your-repo-url>
   ```
2. Open **Unity Hub** → **Add** → select the project folder.
3. Open the project with the correct Unity version.
4. In the Project window, open `Assets/Scenes/MainLevel.unity`.
5. Press **Play**.

> If enemies don't move, open the **NavMesh** object and click **Bake** to regenerate the navigation data.

### Build

1. **File → Build Settings**
2. Add `MainLevel` to *Scenes In Build*.
3. Choose your target platform and click **Build**.

---

## Known Limitations / Ideas for Improvement

- Only one level so far – exit doors are not linked to a next scene yet.
- Several audio and UI references (footsteps, lantern sounds, HUD counters) are still unassigned in the scene.
- Debug logging is enabled on many components; disable it for release builds.
- Chase speed is currently lower than patrol speed – consider rebalancing enemy difficulty.
- Ideas: more levels, more enemy types, save system, settings/pause menu, gamepad support.

---
