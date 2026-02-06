using UnityEngine;
using Wiesenwischer.GameKit.CharacterController.Core.Motor;

namespace Wiesenwischer.GameKit.CharacterController.Core.Locomotion.Modules
{
    /// <summary>
    /// Modul für Ground Detection und Motor.GroundingStatus Interpretation.
    /// Kapselt alle Zugriffe auf den Grounding-Status für bessere Testbarkeit.
    /// </summary>
    public class GroundDetectionModule
    {
        /// <summary>
        /// Erstellt GroundInfo aus dem aktuellen Motor-Grounding-Status.
        /// </summary>
        public GroundInfo GetGroundInfo(CharacterMotor motor)
        {
            var grounding = motor.GroundingStatus;

            return new GroundInfo
            {
                IsGrounded = grounding.FoundAnyGround,
                Point = grounding.GroundPoint,
                Normal = grounding.GroundNormal,
                SlopeAngle = Vector3.Angle(Vector3.up, grounding.GroundNormal),
                Distance = 0f,
                IsWalkable = grounding.IsStableOnGround,
                StabilityReport = CreateStabilityReport(grounding)
            };
        }

        /// <summary>
        /// Prüft ob der Character stabil am Boden steht.
        /// </summary>
        public bool IsStableOnGround(CharacterMotor motor)
        {
            return motor.GroundingStatus.IsStableOnGround;
        }

        /// <summary>
        /// Prüft ob der Character irgendwo Boden gefunden hat (stabil oder instabil).
        /// </summary>
        public bool FoundAnyGround(CharacterMotor motor)
        {
            return motor.GroundingStatus.FoundAnyGround;
        }

        /// <summary>
        /// Prüft ob der Character an einer Kante steht (Ledge Detection).
        /// </summary>
        public bool IsOnLedge(CharacterMotor motor)
        {
            return motor.GroundingStatus.SnappingPrevented;
        }

        /// <summary>
        /// Gibt den aktuellen Slope-Winkel zurück.
        /// </summary>
        public float GetSlopeAngle(CharacterMotor motor)
        {
            var grounding = motor.GroundingStatus;
            if (!grounding.FoundAnyGround)
                return 0f;

            return Vector3.Angle(Vector3.up, grounding.GroundNormal);
        }

        /// <summary>
        /// Gibt die Boden-Normale zurück.
        /// </summary>
        public Vector3 GetGroundNormal(CharacterMotor motor)
        {
            var grounding = motor.GroundingStatus;
            return grounding.FoundAnyGround ? grounding.GroundNormal : Vector3.up;
        }

        /// <summary>
        /// Prüft ob der aktuelle Slope zu steil zum Laufen ist.
        /// </summary>
        public bool IsSlopeTooSteep(CharacterMotor motor, float maxSlopeAngle)
        {
            var grounding = motor.GroundingStatus;
            if (!grounding.FoundAnyGround)
                return false;

            float angle = Vector3.Angle(Vector3.up, grounding.GroundNormal);
            return angle > maxSlopeAngle;
        }

        private HitStabilityReport CreateStabilityReport(CharacterGroundingReport grounding)
        {
            return new HitStabilityReport
            {
                IsStable = grounding.IsStableOnGround,
                InnerNormal = grounding.InnerGroundNormal,
                OuterNormal = grounding.OuterGroundNormal,
                FoundInnerNormal = true,
                FoundOuterNormal = true
            };
        }
    }
}
