using UnityEngine;

namespace Wiesenwischer.GameKit.CharacterController.Core.StateMachine.States
{
    /// <summary>
    /// State nach der Landung.
    /// Spielt Landungsanimation ab und verhindert kurzzeitig Bewegung.
    /// Die Recovery-Zeit hängt von der Aufprallgeschwindigkeit ab.
    /// </summary>
    public class PlayerLandingState : PlayerGroundedState
    {
        public override string StateName => "Landing";

        /// <summary>Verbleibende Landungs-Recovery-Zeit.</summary>
        private float _landingRecoveryTimer;

        /// <summary>Ob ein Jump während der Landung gebuffert wurde.</summary>
        private bool _jumpBuffered;

        public PlayerLandingState(PlayerMovementStateMachine stateMachine) : base(stateMachine)
        {
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            _jumpBuffered = false;

            // Berechne Recovery-Zeit basierend auf Aufprallgeschwindigkeit
            float landingSpeed = Mathf.Abs(ReusableData.LandingVelocity);

            if (landingSpeed < Config.SoftLandingThreshold)
            {
                // Weiche Landung - fast keine Verzögerung
                _landingRecoveryTimer = 0f;
            }
            else if (landingSpeed < Config.HardLandingThreshold)
            {
                // Normale Landung - interpoliere zwischen soft und hard
                float t = (landingSpeed - Config.SoftLandingThreshold) / (Config.HardLandingThreshold - Config.SoftLandingThreshold);
                _landingRecoveryTimer = Mathf.Lerp(Config.SoftLandingDuration, Config.HardLandingDuration, t);
            }
            else
            {
                // Harte Landung - maximale Recovery
                _landingRecoveryTimer = Config.HardLandingDuration;
                // Hier könnte später Falldamage angewendet werden
            }

            // Stoppe horizontale Bewegung bei harter Landung
            if (landingSpeed >= Config.HardLandingThreshold)
            {
                ReusableData.HorizontalVelocity = Vector3.zero;
            }

            // Setze Geschwindigkeitsmodifier auf 0 während Landing
            ReusableData.MovementSpeedModifier = 0f;
        }

        protected override void OnHandleInput()
        {
            base.OnHandleInput();

            // Buffere Jump während Landing für responsive Controls
            if (ReusableData.JumpPressed)
            {
                _jumpBuffered = true;
            }
        }

        protected override void OnUpdate()
        {
            // Nicht base.OnUpdate() aufrufen - wir wollen Coyote Time Check überspringen

            _landingRecoveryTimer -= Time.deltaTime;

            // Wenn Recovery vorbei, wechsle zum nächsten State
            if (_landingRecoveryTimer <= 0f)
            {
                // Gebufferter Jump?
                if (_jumpBuffered && ReusableData.JumpWasReleased)
                {
                    ReusableData.JumpPressed = true;
                    OnJump();
                    return;
                }

                // Hat der Spieler Movement Input?
                if (ReusableData.MoveInput.sqrMagnitude > 0.01f)
                {
                    // Wechsle zu entsprechendem Movement State
                    if (ReusableData.SprintHeld)
                    {
                        ChangeState(stateMachine.SprintingState);
                    }
                    else if (ReusableData.ShouldWalk)
                    {
                        ChangeState(stateMachine.WalkingState);
                    }
                    else
                    {
                        ChangeState(stateMachine.RunningState);
                    }
                }
                else
                {
                    ChangeState(stateMachine.IdlingState);
                }
            }
        }

        protected override void OnExit()
        {
            base.OnExit();

            // Setze Speed Modifier zurück
            ReusableData.MovementSpeedModifier = 1f;
        }
    }
}
