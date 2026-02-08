# ARCHITECTURE --- World, Terrain, Weather & Streaming System

------------------------------------------------------------------------

# 🌍 1. Architektur Vision

Das Ziel ist eine modulare, MMO-fähige World Architecture basierend auf:

-   Ability-driven Gameplay
-   Modular Packages (GameKit Ansatz)
-   Data-driven WorldState
-   Non-destructive Terrain Modifikation
-   Zone-based Environment Simulation
-   Chunk-based Streaming

## Kernprinzip

👉 Systeme kontrollieren nicht die Welt.

👉 Die Welt wird durch einen zentralen Zustand definiert:

WorldState = Single Source of Truth

------------------------------------------------------------------------

# 🧠 2. WorldState Architektur

## Definition

WorldState ist ein Datenmodell, das globale Umweltzustände beschreibt.

### Beispielstruktur

``` csharp
struct WorldState
{
    float TimeOfDay;
    Season CurrentSeason;

    float SnowAmount;
    float RainAmount;
    float FogIntensity;
    float WindStrength;
}
```

## Verantwortung

WorldState: - enthält KEINE Rendering-Logik - enthält KEINE
Terrain-Logik - ist nur Daten

## Consumer Pattern

WorldState \| v Consumers: - EnviroAdapter - TerrainAdapter -
AudioSystem - GameplayAbilities - UI Toolkit

------------------------------------------------------------------------

# 🌦️ 3. Weather Architektur (Enviro Integration)

## Grundsatz

Enviro ist: 👉 Renderer + Simulation

Aber: 👉 NICHT Master-System.

## Architektur

Enviro Weather Simulation \| v EnviroAdapter \| v WorldState (normalized
data)

## Vorteile

-   Asset austauschbar
-   Multiplayer-ready
-   System unabhängig

## Snow/Wetness

HDRP native Shader + Global Shader Parameter:

\_GlobalSnowAmount \_GlobalWetnessAmount

------------------------------------------------------------------------

# 🌄 4. Terrain Architektur (Microverse)

## Entscheidung

Microverse wird genutzt als: 👉 Non-destructive Terrain System.

## Layer Stack

Base Terrain + Biome Layer + Road Modifier Layer + Building Modifier
Layer + Snow Layer

## Vorteile

-   jederzeit änderbar
-   runtime geeignet
-   ideal für Build-Systeme

------------------------------------------------------------------------

# 🏗️ 5. Build System Architektur

## Grundidee

Terrain wird NICHT direkt verändert.

BuildAbility \| v BuildCommand \| v WorldModifier

## WorldModifier Beispiele

-   Terrain flatten
-   Road spline
-   Vegetation removal
-   House foundation

------------------------------------------------------------------------

# 🧱 6. Claim System Architektur

## Motivation

Persistent World Änderungen brauchen Ownership.

## Struktur

World -\> Claim -\> Modifiers

### Claim

``` csharp
class Claim
{
    Guid id;
    Bounds area;
    List<WorldModifier> modifiers;
}
```

## Vorteile

-   Multiplayer Ownership
-   Undo möglich
-   Performance kontrollierbar

------------------------------------------------------------------------

# 🌦️ 7. Zone-Based Environment System

## Motivation

Unterschiedliche Wetterzonen gleichzeitig.

## Architektur

GlobalWorldState + ZoneStates

ZoneState Beispiel:

``` csharp
struct ZoneState
{
    float Snow;
    float Rain;
    float Temperature;
}
```

## Blending

Lokales Wetter wird berechnet aus: - Distanz - Priorität - Zone Falloff

------------------------------------------------------------------------

# 🌍 8. World Streaming Architektur

## Entscheidung

Scene Streaming reicht nicht.

Benötigt: 👉 Chunk-based Streaming.

## Struktur

World -\> Chunk\[x,z\] -\> Claims -\> Modifiers

## Streaming Ablauf

Player moves -\> detect chunk change -\> load nearby chunks -\> unload
distant chunks

------------------------------------------------------------------------

# 🧩 9. Adapter Pattern

WorldState \| + EnviroAdapter + MicroverseAdapter + ShaderAdapter

## Vorteil

-   Austauschbarkeit
-   modulare Packages

------------------------------------------------------------------------

# 📦 10. Package Struktur (GameKit Style)

Wiesenwischer.gamekit.world WorldState ZoneSystem EventBus

Wiesenwischer.gamekit.weather EnviroAdapter

Wiesenwischer.gamekit.terrain.microverse TerrainAdapter

Wiesenwischer.gamekit.build BuildCommands WorldModifiers

Wiesenwischer.gamekit.streaming ChunkManager WorldStreamer

------------------------------------------------------------------------

# 🎮 11. Ability Integration

BuildAbility -\> CreateModifierCommand

Gameplay liest nur WorldState.

------------------------------------------------------------------------

# ❄️ 12. HDRP Integration Guidelines

-   HDRP Sky deaktivieren wenn Enviro aktiv
-   Global Shader Parameter für Snow/Wetness
-   MaterialPropertyBlock für lokale Anpassungen

------------------------------------------------------------------------

# 🔮 13. Zukünftige Erweiterung (vorgemerkt)

👉 Interest Management Streaming System

------------------------------------------------------------------------

# ✅ Zusammenfassung

Die Architektur basiert auf:

-   WorldState als zentrale Datenquelle
-   Enviro als Visual Adapter
-   Microverse als non-destructive Terrain Framework
-   Claim-basierte Weltänderungen
-   Chunk-based Streaming
-   Ability-driven World Modification
