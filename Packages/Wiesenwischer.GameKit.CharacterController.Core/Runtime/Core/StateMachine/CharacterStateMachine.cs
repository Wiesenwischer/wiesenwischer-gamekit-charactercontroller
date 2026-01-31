using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wiesenwischer.GameKit.CharacterController.Core.StateMachine
{
    /// <summary>
    /// State Machine für Character States.
    /// Verwaltet State Transitions und History für Debugging.
    /// </summary>
    public class CharacterStateMachine
    {
        private readonly Dictionary<string, ICharacterState> _registeredStates;
        private readonly StateHistory _history;
        private ICharacterState _currentState;
        private IStateMachineContext _context;
        private bool _isInitialized;

        /// <summary>
        /// Maximale Anzahl der History-Einträge.
        /// </summary>
        public const int MaxHistoryEntries = 100;

        /// <summary>
        /// Erstellt eine neue CharacterStateMachine.
        /// </summary>
        public CharacterStateMachine()
        {
            _registeredStates = new Dictionary<string, ICharacterState>();
            _history = new StateHistory(MaxHistoryEntries);
        }

        /// <summary>
        /// Der aktuelle State.
        /// </summary>
        public ICharacterState CurrentState => _currentState;

        /// <summary>
        /// Name des aktuellen States.
        /// </summary>
        public string CurrentStateName => _currentState?.StateName ?? "None";

        /// <summary>
        /// Ob die State Machine initialisiert wurde.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Anzahl der registrierten States.
        /// </summary>
        public int RegisteredStateCount => _registeredStates.Count;

        /// <summary>
        /// Zugriff auf die State History für Debugging.
        /// </summary>
        public StateHistory History => _history;

        #region State Registration

        /// <summary>
        /// Registriert einen State in der State Machine.
        /// </summary>
        /// <param name="state">Der zu registrierende State.</param>
        /// <exception cref="ArgumentNullException">Wenn state null ist.</exception>
        /// <exception cref="ArgumentException">Wenn ein State mit diesem Namen bereits existiert.</exception>
        public void RegisterState(ICharacterState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (_registeredStates.ContainsKey(state.StateName))
            {
                throw new ArgumentException($"State mit Namen '{state.StateName}' ist bereits registriert.");
            }

            _registeredStates[state.StateName] = state;
        }

        /// <summary>
        /// Registriert mehrere States auf einmal.
        /// </summary>
        /// <param name="states">Die zu registrierenden States.</param>
        public void RegisterStates(params ICharacterState[] states)
        {
            foreach (var state in states)
            {
                RegisterState(state);
            }
        }

        /// <summary>
        /// Entfernt einen registrierten State.
        /// </summary>
        /// <param name="stateName">Name des zu entfernenden States.</param>
        /// <returns>True wenn erfolgreich entfernt.</returns>
        public bool UnregisterState(string stateName)
        {
            if (_currentState?.StateName == stateName)
            {
                Debug.LogWarning($"[CharacterStateMachine] Kann aktiven State '{stateName}' nicht entfernen.");
                return false;
            }

            return _registeredStates.Remove(stateName);
        }

        /// <summary>
        /// Prüft ob ein State mit dem Namen registriert ist.
        /// </summary>
        public bool HasState(string stateName)
        {
            return _registeredStates.ContainsKey(stateName);
        }

        /// <summary>
        /// Gibt einen registrierten State zurück.
        /// </summary>
        /// <param name="stateName">Name des States.</param>
        /// <returns>Der State oder null wenn nicht gefunden.</returns>
        public ICharacterState GetState(string stateName)
        {
            _registeredStates.TryGetValue(stateName, out var state);
            return state;
        }

        /// <summary>
        /// Gibt einen registrierten State als spezifischen Typ zurück.
        /// </summary>
        /// <typeparam name="T">Der erwartete State-Typ.</typeparam>
        /// <param name="stateName">Name des States.</param>
        /// <returns>Der State als T oder null.</returns>
        public T GetState<T>(string stateName) where T : class, ICharacterState
        {
            return GetState(stateName) as T;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialisiert die State Machine mit dem gegebenen Kontext.
        /// </summary>
        /// <param name="context">Der State Machine Kontext.</param>
        /// <param name="initialStateName">Name des initialen States.</param>
        /// <exception cref="ArgumentNullException">Wenn context null ist.</exception>
        /// <exception cref="ArgumentException">Wenn der initiale State nicht registriert ist.</exception>
        public void Initialize(IStateMachineContext context, string initialStateName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!_registeredStates.TryGetValue(initialStateName, out var initialState))
            {
                throw new ArgumentException($"Initialer State '{initialStateName}' ist nicht registriert.");
            }

            _context = context;
            _currentState = initialState;
            _isInitialized = true;

            // Enter initial state
            _currentState.Enter(_context);
            _history.AddEntry(null, _currentState, _context.CurrentTick, StateTransitionReason.Initialization);
        }

        #endregion

        #region Update

        /// <summary>
        /// Aktualisiert die State Machine.
        /// Sollte jeden Tick aufgerufen werden.
        /// </summary>
        /// <param name="deltaTime">Die Zeit seit dem letzten Tick.</param>
        public void Update(float deltaTime)
        {
            if (!_isInitialized || _currentState == null)
            {
                return;
            }

            // Update current state
            _currentState.Update(_context, deltaTime);

            // Check for transitions
            var nextState = _currentState.EvaluateTransitions(_context);
            if (nextState != null && nextState != _currentState)
            {
                TransitionTo(nextState, StateTransitionReason.Condition);
            }
        }

        #endregion

        #region Transitions

        /// <summary>
        /// Wechselt zu einem State per Name.
        /// </summary>
        /// <param name="stateName">Name des Ziel-States.</param>
        /// <param name="reason">Grund für den Wechsel.</param>
        /// <returns>True wenn erfolgreich gewechselt.</returns>
        public bool TransitionTo(string stateName, StateTransitionReason reason = StateTransitionReason.Forced)
        {
            if (!_registeredStates.TryGetValue(stateName, out var targetState))
            {
                Debug.LogWarning($"[CharacterStateMachine] State '{stateName}' ist nicht registriert.");
                return false;
            }

            TransitionTo(targetState, reason);
            return true;
        }

        /// <summary>
        /// Wechselt zu einem State.
        /// </summary>
        /// <param name="targetState">Der Ziel-State.</param>
        /// <param name="reason">Grund für den Wechsel.</param>
        public void TransitionTo(ICharacterState targetState, StateTransitionReason reason = StateTransitionReason.Forced)
        {
            if (targetState == null)
            {
                Debug.LogWarning("[CharacterStateMachine] Kann nicht zu null State wechseln.");
                return;
            }

            if (!_isInitialized)
            {
                Debug.LogWarning("[CharacterStateMachine] State Machine ist nicht initialisiert.");
                return;
            }

            var previousState = _currentState;

            // Exit current state
            _currentState?.Exit(_context);

            // Enter new state
            _currentState = targetState;
            _currentState.Enter(_context);

            // Record history
            _history.AddEntry(previousState, _currentState, _context.CurrentTick, reason);
        }

        /// <summary>
        /// Wechselt zu einem State per Typ.
        /// </summary>
        /// <typeparam name="T">Der State-Typ.</typeparam>
        /// <param name="reason">Grund für den Wechsel.</param>
        /// <returns>True wenn erfolgreich gewechselt.</returns>
        public bool TransitionTo<T>(StateTransitionReason reason = StateTransitionReason.Forced) where T : class, ICharacterState
        {
            foreach (var state in _registeredStates.Values)
            {
                if (state is T)
                {
                    TransitionTo(state, reason);
                    return true;
                }
            }

            Debug.LogWarning($"[CharacterStateMachine] Kein State vom Typ {typeof(T).Name} registriert.");
            return false;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Setzt die State Machine zurück.
        /// </summary>
        public void Reset()
        {
            if (_currentState != null && _context != null)
            {
                _currentState.Exit(_context);
            }

            _currentState = null;
            _context = null;
            _isInitialized = false;
            _history.Clear();
        }

        /// <summary>
        /// Gibt alle registrierten State-Namen zurück.
        /// </summary>
        public IEnumerable<string> GetRegisteredStateNames()
        {
            return _registeredStates.Keys;
        }

        #endregion
    }

    /// <summary>
    /// Gründe für State-Übergänge.
    /// </summary>
    public enum StateTransitionReason
    {
        /// <summary>Initialisierung der State Machine.</summary>
        Initialization,
        /// <summary>Bedingung im aktuellen State erfüllt.</summary>
        Condition,
        /// <summary>Erzwungener Wechsel von außen.</summary>
        Forced,
        /// <summary>Netzwerk-Synchronisation.</summary>
        NetworkSync,
        /// <summary>Rollback durch CSP.</summary>
        Rollback
    }

    /// <summary>
    /// Verwaltet die History der State-Übergänge.
    /// </summary>
    public class StateHistory
    {
        private readonly Queue<StateHistoryEntry> _entries;
        private readonly int _maxEntries;

        public StateHistory(int maxEntries)
        {
            _maxEntries = maxEntries;
            _entries = new Queue<StateHistoryEntry>(maxEntries);
        }

        /// <summary>
        /// Anzahl der History-Einträge.
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// Fügt einen neuen History-Eintrag hinzu.
        /// </summary>
        public void AddEntry(ICharacterState fromState, ICharacterState toState, int tick, StateTransitionReason reason)
        {
            // Remove oldest if at capacity
            while (_entries.Count >= _maxEntries)
            {
                _entries.Dequeue();
            }

            _entries.Enqueue(new StateHistoryEntry
            {
                FromStateName = fromState?.StateName ?? "None",
                ToStateName = toState?.StateName ?? "None",
                Tick = tick,
                Timestamp = Time.time,
                Reason = reason
            });
        }

        /// <summary>
        /// Gibt alle History-Einträge zurück (älteste zuerst).
        /// </summary>
        public IEnumerable<StateHistoryEntry> GetEntries()
        {
            return _entries;
        }

        /// <summary>
        /// Gibt die letzten N Einträge zurück (neueste zuerst).
        /// </summary>
        public StateHistoryEntry[] GetRecentEntries(int count)
        {
            var entries = _entries.ToArray();
            var result = new StateHistoryEntry[Mathf.Min(count, entries.Length)];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = entries[entries.Length - 1 - i];
            }

            return result;
        }

        /// <summary>
        /// Gibt den letzten Eintrag zurück.
        /// </summary>
        public StateHistoryEntry? GetLastEntry()
        {
            if (_entries.Count == 0)
            {
                return null;
            }

            var arr = _entries.ToArray();
            return arr[arr.Length - 1];
        }

        /// <summary>
        /// Leert die History.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }
    }

    /// <summary>
    /// Ein Eintrag in der State History.
    /// </summary>
    public struct StateHistoryEntry
    {
        /// <summary>Name des vorherigen States.</summary>
        public string FromStateName;
        /// <summary>Name des neuen States.</summary>
        public string ToStateName;
        /// <summary>Tick bei dem der Übergang stattfand.</summary>
        public int Tick;
        /// <summary>Zeitstempel (Time.time).</summary>
        public float Timestamp;
        /// <summary>Grund für den Übergang.</summary>
        public StateTransitionReason Reason;

        public override string ToString()
        {
            return $"[Tick {Tick}] {FromStateName} -> {ToStateName} ({Reason})";
        }
    }
}
