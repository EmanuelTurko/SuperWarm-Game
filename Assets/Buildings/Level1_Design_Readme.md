# Level 1 - Tactical FPS Level Design Readme (SuperHot-Style)

This document outlines the architectural plan, layout, player pathing, and prefab selection for **Level1**—a highly tactical, close-quarters combat sandbox optimized for a fast-paced "SuperHot-Style" FPS experience.

---

## 📐 1. Overall Layout Plan
The level is designed around a **4x3 grid system** (using cell dimensions of 4x4 units, matching the modular dimensions of the prefabs). Rather than a long, linear hallway, it forms a **compact, snake-like ring loop with an inner courtyard/server room**. 

By using an intersecting layout, the space feels large and complex while remaining physically compact. This design allows the entire level to be finished in **30 to 45 seconds** if rushed, but forces the player to use slow-motion thinking to survive intense crossfires.

```
       [0, 2] Corner Cover           [1, 2] Crossing Corridor        [2, 2] High-Risk Junction       [3, 2] Overlook Corner
      +------------------------+    +------------------------+    +------------------------+    +------------------------+
      |  (Solid Outer Walls)   |====|  (Solid North Wall)    |====|  (Solid North Wall)    |====| (Outer Walls Solid)    |
      |   [Center Column]      |    |  Patrolling Enemy 1    |    |                        |    |  Stationary Enemy 2    |
      +------------------------+    +------------------------+    +------------------------+    +------------------------+
                  ||                            ||                                                           ||
                  ||                            || (Window View)                                             ||
      +------------------------+    +------------------------+                                  +------------------------+
      |  (Solid West Wall)     |    |   Server Side Room     |                                  |   Sniper Corridor      |
      |   Crossfire Hallway    |====|        [1, 1]              |                                  |        [3, 1]          |
      |                        |    |   Stationary Enemy 4   |                                  |                        |
      +------------------------+    +------------------------+                                  +------------------------+
                  ||                     // (Diagonal sight)                                                 ||
                  ||                    //                                                                   || (Window View)
      +------------------------+                                                                +------------------------+
      |  [0, 0] Player Spawn   |                                                                |   [3, 0] Goal Room     |
      |   (Solid Walls)        |                                                                |   Escape Teleporter    |
      |   Safe Starting Zone   |                                                                |   Stationary Enemy 5   |
      +------------------------+                                                                +------------------------+
```

---

## 🏃 2. Intended Player Path
1. **Spawn [0, 0]**: The player spawns in a fully-enclosed, safe starter room facing North.
2. **First Threat [0, 1]**: The player moves North into the *Crossfire Hallway*. On their right, large windows expose them to an enemy waiting inside the *Server Side Room [1, 1]*. The player must either rush past or fire back through the window.
3. **Pillar Cover [0, 2]**: Reaching the northwest corner, the player can take cover behind a solid column, planning their next move.
4. **Corridor Crossing [1, 2]**: Heading East, the player encounters a patrolling enemy. The corridor has a side window looking down into the *Server Side Room*, creating a secondary threat angle from below/side.
5. **The Junction [2, 2]**: A brief tactical buffer zone connecting the northern crossing to the eastern sector.
6. **The Ambush [3, 2]**: Turning South at the northeast corner, a stationary guard immediately aims at the player.
7. **Sniper Alley [3, 1]**: Rushing South, the player is exposed to a sniper hiding in the adjacent room `[2, 1]` who shoots through side windows. The player has to fire through the window gaps while moving.
8. **Final Showdown [3, 0]**: The player enters the Goal Room, defeats the final guard, and steps into the teleporter to escape.

---

## 👁️ 3. Enemy Sightline & Crossfire Ideas
* **Through-Window Exposure**: Large windows are placed along key hallway boundaries. Instead of simple walls, the player is constantly visible to stationary enemies sitting inside the central rooms `[1, 1]` and `[2, 1]`.
* **Multi-Angle Threat**: In the *Northern Crossing [1, 2]*, the player is squeezed between a moving threat (Patrolling Enemy 1) and a potential flank shot from the central room.
* **Sniper Window Gaps**: In the *Sniper Corridor [3, 1]*, the wall is composed of vertical window slats. This lets the player and the sniper trade shots while running, utilizing the pillars of the window frame as moving cover.

---

## 🧱 4. Prefab Allocation Matrix
All modular elements used to construct this level are located in `Assets/Buildings`. 

### A. Prefab Usage by Structural Role
| Structural Role | Selected Prefabs | Justification |
| :--- | :--- | :--- |
| **Floor** | `FloorTile_Basic.fbx`, `FloorTile_Basic2.fbx` | Flat, fully solid surfaces designed to support navigation and colliders. |
| **Ceiling** | `RoofTile_Empty.fbx`, `RoofTile_Plate.fbx` | Thin panels designed to act as upper enclosures. |
| **Outer Walls (Solid)** | `Walls/Wall_1.fbx` to `Walls/Wall_5.fbx` | Solid, double-sided, thick concrete structures to seal the map boundaries. |
| **Inner Walls (Aperture)** | `Walls/ThreeWindows_Wall_SideA.fbx`, `Walls/Window_Wall_SideA.fbx` | Walls with openings and windows designed to allow sightlines and projectiles to pass. |
| **Cover** | `Column_1.fbx` | Large concrete pillars suitable for blocking player/enemy fire at key corners. |
| **Stairs** | `Staircase.fbx` | Multi-step modular stair segments (not required for this single-level plan but available). |
| **Props** | `Props_Computer.fbx`, `Props_Shelf.fbx` | Interior room fillers to break lines of sight and add flavor. |
| **Exit Goal** | `Props_Teleporter_1.fbx` | Serves as the interactive exit pad. |

---

## ⚠️ 5. Safety, Soundness, and Unsuitable Assets
To ensure visual integrity and mechanical reliability, several prefabs MUST NOT be used for general structural roles:

* **DO NOT use Floor assets as Walls, or Wall assets as Floors**: Walls and floors have distinct UV mapping, mesh pivots, and orientations. Swapping them causes broken geometry, incorrect texture mapping, and physics glitches.
* **`FloorTile_Empty.fbx` (UNSUITABLE for walking)**: This asset is an open metal frame. If used for floors, it leaves a hollow gap that looks visually broken and would cause the player to fall out of bounds.
* **`Walls/Wall_Empty.fbx` (Doorway/Opening)**: This is an empty archway/frame. It should not be used as a solid outer wall, as it would expose the void of the scene. It is only safe for interior doorways.
* **Mesh Colliders are REQUIRED**: The source `.fbx` models in `Assets/Buildings` do NOT have the `addCollider` flag enabled. Therefore, our level generation editor script **MUST programmatically attach a `MeshCollider` component** to all instantiated floor tiles, wall segments, and column cover props so that gravity, character controller collisions, and raycast bullets work correctly.

---

## 🏁 6. Escape Mechanism
* **Target Pad**: `Props_Teleporter_1` positioned in cell `[3, 0]`.
* **Configuration**: Equipped with a custom `BoxCollider` (`IsTrigger = true`) and the `ScenePortal` script component referencing `"LevelTest"`.
