# GameKit Character Controller Core

Modularer Third-Person Character Controller mit State Machine, deterministischer Movement-Logik und CSP-Vorbereitung für MMO-Spiele.

## Features

- **State Machine**: Hierarchische State Machine für Character-Zustände
- **Deterministisches Movement**: CSP-ready Bewegungslogik mit Fixed Tick Rate
- **Input Abstraktion**: Interface-basiertes Input-System (Unity Input System optional)
- **Ground Detection**: Raycast/SphereCast-basierte Boden-Erkennung
- **Prediction Buffer**: Vorbereitung für Client-Side Prediction

## Installation

Das Package ist Teil des Unity-Projekts unter `Packages/Wiesenwischer.GameKit.CharacterController.Core/`.

## Verwendung

```csharp
// PlayerController zu einem GameObject hinzufügen
var player = gameObject.AddComponent<PlayerController>();
player.Config = movementConfig; // MovementConfig ScriptableObject
```

## Architektur

```
Core/
├── StateMachine/       # State Machine System
│   ├── ICharacterState.cs
│   ├── CharacterStateMachine.cs
│   └── States/
├── Movement/           # Movement System
│   ├── MovementSimulator.cs
│   ├── GroundingDetection.cs
│   └── MovementConfig.cs
├── Input/              # Input Abstraktion
│   ├── IMovementInputProvider.cs
│   └── PlayerInputProvider.cs
├── Prediction/         # CSP Vorbereitung
│   ├── ControllerInput.cs
│   ├── InputBuffer.cs
│   └── PredictionBuffer.cs
└── PlayerController.cs # Haupt-Controller
```

## Dependencies

- Unity 2022.3 LTS
- Unity Input System (optional)

## Lizenz

Teil des Wiesenwischer GameKit Frameworks.
