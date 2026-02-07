using UnityEngine;

namespace Wiesenwischer.GameKit.CharacterController.Core.StateMachine.States
{
    /// <summary>
    /// State während der Character fällt.
    /// Einfach: Wenn grounded → Landing/Jump.
    /// </summary>
    public class PlayerFallingState : PlayerAirborneState
    {
        public override string StateName => "Falling";

        private bool _jumpBuffered;
        private float _jumpBufferTimer;

        public PlayerFallingState(PlayerMovementStateMachine stateMachine) : base(stateMachine)
        {
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            _jumpBuffered = false;
            _jumpBufferTimer = 0f;
        }

        protected override void OnHandleInput()
        {
            // Jump Buffer: Speichere Jump-Input
            if (ReusableData.JumpPressed)
            {
                _jumpBuffered = true;
                _jumpBufferTimer = Config.JumpBufferTime;
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Jump Buffer Timer
            if (_jumpBuffered)
            {
                _jumpBufferTimer -= Time.deltaTime;
                if (_jumpBufferTimer <= 0f)
                {
                    _jumpBuffered = false;
                }
            }

            // Berechne Fallhöhe
            float fallDistance = ReusableData.LastGroundedY - Player.transform.position.y;

            // Gelandet ODER kleiner Drop (Treppen)?
            // Bei kleinen Drops (< MaxStepHeight) behandeln wir das als "noch geerdet"
            bool shouldLand = ReusableData.IsGrounded || (fallDistance < Config.MaxStepHeight && fallDistance >= 0f);

            if (shouldLand)
            {
                // Landing-Klassifikation über Fall-Distanz statt akkumulierte VerticalVelocity.
                // VerticalVelocity ist durch Erkennungs-Latenz aufgebläht (Gravity wird 1-2 Ticks
                // nach tatsächlichem Bodenkontakt weiter angewendet).
                // Umrechnung in äquivalente Geschwindigkeit: v = sqrt(2*g*d)
                float effectiveFallDistance = Mathf.Max(0f, fallDistance);
                float landingSpeed = Mathf.Sqrt(2f * Config.Gravity * effectiveFallDistance);
                ReusableData.LandingVelocity = -landingSpeed;

                if (_jumpBuffered)
                {
                    ReusableData.JumpWasReleased = true;
                    ReusableData.JumpPressed = true;
                    ChangeState(stateMachine.JumpingState);
                }
                else if (landingSpeed >= Config.HardLandingThreshold)
                {
                    ChangeState(stateMachine.HardLandingState);
                }
                else
                {
                    ChangeState(stateMachine.SoftLandingState);
                }
            }
        }
    }
}
