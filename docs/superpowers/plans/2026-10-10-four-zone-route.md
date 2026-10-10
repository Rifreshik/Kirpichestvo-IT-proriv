# Four-Zone Unity Route Implementation Plan

> **For agentic workers:** Work through this plan task by task in the current checkout. Preserve unrelated working-tree changes.

**Goal:** Make the existing Unity prototype playable as four isolated zones in one scene: start, target shooting, computer assembly, and a clear ending.

**Architecture:** Keep the existing single `SampleScene` and its two gameplay loops. Repair the player rig, place each loop on its own separated platform, route progression through the existing reward portal and assembly completion, add an explicit finish trigger, and register the scene for builds. Use an idempotent editor setup path for model placement; do not mutate the scene automatically on project open.

**Tech Stack:** Unity 2022.3 project, C#, Unity Editor APIs, legacy Input Manager already enabled (`activeInputHandler: 0`).

## Global Constraints

- Keep all four zones in `Assets/Scenes/SampleScene.unity`.
- Use the supplied FBX/Blender assets; no external location pack is present in the project.
- Preserve the player's current local changes in `Library/ArtifactDB-lock` and `Library/SourceAssetDB-lock`.
- Target the project's Unity version `2022.3.45f1`; the locally installed editor is `2022.3.62f3` and any version difference must be reported.
- Validate route order: start → shoot all targets → assemble all six PC parts → final.

---

## File Map

- `Assets/scripts/Movementtag.cs` — player movement, camera look, and player-camera attachment.
- `Assets/scripts/ShotTargetGoal.cs` — all-target reward and route to the computer assembly zone.
- `Assets/scripts/ComputerAssemblyStation.cs` — existing six-part assembly and transition to the final platform.
- `Assets/scripts/FinalZone.cs` — new end trigger and completion display.
- `Assets/scripts/FinalZone.cs.meta` — Unity script asset identity.
- `Assets/Editor/ModelsPlatformBuilder.cs` — safe, repeatable placement of supplied models in their intended zones.
- `Assets/Scenes/SampleScene.unity` — four-platform layout, player rig, route component, and model instances.
- `ProjectSettings/EditorBuildSettings.asset` — include the single playable scene in the build.

## Task 1: Repair the Player Rig

**Files:**
- Modify: `Assets/scripts/Movementtag.cs`
- Modify: `Assets/Scenes/SampleScene.unity`

- [ ] Rename the `MonoBehaviour` class to `Movementtag` so it matches its `.cs` filename and the scene's script asset.
- [ ] Cache and validate the player's `Rigidbody` and camera. Parent the existing root-level camera to the player and set a first-person local offset; tag it `MainCamera` so `Camera.main` works.
- [ ] Read input in `Update`, apply normalized movement in `FixedUpdate` with `Rigidbody.MovePosition`, and preserve the existing walk, sprint, and mouse sensitivity values.
- [ ] Keep missing-component handling explicit: emit a useful error and disable movement rather than throwing a `NullReferenceException`.
- [ ] Compile the project and inspect the player from the spawn point; verify WASD, sprint, mouse look, and weapon ray direction.

## Task 2: Arrange Four Separated Zones and Supplied Models

**Files:**
- Modify: `Assets/Editor/ModelsPlatformBuilder.cs`
- Modify: `Assets/Scenes/SampleScene.unity`
- Modify: `Assets/scripts/ShotTargetGoal.cs`

- [ ] Define stable coordinates for the start platform, target platform, assembly platform, and final platform. Move target spheres with their target platform and leave no unrelated platforms between the four stops.
- [ ] Keep the platform names referenced by gameplay (`Cube (9)` for assembly and `Cube (10)` for final), or update every corresponding lookup together.
- [ ] Replace the fixed reward destination with the assembly platform's actual transform so moving the zone cannot strand the reward portal.
- [ ] Place supplied FBX props near the relevant platform and at readable scale. Do not place every model at world origin.
- [ ] Remove automatic scene-opening/build triggers from `ModelsPlatformBuilder`; provide a manual menu command that detects existing instances and can be run repeatedly without duplicating or deleting unrelated scene content.
- [ ] Set a sensible camera far clip or equivalent occlusion/fog treatment after choosing spacing; verify from each spawn point that the next zone is not visible.
- [ ] In Unity, run the setup command twice and confirm the second run leaves the same four zones and model count.

## Task 3: Add a Clear, Reachable Ending

**Files:**
- Create: `Assets/scripts/FinalZone.cs`
- Create: `Assets/scripts/FinalZone.cs.meta`
- Modify: `Assets/Scenes/SampleScene.unity`
- Modify: `Assets/scripts/ComputerAssemblyStation.cs`

- [ ] Add a trigger volume above the final platform and accept only the player tag, including player child colliders whose root is tagged `Player`.
- [ ] On first entry, stop player movement, unlock the cursor, and display a Russian completion message. Ignore repeat trigger events.
- [ ] Ensure assembly completion places the player above the final platform using its collider bounds, clears Rigidbody linear/angular velocity, and faces the player toward the finish area.
- [ ] Verify the final platform is reachable only after the six required parts are installed in order.

## Task 4: Make the Scene Buildable and Verify the Full Route

**Files:**
- Modify: `ProjectSettings/EditorBuildSettings.asset`

- [ ] Add `Assets/Scenes/SampleScene.unity` with its existing scene GUID and enable it in build settings.
- [ ] Run Unity batch-mode compilation using the installed editor, capturing the log and checking for C# errors or missing scripts.
- [ ] Run the available Unity test suite if one is installed; otherwise do not add a test framework solely for this change.
- [ ] Verify in Play Mode that the player can leave the start, shoot all spheres once, use the reward portal, collect/install all six parts, and reach the completion message.
- [ ] Build a Windows player if the editor license and local toolchain allow it. Record any environment or license blocker separately from code results.
- [ ] Confirm `git status` shows only the intended project files plus the two pre-existing modified `Library` lock files.
