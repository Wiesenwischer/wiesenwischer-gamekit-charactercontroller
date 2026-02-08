using UnityEngine;

namespace Wiesenwischer.GameKit.CharacterController.Core.StateMachine.States
{
    /// <summary>
    /// State während der Character fällt.
    /// Wenn grounded → SoftLanding oder HardLanding basierend auf Fallhöhe.
    /// </summary>
    public class PlayerFallingState : PlayerAirborneState
    {
        public override string StateName => "Falling";

        public PlayerFallingState(PlayerMovementStateMachine stateMachine) : base(stateMachine)
        {
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Berechne Fallhöhe
            float fallDistance = ReusableData.LastGroundedY - Player.transform.position.y;

            // Landing: Motor ist Single Source of Truth für Ground-State
            if (ReusableData.IsGrounded)
            {
                // Landing-Klassifikation über Fall-Distanz statt akkumulierte VerticalVelocity.
                // VerticalVelocity ist durch Erkennungs-Latenz aufgebläht (Gravity wird 1-2 Ticks
                // nach tatsächlichem Bodenkontakt weiter angewendet).
                // Umrechnung in äquivalente Geschwindigkeit: v = sqrt(2*g*d)
                float effectiveFallDistance = Mathf.Max(0f, fallDistance);
                float landingSpeed = Mathf.Sqrt(2f * Config.Gravity * effectiveFallDistance);
                ReusableData.LandingVelocity = -landingSpeed;

                if (landingSpeed >= Config.HardLandingThreshold)
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
