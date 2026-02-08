# Wiesenwischer GameKit -- Master Architecture Overview

This document summarizes the complete Unity Open‑World architecture
discussed:

-   Multi Scene Setup
-   CoreRoot Pattern
-   Terrain & Streaming
-   Player Architecture
-   Floating Origin
-   Lifetime / Initialization Pattern

Designed for:

-   HDRP
-   Microverse Terrain
-   Enviro Weather
-   Ability-driven gameplay
-   MMO-ready scalability

------------------------------------------------------------------------

# 🧭 High Level Architecture

    Bootstrap Scene
           |
           v
    Persistent Scene (CoreRoot)
           |
           + Environment Scene
           + Terrain Scenes
           + Player Scene
           + UI Scene
           |
           v
    Runtime Streaming Content

------------------------------------------------------------------------

# 🧠 Core Layers (Mental Model)

## Bootstrap

Responsible for startup only.

Loads scenes in correct order:

1.  Persistent
2.  Environment
3.  Terrain
4.  Player
5.  UI
6.  Start streaming

Contains only:

-   BootstrapLoader

------------------------------------------------------------------------

## Persistent Scene (CoreRoot)

The brain of the application.

Contains logic systems only:

-   WorldState
-   EventBus
-   StreamingManager
-   ClaimSystem
-   Save/Load
-   AudioManager
-   Floating Origin controller

NO visuals here.

------------------------------------------------------------------------

## World Scenes (Visual Layer)

### Environment Scene

-   Enviro Weather
-   Lighting
-   Global Volumes

### Terrain Scenes

-   Terrain Tiles
-   Microverse layers

------------------------------------------------------------------------

## Player Scene

Player exists independently from terrain.

Contains:

-   PlayerRoot
-   CharacterController
-   Ability System
-   Camera (Cinemachine)
-   Input

------------------------------------------------------------------------

## Runtime Streaming

Dynamic content loaded during gameplay:

-   Claims
-   Buildings
-   Roads
-   NPCs
-   Props

------------------------------------------------------------------------

# 🌍 Terrain Architecture

## Terrain Tiles vs Streaming Chunks

Terrain Tiles:

-   Rendering units
-   Larger (e.g. 1024m)

Streaming Chunks:

-   Gameplay data units
-   Smaller (e.g. 128m)

Example:

Terrain Tile = 1024m Chunk Size = 128m

One terrain contains multiple chunks.

------------------------------------------------------------------------

# 🌦️ Weather Architecture

Enviro is NOT master system.

Instead:

Enviro -\> Adapter -\> WorldState

WorldState drives:

-   Gameplay
-   UI
-   Shader snow/wetness
-   Audio reactions

Global shader parameters example:

    _GlobalSnowAmount
    _GlobalWetnessAmount

------------------------------------------------------------------------

# 🧱 Hierarchy Layout

Top-level GameObjects:

    CoreRoot
    WorldRoot
    PlayerRoot
    UIRoot
    DebugRoot (optional)

## WorldRoot structure

    WorldRoot
       EnvironmentRoot
       TerrainRoot
       RuntimeContentRoot

Floating origin moves ONLY WorldRoot.

------------------------------------------------------------------------

# 🎮 Player Architecture

Player in separate scene.

Reasons:

-   Terrain streaming safe
-   Multiplayer ready
-   Camera independent
-   Respawn easier

------------------------------------------------------------------------

# 🌍 Floating Origin

Problem:

Floating point precision decreases far from world origin.

Solution:

Shift world instead of player.

Implementation:

Move WorldRoot transform when player exceeds threshold distance.

------------------------------------------------------------------------

# ⭐ CoreRoot Pattern

Single access point for services.

Example:

    CoreRoot
       WorldStateManager
       EventBus
       StreamingManager

Access from any scene:

    CoreRoot.Instance.WorldState

Use interfaces when possible.

------------------------------------------------------------------------

# ⭐ Lifetime / Initialization Pattern

Major source of bugs in Unity:

-   Undefined startup order
-   Race conditions between systems

Solution:

Controlled initialization stages.

## Initialization Phases

1.  Bootstrap
2.  Core systems ready
3.  Environment initialized
4.  Terrain initialized
5.  Player spawned
6.  Streaming started

Example flow:

    Bootstrap
       -> Load Persistent
       -> Initialize CoreRoot
       -> Load Environment
       -> Load Terrain
       -> Spawn Player
       -> Start Streaming

Benefits:

-   No race conditions
-   Stable multi-scene architecture
-   Predictable startup

------------------------------------------------------------------------

# 🧩 Architecture Principles Summary

Persistent thinks. World shows. Player experiences. Streaming expands.

WorldState = Single Source of Truth.

------------------------------------------------------------------------

# 🔮 Future Extensions (planned)

-   Interest-based streaming
-   Dual coordinate system
-   MMO networking authority
