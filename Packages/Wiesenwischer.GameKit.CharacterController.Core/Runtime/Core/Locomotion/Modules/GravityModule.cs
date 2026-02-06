using UnityEngine;

namespace Wiesenwischer.GameKit.CharacterController.Core.Locomotion.Modules
{
    /// <summary>
    /// Modul für Gravity-Berechnung und vertikale Velocity.
    /// Extrahiert aus CharacterLocomotion für bessere Testbarkeit und Wiederverwendbarkeit.
    /// </summary>
    public class GravityModule
    {
        /// <summary>
        /// Kleine negative Velocity die den Character am Boden hält.
        /// Verhindert "Schweben" auf Rampen und bei Ground Snapping.
        /// </summary>
        public const float GroundingVelocity = -2f;

        /// <summary>
        /// Berechnet die vertikale Velocity unter Berücksichtigung von Gravity und Grounding.
        /// </summary>
        /// <param name="currentVelocity">Aktuelle vertikale Velocity (z.B. von Jump)</param>
        /// <param name="isGrounded">Ist der Character stabil am Boden?</param>
        /// <param name="gravity">Gravity-Wert (positiv, wird subtrahiert)</param>
        /// <param name="maxFallSpeed">Maximale Fallgeschwindigkeit (positiv)</param>
        /// <param name="deltaTime">Zeit seit letztem Frame</param>
        /// <returns>Neue vertikale Velocity</returns>
        public float CalculateVerticalVelocity(
            float currentVelocity,
            bool isGrounded,
            float gravity,
            float maxFallSpeed,
            float deltaTime)
        {
            // Wenn grounded und nicht aufwärts bewegt → Grounding-Velocity
            if (isGrounded && currentVelocity <= 0)
            {
                return GroundingVelocity;
            }

            // In der Luft → Gravity anwenden
            float velocity = currentVelocity - gravity * deltaTime;

            // Fallgeschwindigkeit begrenzen
            velocity = Mathf.Max(velocity, -maxFallSpeed);

            return velocity;
        }

        /// <summary>
        /// Wendet Grounding-Velocity an wenn stabil am Boden.
        /// Sollte nach Ground Probing aufgerufen werden.
        /// </summary>
        /// <param name="currentVelocity">Aktuelle vertikale Velocity</param>
        /// <param name="isStableOnGround">Ist der Character stabil am Boden?</param>
        /// <returns>Angepasste Velocity (GroundingVelocity wenn grounded und nicht aufwärts)</returns>
        public float ApplyGroundSnapping(float currentVelocity, bool isStableOnGround)
        {
            if (isStableOnGround && currentVelocity <= 0f)
            {
                return GroundingVelocity;
            }
            return currentVelocity;
        }
    }
}
