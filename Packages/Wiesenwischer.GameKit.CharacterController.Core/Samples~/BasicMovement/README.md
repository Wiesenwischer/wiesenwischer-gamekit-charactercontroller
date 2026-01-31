# Basic Movement Sample

Dieses Sample demonstriert die Grundfunktionen des Character Controller Core Packages.

## Setup

### Automatisches Setup

1. Öffne Unity
2. Gehe zu **Wiesenwischer > GameKit > Create Default MovementConfig**
3. Gehe zu **Wiesenwischer > GameKit > Create Demo Scene**
4. Gehe zu **Wiesenwischer > GameKit > Create Core Prefabs**
5. Weise im Player-Objekt die MovementConfig zu

### Manuelles Setup

1. Erstelle eine neue Szene
2. Erstelle ein leeres GameObject und nenne es "Player"
3. Füge folgende Components hinzu:
   - `CharacterController` (Unity Built-in)
   - `PlayerController` (aus diesem Package)
   - `PlayerInputProvider` (aus diesem Package)
4. Erstelle ein `MovementConfig` Asset:
   - Rechtsklick im Project Window
   - Create > Wiesenwischer > GameKit > Movement Config
5. Weise die Config dem PlayerController zu
6. Erstelle einen Ground Plane

## Steuerung

| Taste | Aktion |
|-------|--------|
| WASD | Bewegung |
| Leertaste | Springen |
| Shift | Sprinten |
| Maus | Kamera (falls implementiert) |

## Features demonstriert

- **Walking/Running**: Verschiedene Geschwindigkeiten
- **Jumping**: Variable Sprunghöhe
- **Coyote Time**: Kurze Sprungmöglichkeit nach Verlassen des Bodens
- **Jump Buffer**: Sprung-Input wird kurz vor Landung gebuffert
- **Slope Handling**: Bewegung auf Schrägen
- **Step Detection**: Automatisches Erklimmen kleiner Stufen

## Konfiguration

Die `MovementConfig` enthält alle anpassbaren Parameter:

### Ground Movement
- **Walk Speed**: Normale Bewegungsgeschwindigkeit
- **Run Speed**: Sprint-Geschwindigkeit
- **Acceleration**: Beschleunigung
- **Deceleration**: Verzögerung beim Stoppen

### Air Movement
- **Air Control**: Kontrolle in der Luft (0-1)
- **Gravity**: Schwerkraft
- **Max Fall Speed**: Maximale Fallgeschwindigkeit

### Jumping
- **Jump Height**: Maximale Sprunghöhe
- **Coyote Time**: Zeit nach Ground-Verlust, in der noch gesprungen werden kann
- **Jump Buffer Time**: Zeit vor Landung, in der Sprung-Input gespeichert wird

### Ground Detection
- **Ground Check Distance**: Reichweite der Bodenerkennung
- **Ground Layers**: Welche Layer als Boden zählen
- **Max Slope Angle**: Maximaler begehbarer Winkel

## Debug

Im Play Mode zeigt der Custom Inspector:
- Aktueller State
- Velocities (horizontal/vertikal)
- Ground Info
- State History

Aktiviere "Draw Gizmos" für visuelle Debug-Informationen im Scene View.
