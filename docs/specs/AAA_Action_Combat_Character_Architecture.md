# AAA Action Combat & Character Architecture Overview

Generated on: 2026-02-07 23:48

------------------------------------------------------------------------

## 1. Overall Architecture Philosophy

Modern AAA-style character systems separate responsibilities into
multiple layers:

-   **Intent System** -- What the player wants to do
-   **Movement / Locomotion Layer** -- How the character moves
-   **Ability System** -- Actions like attacks, dodge, interact
-   **Status Layer** -- Stun, root, knockback etc.
-   **Animation Layer** -- Visual execution
-   **Character Motor** -- Executes the final movement

This avoids state explosion and keeps systems modular.

------------------------------------------------------------------------

## 2. Intent System (Player Intent vs Character State)

### Problem

Directly mapping input → state causes:

-   missed inputs
-   unresponsive controls
-   difficult animation syncing

### Solution

Input → Intent → State Machine

Example:

-   Player presses jump
-   Jump intent stored
-   State machine executes when grounded

Benefits:

-   input buffering
-   coyote time
-   smoother gameplay feel
-   multiplayer-friendly

------------------------------------------------------------------------

## 3. Locomotion Graph (Movement Decision Tree)

Instead of many transitions:

Idle → Walk → Run → Jump → Fall

Use calculated movement modes:

Grounded Airborne Sliding Swimming

Movement mode determined by:

-   grounded state
-   velocity
-   environment
-   gameplay tags

Advantages:

-   deterministic behaviour
-   fewer transitions
-   easier expansion

------------------------------------------------------------------------

## 4. State Layers vs Ability Layers

### Movement Layer

-   Idle
-   Move
-   Jump
-   Fall

### Ability Layer

-   Attack
-   Dodge
-   Cast
-   Interact

### Status Layer

-   Stunned
-   Rooted
-   Dead

Movement continues while abilities play.

------------------------------------------------------------------------

## 5. Gameplay Tags

Replace boolean checks with tag-based logic:

Example tags:

-   State.Grounded
-   State.Airborne
-   Ability.Attack
-   Status.Stunned

Benefits:

-   systems loosely coupled
-   easier extension
-   cleaner conditions

------------------------------------------------------------------------

## 6. Priority System

Every state/ability has a priority.

Example:

-   Move = 1
-   Jump = 2
-   Attack = 10
-   Dodge = 20
-   Stun = 100

Rule:

NewPriority \>= CurrentPriority

Allows forced overrides (e.g. stun interrupts everything).

------------------------------------------------------------------------

## 7. Character Motor vs State Machine (IMPORTANT)

### Key Concept

You do NOT need an extra MotorController if you already have:

-   Character Motor
-   State Machine

The important part is HOW states communicate with the motor.

### Roles

States = Decision Layer\
Motor = Execution Layer (Single Source of Truth)

States should NOT directly:

-   modify transform
-   overwrite velocity directly

Instead they provide:

-   movement intent
-   impulses
-   modifiers

Example:

    State → movementIntent.jumpImpulse
    State → movementIntent.desiredVelocity
    Ability → movementIntent.externalForce

Motor combines internally:

    finalVelocity =
        desiredVelocity
      + gravity
      + impulses
      + external forces

Benefits:

-   Jump + attack works simultaneously
-   Dodge does not break movement
-   Knockback integrates cleanly
-   Deterministic behaviour

------------------------------------------------------------------------

## 8. Motion Warping / Root Alignment

### Problem

Animation movement rarely matches gameplay positioning.

Issues:

-   foot sliding
-   attacks missing target
-   dodge distance wrong

### Solution

Motion Warping adjusts movement dynamically:

Animation movement + Target position = Final movement

Especially important for action combat.

------------------------------------------------------------------------

## 9. Action Combat vs Tab Targeting

Action combat requires precise spatial alignment:

-   hitboxes matter
-   animation timing matters
-   positioning matters

Motion warping ensures attacks land correctly.

------------------------------------------------------------------------

## 10. Animation Driven Windows (Core of Action Combat)

Instead of hardcoding timing, animation defines windows:

### Hit Window

Frames where damage is applied.

### Cancel Window

Allows interrupting animation (e.g. dodge cancel).

### Input Buffer Window

Allows next action queued before animation ends.

Example:

Frame 10-15 → Hit active Frame 18-25 → Combo input allowed Frame 30 →
Recover

Benefits:

-   responsive combat
-   smooth combo chaining
-   precise gameplay feel

------------------------------------------------------------------------

## 11. Recommended High-Level Flow

Input System ↓ Intent System ↓ Locomotion Graph (Movement Mode) ↓ State
Machine (Decision Layer) ↓ Movement Intent Aggregation ↓ Character Motor
(Execution Layer) ↓ Animator Layers

------------------------------------------------------------------------

## 12. Mental Model

-   Intent = Player wish
-   State = Movement condition
-   Ability = Action execution
-   Tags = Communication between systems
-   Motor = Final movement authority
-   Motion Warping = Align animation with gameplay reality

------------------------------------------------------------------------

End of document.
