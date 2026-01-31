# Changelog

Alle Änderungen an diesem Package werden in dieser Datei dokumentiert.

Das Format basiert auf [Keep a Changelog](https://keepachangelog.com/de/1.0.0/),
und das Projekt folgt [Semantic Versioning](https://semver.org/lang/de/).

## [Unreleased]

### Added
- Initiale Package-Struktur
- Assembly Definitions für Runtime, Editor und Tests
- Basis-Dokumentation

## [0.1.0] - TBD

### Added
- State Machine Framework (ICharacterState, CharacterStateMachine)
- Character States (Grounded, Airborne, Jumping, Falling)
- Movement System (MovementSimulator, GroundingDetection)
- MovementConfig ScriptableObject
- Input Provider System (IMovementInputProvider, PlayerInputProvider)
- CSP Strukturen (ControllerInput, InputBuffer, PredictionBuffer)
- Fixed Tick System
- PlayerController MonoBehaviour
- Unit Tests
- Demo-Szene und Prefabs
