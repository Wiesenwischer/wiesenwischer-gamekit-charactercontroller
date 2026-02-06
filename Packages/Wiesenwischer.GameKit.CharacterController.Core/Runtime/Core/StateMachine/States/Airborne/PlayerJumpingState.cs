using UnityEngine;
using Wiesenwischer.GameKit.CharacterController.Core.Locomotion.Modules;

namespace Wiesenwischer.GameKit.CharacterController.Core.StateMachine.States
{
    /// <summary>
    /// State während der Character nach oben springt.
    /// Nutzt JumpModule für Berechnungen.
    /// </summary>
    public class PlayerJumpingState : PlayerAirborneState
    {
        private readonly JumpModule _jumpModule = new JumpModule();
        private const float CeilingCheckDistance = 0.1f;

        public override string StateName => "Jumping";

        public PlayerJumpingState(PlayerMovementStateMachine stateMachine) : base(stateMachine)
        {
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            // Berechne und setze Jump Velocity via JumpModule
            ReusableData.VerticalVelocity = _jumpModule.CalculateJumpVelocity(
                Config.JumpHeight,
                Config.Gravity);
            ReusableData.JumpButtonReleased = false;
        }

        protected override void OnHandleInput()
        {
            // Variable Jump: Nur wenn aktiviert
            if (!Config.UseVariableJump) return;

            // JumpModule handhabt Variable Jump Logik
            var (velocity, released) = _jumpModule.ApplyVariableJump(
                ReusableData.VerticalVelocity,
                ReusableData.JumpHeld,
                ReusableData.JumpButtonReleased);

            ReusableData.VerticalVelocity = velocity;
            ReusableData.JumpButtonReleased = released;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Transition zu Falling wenn wir anfangen zu fallen
            if (_jumpModule.IsFalling(ReusableData.VerticalVelocity))
            {
                ChangeState(stateMachine.FallingState);
                return;
            }

            // Ceiling Detection via JumpModule
            var motor = stateMachine.Player.CharacterMotor;
            if (stateTime > 0.05f && _jumpModule.CheckCeiling(motor, CeilingCheckDistance, Config.GroundLayers))
            {
                ReusableData.VerticalVelocity = 0f;
                ChangeState(stateMachine.FallingState);
            }
        }
    }
}
