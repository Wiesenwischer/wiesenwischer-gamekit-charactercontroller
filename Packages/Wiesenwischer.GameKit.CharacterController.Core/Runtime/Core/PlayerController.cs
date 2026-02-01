using UnityEngine;
using Wiesenwischer.GameKit.CharacterController.Core.Data;
using Wiesenwischer.GameKit.CharacterController.Core.Input;
using Wiesenwischer.GameKit.CharacterController.Core.Locomotion;
using Wiesenwischer.GameKit.CharacterController.Core.Motor;
using Wiesenwischer.GameKit.CharacterController.Core.StateMachine;

namespace Wiesenwischer.GameKit.CharacterController.Core
{
    /// <summary>
    /// Hauptkomponente für Character Controller.
    /// Basiert auf dem Genshin Impact Pattern:
    /// - Zentraler Zugriffspunkt für alle Komponenten
    /// - Verwendet PlayerMovementStateMachine mit ReusableData
    /// - CSP (Client-Side Prediction) kompatibel für MMO-Nutzung
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Configuration")]
        [Tooltip("Locomotion-Konfiguration")]
        [SerializeField] private LocomotionConfig _config;

        [Header("Capsule Settings")]
        [Tooltip("Skin Width für Kollisionserkennung")]
        [SerializeField] private float _skinWidth = 0.02f;

        [Header("Input")]
        [Tooltip("Input Provider (optional - wird automatisch gesucht)")]
        [SerializeField] private MonoBehaviour _inputProviderComponent;

        [Header("Debug")]
        [Tooltip("Debug-Informationen anzeigen")]
        [SerializeField] private bool _showDebugInfo = true;
        [Tooltip("Debug-Gizmos zeichnen")]
        [SerializeField] private bool _drawGizmos = true;

        #endregion

        #region Components (Genshin Pattern - Read-Only Properties)

        /// <summary>Der CapsuleCollider für Kollisionserkennung.</summary>
        public CapsuleCollider CapsuleCollider { get; private set; }

        /// <summary>Der Input Provider.</summary>
        public IMovementInputProvider InputProvider { get; private set; }

        /// <summary>Ground Detection System.</summary>
        public GroundingDetection GroundingDetection { get; private set; }

        /// <summary>Character Locomotion System.</summary>
        public CharacterLocomotion Locomotion { get; private set; }

        /// <summary>Die Locomotion-Konfiguration.</summary>
        public LocomotionConfig LocomotionConfig => _config;

        /// <summary>Der kinematische Motor.</summary>
        public KinematicMotor KinematicMotor => Locomotion?.Motor;

        #endregion

        #region State Machine

        private PlayerMovementStateMachine _movementStateMachine;

        /// <summary>Die Movement State Machine.</summary>
        public PlayerMovementStateMachine MovementStateMachine => _movementStateMachine;

        /// <summary>Shared runtime data (Shortcut).</summary>
        public PlayerStateReusableData ReusableData => _movementStateMachine?.ReusableData;

        #endregion

        #region Tick System

        private TickSystem _tickSystem;

        /// <summary>Das Tick-System.</summary>
        public TickSystem TickSystem => _tickSystem;

        #endregion

        #region Public Properties (Convenience)

        /// <summary>Der aktuelle State-Name.</summary>
        public string CurrentStateName => _movementStateMachine?.CurrentStateName ?? "None";

        /// <summary>Ob der Character auf dem Boden steht.</summary>
        public bool IsGrounded => GroundingDetection?.IsGrounded ?? false;

        /// <summary>Ob der Character gerade rutscht.</summary>
        public bool IsSliding => Locomotion?.IsSliding ?? false;

        /// <summary>Die aktuelle Geschwindigkeit.</summary>
        public Vector3 Velocity => ReusableData?.Velocity ?? Vector3.zero;

        /// <summary>Aktueller Tick.</summary>
        public int CurrentTick => _tickSystem?.CurrentTick ?? 0;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            InitializeComponents();
            InitializeTickSystem();
            InitializeSystems();
            InitializeStateMachine();
        }

        private void Update()
        {
            // 1. Input lesen und in ReusableData schreiben
            UpdateInput();

            // 2. State Machine Update (HandleInput + Update)
            _movementStateMachine?.Update();

            // 3. Tick System aktualisieren
            _tickSystem?.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_tickSystem != null)
            {
                _tickSystem.OnTick -= OnFixedTick;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;
            GroundingDetection?.DrawDebugGizmos();
            Locomotion?.DrawDebugGizmos();
        }

        private void OnGUI()
        {
            if (!_showDebugInfo || !Application.isPlaying) return;
            DrawDebugGUI();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            // Get CapsuleCollider
            CapsuleCollider = GetComponent<CapsuleCollider>();
            if (CapsuleCollider == null)
            {
                Debug.LogError($"[PlayerController] FEHLER auf '{gameObject.name}': " +
                    "CapsuleCollider-Komponente fehlt!");
                enabled = false;
                return;
            }

            // Validate CapsuleCollider
            if (CapsuleCollider.radius <= 0f || CapsuleCollider.height <= 0f)
            {
                Debug.LogError($"[PlayerController] FEHLER auf '{gameObject.name}': " +
                    "CapsuleCollider hat ungültige Dimensionen.");
            }

            // Find Input Provider
            if (_inputProviderComponent != null)
            {
                InputProvider = _inputProviderComponent as IMovementInputProvider;
            }
            InputProvider ??= GetComponent<IMovementInputProvider>();

            if (InputProvider == null)
            {
                Debug.LogWarning($"[PlayerController] WARNUNG auf '{gameObject.name}': " +
                    "Kein Input Provider gefunden.");
            }

            // Validate config
            if (_config == null)
            {
                Debug.LogError($"[PlayerController] FEHLER auf '{gameObject.name}': " +
                    "Keine LocomotionConfig zugewiesen!");
                enabled = false;
                return;
            }

            ValidateLocomotionConfig();
        }

        private void ValidateLocomotionConfig()
        {
            if (_config.WalkSpeed <= 0f)
                Debug.LogWarning("[PlayerController] WARNUNG: WalkSpeed sollte > 0 sein.");
            if (_config.Gravity <= 0f)
                Debug.LogWarning("[PlayerController] WARNUNG: Gravity sollte > 0 sein.");
            if (_config.GroundLayers == 0)
                Debug.LogWarning("[PlayerController] WARNUNG: GroundLayers ist leer.");
        }

        private void InitializeTickSystem()
        {
            _tickSystem = new TickSystem(TickSystem.DefaultTickRate);
            _tickSystem.OnTick += OnFixedTick;
        }

        private void InitializeSystems()
        {
            if (_config == null) return;

            // Initialize Ground Detection
            GroundingDetection = new GroundingDetection(
                transform,
                _config,
                CapsuleCollider.radius,
                CapsuleCollider.height
            );

            // Initialize Character Locomotion
            Locomotion = new CharacterLocomotion(
                transform,
                CapsuleCollider,
                _config,
                GroundingDetection,
                _skinWidth
            );
        }

        private void InitializeStateMachine()
        {
            _movementStateMachine = new PlayerMovementStateMachine(this);
            _movementStateMachine.Initialize();
        }

        #endregion

        #region Update Loop

        /// <summary>
        /// Liest Input und schreibt in ReusableData.
        /// </summary>
        private void UpdateInput()
        {
            if (InputProvider == null || ReusableData == null) return;

            ReusableData.MoveInput = InputProvider.MoveInput;
            ReusableData.JumpPressed = InputProvider.JumpPressed;
            ReusableData.JumpHeld = InputProvider.JumpHeld;
            ReusableData.SprintHeld = InputProvider.SprintHeld;
            ReusableData.DashPressed = InputProvider.DashPressed;
        }

        /// <summary>
        /// Fixed Tick - Physics Update.
        /// </summary>
        private void OnFixedTick(int tick, float deltaTime)
        {
            if (ReusableData == null) return;

            // Update Tick in ReusableData
            ReusableData.CurrentTick = tick;

            // Update IsGrounded in ReusableData
            ReusableData.IsGrounded = GroundingDetection?.IsGrounded ?? false;
            ReusableData.IsSliding = Locomotion?.IsSliding ?? false;

            // State Machine Physics Update
            _movementStateMachine?.PhysicsUpdate(deltaTime);

            // Apply Movement
            ApplyMovement(deltaTime);
        }

        /// <summary>
        /// Wendet Bewegung über Locomotion an.
        /// </summary>
        private void ApplyMovement(float deltaTime)
        {
            if (Locomotion == null || ReusableData == null) return;

            // Create locomotion input from ReusableData
            var input = new LocomotionInput
            {
                MoveDirection = ReusableData.MoveInput,
                LookDirection = GetCameraForward(),
                IsSprinting = ReusableData.SprintHeld,
                VerticalVelocity = ReusableData.VerticalVelocity,
                StepDetectionEnabled = ReusableData.StepDetectionEnabled
            };

            // Simulate locomotion
            Locomotion.Simulate(input, deltaTime);

            // Sync velocities back to ReusableData
            ReusableData.VerticalVelocity = Locomotion.VerticalVelocity;
            ReusableData.HorizontalVelocity = Locomotion.HorizontalVelocity;
        }

        /// <summary>
        /// Ermittelt die Forward-Richtung der Kamera.
        /// </summary>
        private Vector3 GetCameraForward()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 cameraForward = mainCamera.transform.forward;
                cameraForward.y = 0f;
                if (cameraForward.sqrMagnitude > 0.01f)
                {
                    return cameraForward.normalized;
                }
            }
            return transform.forward;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Setzt den Character auf eine Position.
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            Locomotion?.Motor?.SetPosition(position);
        }

        /// <summary>
        /// Setzt die Geschwindigkeit zurück.
        /// </summary>
        public void ResetVelocity()
        {
            ReusableData?.ResetMovementData();
            Locomotion?.StopMovement();
        }

        #endregion

        #region Debug

        private void DrawDebugGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 350, 250));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"<b>PlayerController (Genshin Pattern)</b>");
            GUILayout.Label($"State: {CurrentStateName}");
            GUILayout.Label($"Grounded: {IsGrounded}");
            GUILayout.Label($"Sliding: {IsSliding}");
            GUILayout.Label($"Velocity: {Velocity:F2}");

            if (ReusableData != null)
            {
                GUILayout.Label($"H-Velocity: {ReusableData.HorizontalVelocity.magnitude:F2}");
                GUILayout.Label($"V-Velocity: {ReusableData.VerticalVelocity:F2}");
            }

            GUILayout.Label($"Tick: {CurrentTick}");

            if (GroundingDetection != null)
            {
                var gi = GroundingDetection.GroundInfo;
                GUILayout.Label($"Slope: {gi.SlopeAngle:F1}° ({(gi.IsWalkable ? "walkable" : "too steep")})");
            }

            GUILayout.Label($"<i>CSP-Ready</i>");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}
