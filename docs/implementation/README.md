# Implementierungsplan - GameKit Character Controller

> **Letzte Aktualisierung:** 2026-02-07
> **Aktuelle Phase:** Phase 1 - Animation-Vorbereitung
> **Aktueller Schritt:** 1.1

---

## Übersicht

Dieser Plan beschreibt die schrittweise Implementierung des GameKit Character Controllers mit Animation, Abilities und Netzwerk-Unterstützung.

**Prinzipien:**
- Jede Phase = eigener Feature-Branch
- Jeder Schritt = eigener Commit
- Dokumentation vor Implementierung

---

## Tracking

### Phase 1: Animation-Vorbereitung
**Branch:** `feature/phase-1-animation-prep`

- [ ] [1.1 Character Asset beschaffen](phase-1-animation-prep/1.1-character-asset.md)
- [ ] [1.2 Character in Unity importieren](phase-1-animation-prep/1.2-import-character.md)
- [ ] [1.3 Basis-Animationen beschaffen](phase-1-animation-prep/1.3-animations.md)
- [ ] [1.4 Animation Package Struktur](phase-1-animation-prep/1.4-package-structure.md)

### Phase 2: Animator Setup
**Branch:** `feature/phase-2-animator-setup`

- [ ] [2.1 Avatar Masks erstellen](phase-2-animator-setup/2.1-avatar-masks.md)
- [ ] [2.2 Animator Controller erstellen](phase-2-animator-setup/2.2-animator-controller.md)
- [ ] [2.3 Locomotion Blend Tree](phase-2-animator-setup/2.3-locomotion-blend-tree.md)
- [ ] [2.4 Airborne States](phase-2-animator-setup/2.4-airborne-states.md)
- [ ] [2.5 Parameter-Bridge](phase-2-animator-setup/2.5-parameter-bridge.md)

### Phase 3: Animation-Integration
**Branch:** `feature/phase-3-animation-integration`

- [ ] [3.1 IAnimationController Interface](phase-3-animation-integration/3.1-animation-interface.md)
- [ ] [3.2 CharacterAnimator Komponente](phase-3-animation-integration/3.2-character-animator.md)
- [ ] [3.3 State Machine Sync](phase-3-animation-integration/3.3-state-machine-sync.md)
- [ ] [3.4 Player Prefab](phase-3-animation-integration/3.4-player-prefab.md)
- [ ] [3.5 Testing & Feintuning](phase-3-animation-integration/3.5-testing.md)

### Phase 4: Ability System
**Branch:** `feature/phase-4-ability-system`

- [ ] [4.1 IAbility Interface](phase-4-ability-system/4.1-ability-interface.md)
- [ ] [4.2 AbilitySystem Manager](phase-4-ability-system/4.2-ability-system.md)
- [ ] [4.3 JumpAbility](phase-4-ability-system/4.3-jump-ability.md)
- [ ] [4.4 SprintAbility](phase-4-ability-system/4.4-sprint-ability.md)
- [ ] [4.5 Animation Layer Integration](phase-4-ability-system/4.5-animation-layer.md)

### Phase 5: Netzwerk-Grundstruktur
**Branch:** `feature/phase-5-network`

- [ ] [5.1 FishNet einbinden](phase-5-network/5.1-fishnet-setup.md)
- [ ] [5.2 NetworkPlayer](phase-5-network/5.2-network-player.md)
- [ ] [5.3 Input Sync](phase-5-network/5.3-input-sync.md)
- [ ] [5.4 Position/Rotation Sync](phase-5-network/5.4-transform-sync.md)
- [ ] [5.5 Client-Side Prediction](phase-5-network/5.5-csp.md)

### Phase 6: Netzwerk-Animation
**Branch:** `feature/phase-6-network-animation`

- [ ] [6.1 Animator Sync](phase-6-network-animation/6.1-animator-sync.md)
- [ ] [6.2 State Sync](phase-6-network-animation/6.2-state-sync.md)
- [ ] [6.3 Ability Sync](phase-6-network-animation/6.3-ability-sync.md)
- [ ] [6.4 Lag Compensation](phase-6-network-animation/6.4-lag-compensation.md)

---

## Abhängigkeiten

```
Phase 1 ──> Phase 2 ──> Phase 3
                            │
                            v
                       Phase 4
                            │
                            v
                       Phase 5 ──> Phase 6
```

---

## Workflow

### Neue Phase starten
```bash
git checkout main
git pull origin main
git checkout -b feature/phase-X-name
```

### Schritt abschließen
```bash
git add .
git commit -m "feat(phase-X): X.Y Beschreibung"
```

### Phase abschließen
```bash
git push -u origin feature/phase-X-name
gh pr create --title "feat: Phase X - Name"
# Nach Review: Merge
```

---

## Zeitschätzung

| Phase | Beschreibung | Dauer |
|-------|--------------|-------|
| 1 | Animation-Vorbereitung | ~2h |
| 2 | Animator Setup | ~3h |
| 3 | Animation-Integration | ~3h |
| 4 | Ability System | ~4h |
| 5 | Netzwerk-Grundstruktur | ~5h |
| 6 | Netzwerk-Animation | ~4h |

**Gesamt:** ~21 Stunden
