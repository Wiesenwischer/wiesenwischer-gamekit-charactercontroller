using UnityEngine;

namespace Wiesenwischer.GameKit.CharacterController.Core.StateMachine.States
{
    /// <summary>
    /// Basis-State für alle Moving-States (Walking, Running, Sprinting).
    /// Handhabt gemeinsame Bewegungslogik.
    /// </summary>
    public class PlayerMovingState : PlayerGroundedState
    {
        public override string StateName => "Moving";

        public PlayerMovingState(PlayerMovementStateMachine stateMachine) : base(stateMachine)
        {
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Transition zu Idle wenn kein Input
            if (!HasMovementInput())
            {
                ChangeState(stateMachine.IdlingState);
                return;
            }
        }
    }
}
