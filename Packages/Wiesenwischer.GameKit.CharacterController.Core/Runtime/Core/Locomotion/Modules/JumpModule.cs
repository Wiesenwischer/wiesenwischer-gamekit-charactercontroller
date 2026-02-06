using UnityEngine;
using Wiesenwischer.GameKit.CharacterController.Core.Motor;

namespace Wiesenwischer.GameKit.CharacterController.Core.Locomotion.Modules
{
    /// <summary>
    /// Modul für Jump-Berechnungen und Ceiling Detection.
    /// Extrahiert aus PlayerJumpingState für bessere Testbarkeit.
    /// </summary>
    public class JumpModule
    {
        /// <summary>
        /// Standard-Multiplikator für Variable Jump (Button früh losgelassen).
        /// </summary>
        public const float DefaultJumpCutMultiplier = 0.5f;

        /// <summary>
        /// Berechnet die initiale Jump-Velocity basierend auf Sprunghöhe und Gravity.
        /// Formel: v = sqrt(2 * g * h)
        /// </summary>
        /// <param name="jumpHeight">Gewünschte Sprunghöhe in Units</param>
        /// <param name="gravity">Gravity-Wert (positiv)</param>
        /// <returns>Initiale vertikale Velocity für den Sprung</returns>
        public float CalculateJumpVelocity(float jumpHeight, float gravity)
        {
            return Mathf.Sqrt(2f * gravity * jumpHeight);
        }

        /// <summary>
        /// Wendet Variable Jump an - reduziert Velocity wenn Button früh losgelassen.
        /// </summary>
        /// <param name="currentVelocity">Aktuelle vertikale Velocity</param>
        /// <param name="jumpHeld">Wird der Jump-Button noch gehalten?</param>
        /// <param name="wasReleased">Wurde der Button bereits einmal losgelassen?</param>
        /// <param name="cutMultiplier">Multiplikator für Velocity-Reduktion</param>
        /// <returns>Tuple: (neue Velocity, neuer wasReleased Status)</returns>
        public (float velocity, bool released) ApplyVariableJump(
            float currentVelocity,
            bool jumpHeld,
            bool wasReleased,
            float cutMultiplier = DefaultJumpCutMultiplier)
        {
            // Wenn bereits losgelassen oder noch gehalten, nichts tun
            if (wasReleased || jumpHeld)
            {
                return (currentVelocity, wasReleased);
            }

            // Button wurde gerade losgelassen
            float newVelocity = currentVelocity;
            if (currentVelocity > 0)
            {
                newVelocity *= cutMultiplier;
            }

            return (newVelocity, true);
        }

        /// <summary>
        /// Prüft ob der Character eine Decke getroffen hat (Ceiling Detection).
        /// Verwendet SphereCast nach oben.
        /// </summary>
        /// <param name="motor">Der CharacterMotor</param>
        /// <param name="checkDistance">Distanz für den Check</param>
        /// <param name="groundLayers">Layer-Maske für Kollisionen</param>
        /// <returns>True wenn Decke getroffen</returns>
        public bool CheckCeiling(CharacterMotor motor, float checkDistance, LayerMask groundLayers)
        {
            if (motor == null) return false;

            // Origin ist am oberen Rand der Capsule
            Vector3 origin = motor.TransientPosition + Vector3.up * (motor.Height - motor.Radius);

            return Physics.SphereCast(
                origin,
                motor.Radius * 0.9f,
                Vector3.up,
                out _,
                checkDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Prüft ob der Character noch aufwärts bewegt (Jumping) oder bereits fällt.
        /// </summary>
        public bool IsAscending(float verticalVelocity)
        {
            return verticalVelocity > 0;
        }

        /// <summary>
        /// Prüft ob der Character fällt.
        /// </summary>
        public bool IsFalling(float verticalVelocity)
        {
            return verticalVelocity <= 0;
        }
    }
}
