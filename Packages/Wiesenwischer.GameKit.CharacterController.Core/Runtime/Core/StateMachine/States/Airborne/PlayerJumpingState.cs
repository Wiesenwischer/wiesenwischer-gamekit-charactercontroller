using UnityEngine;

namespace Wiesenwischer.GameKit.CharacterController.Core.StateMachine.States
{
    /// <summary>
    /// State während der Character nach oben springt.
    /// Einfach: Variable Jump + Transition zu Falling.
    /// </summary>
    public class PlayerJumpingState : PlayerAirborneState
    {
        public override string StateName => "Jumping";

        public PlayerJumpingState(PlayerMovementStateMachine stateMachine) : base(stateMachine)
        {
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            // Berechne und setze Jump Velocity
            ReusableData.VerticalVelocity = CalculateJumpVelocity();
            ReusableData.JumpButtonReleased = false;
        }

        protected override void OnHandleInput()
        {
            // Variable Jump: Nur wenn aktiviert
            if (!Config.UseVariableJump) return;

            if (!ReusableData.JumpHeld && !ReusableData.JumpButtonReleased)
            {
                ReusableData.JumpButtonReleased = true;

                // Reduziere Velocity wenn Button früh losgelassen
                if (ReusableData.VerticalVelocity > 0)
                {
                    ReusableData.VerticalVelocity *= 0.5f;
                }
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Transition zu Falling wenn wir anfangen zu fallen
            if (ReusableData.VerticalVelocity <= 0)
            {
                ChangeState(stateMachine.FallingState);
                return;
            }

            // Wenn wir eine Decke getroffen haben
            if (stateTime > 0.05f && HitCeiling())
            {
                ReusableData.VerticalVelocity = 0f;
                ChangeState(stateMachine.FallingState);
            }
        }

        private bool HitCeiling()
        {
            var motor = stateMachine.Player.KinematicMotor;
            return motor != null && (motor.CollisionFlags & CollisionFlags.Above) != 0;
        }

        private float CalculateJumpVelocity()
        {
            return Mathf.Sqrt(2f * Config.Gravity * Config.JumpHeight);
        }
    }
}
