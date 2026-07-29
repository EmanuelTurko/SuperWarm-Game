# Project Overview
- **Game Title**: SuperHot Style FPS Level
- **High-Level Concept**: A highly tactical, fast-paced level with complex intersecting hallways and side-rooms. Enemies can spot and fire at the player through multiple gaps, windows, and cross-angles.
- **Players**: Single player
- **Inspiration / Reference Games**: SuperHot, Mirror's Edge, Portal (clean industrial/sci-fi style)
- **Tone / Art Direction**: Minimalist, clean industrial, sci-fi (white building blocks, red/orange enemy indicators, slow-motion gameplay)
- **Target Platform**: PC / Standalone Windows
- **Screen Orientation / Resolution**: Landscape 1920x1080
- **Render Pipeline**: Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
- Player spawns, moves through hallways, and must eliminate enemies while dodging incoming bullets.
- Using walls, corners, and pillars as cover is essential to survive cross-room window-fire.
- Player reaches the escape teleporter pad to complete the level.

## Controls and Input Methods
- Standard mouse and keyboard FPS controls:
  - WASD for movement
  - Mouse for aiming/looking
  - Left Mouse Button to shoot
  - Space to jump
  - 'B' to toggle between First Person and Third Person views (already implemented in PlayerMovement).

# UI
- **Crosshair**: Standard screen-center crosshair for aiming.
- **HP Bar**: Displays player health (max 2 HP).
- **Timer**: Level time remaining (starts at 60s, counts down).

# Key Asset & Context
The level will be constructed using the following assets in `Assets/Buildings`:
- Floors: `FloorTile_Basic.fbx` (2x2 units, pivot at bottom-center)
- Walls:
  - Solid: `Wall_1.fbx`, `Wall_2.fbx` (4x4.43 units)
  - Windows: `ThreeWindows_Wall_SideA.fbx`, `Window_Wall_SideA.fbx` (4x4.43 units, with open gaps)
- Columns: `Column_1.fbx` (serving as tactical cover)
- Stairs / Roofs: `Staircase.fbx`, `RoofTile_Empty.fbx`
- Props: `Props_Computer.fbx`, `Props_Shelf.fbx`, `Props_Teleporter_1.fbx` (used for the exit portal)
- Player Prefab: `Assets/Prefab/Player.prefab`
- Enemy Prefab: `Assets/Prefab/Enemy_Rifle Slowmo.prefab`

# Implementation Steps

## Step 1: Copy Core Game Systems from LevelTest
- **Description**: Open the existing `LevelTest.unity` scene, copy the configured `Player`, `Canvas`, `EventSystem`, and `Global Volume` GameObjects, and paste them into the fresh `Level1.unity` scene. This ensures all UI, cameras, post-processing, and physics settings are perfectly configured.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

## Step 2: Build the Grid-Aligned Level Layout
- **Description**: Write and execute a C# Editor script to programmatically lay out the 3D grid-aligned level inside `Level1.unity`. The layout consists of a 4x3 cell grid (each cell is 4x4 units) with the following structure:
  - **(0, 0) Start Room**: Enclosed on South, West, and East. Open to North. Player spawns here.
  - **(0, 1) Window Hallway**: West wall solid, East wall contains a 3-window panel looking into the Server Side Room `(1, 1)`.
  - **(0, 2) Corner Room**: North and West walls solid. A large column in the center for cover. Open to East.
  - **(1, 2) Crossing Room**: North wall solid, South wall has a window looking into Server Side Room `(1, 1)`. Open to West and East.
  - **(2, 2) Middle Junction**: North and South walls solid. Open to West and East.
  - **(3, 2) Overlook Corner**: North and East walls solid. Open to West and South.
  - **(3, 1) Sniper Hallway**: East wall solid, West wall contains a 3-window panel looking into Sniper Room `(2, 1)`. Open to North and South.
  - **(2, 1) Sniper Room**: North, South, and West walls solid. East wall contains windows looking into `(3, 1)`.
  - **(1, 1) Server Side Room**: South wall solid, West wall has windows looking into `(0, 1)`. North wall has windows looking into `(1, 2)`. East wall solid.
  - **(3, 0) Escape Room**: South, East, and West walls solid. North is open. Features the escape portal at the center.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Populate Enemies & Wire Up References
- **Description**: Instantiate 5 slow-motion enemies (`Enemy_Rifle Slowmo.prefab`) at tactical locations:
  - **Enemy 1** at `(1, 2)` (coordinates `x=4, z=8`): patrols the hallway.
  - **Enemy 2** at `(3, 2)` (coordinates `x=12, z=8`): stands at the corner, guarding the turn.
  - **Enemy 3** at `(2, 1)` (coordinates `x=8, z=4`): sniper inside the room, shooting through the East windows into `(3, 1)`.
  - **Enemy 4** at `(1, 1)` (coordinates `x=4, z=4`): server room guard, shooting through West windows into `(0, 1)` and North windows into `(1, 2)`.
  - **Enemy 5** at `(3, 0)` (coordinates `x=12, z=0`): guards the final escape portal.
  - **Wiring**: For each instantiated enemy, programmatically assign their `player` field to the Player Transform in the scene.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

## Step 4: Setup the Escape Portal and Scene Transition
- **Description**: Place `Props_Teleporter_1.fbx` at the center of the Escape Room `(3, 0)`. Add a BoxCollider with `IsTrigger = true` and attach the `ScenePortal` component. Set `sceneToLoad` to `"LevelTest"`.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

## Step 5: Scene Visual and Integrity Validation
- **Description**: Run visual check of the generated level using `CaptureMultiAngleSceneView`. Verify there are no overlapping meshes, floating objects, or gaps in walls and floors. Verify player starting position and enemy positions.
- **Assigned role**: developer
- **Dependencies**: Step 3, Step 4
- **Parallelizable**: No

# Verification & Testing
- **Visual Validation**: Take high-quality multi-angle screenshots to confirm grid-alignment, proper height, and visual fidelity.
- **Player Playtest**: Enter Play Mode to test movement, shooting, slow-motion gameplay, and path flow.
- **Tactical Check**: Walk through the level and verify that:
  - Enemies in side rooms `(1, 1)` and `(2, 1)` correctly spot and shoot at the player through the window walls.
  - The column at `(0, 2)` offers adequate cover.
  - Reaching the escape teleporter successfully loads the `LevelTest` scene.
