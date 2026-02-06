using UnityEngine;

namespace Wiesenwischer.GameKit.CharacterController.Core.Locomotion.Modules
{
    /// <summary>
    /// Modul für Slope-Handling und Sliding-Berechnungen.
    /// Handhabt Bewegung auf Schrägen und Rutschen auf zu steilen Hängen.
    /// </summary>
    public class SlopeModule
    {
        /// <summary>
        /// Prüft ob der Character auf dem aktuellen Slope rutschen sollte.
        /// </summary>
        /// <param name="slopeAngle">Aktueller Slope-Winkel in Grad</param>
        /// <param name="maxSlopeAngle">Maximaler begehbarer Winkel</param>
        /// <returns>True wenn Character rutschen sollte</returns>
        public bool ShouldSlide(float slopeAngle, float maxSlopeAngle)
        {
            return slopeAngle > maxSlopeAngle;
        }

        /// <summary>
        /// Berechnet die Rutsch-Velocity basierend auf Slope-Normale.
        /// </summary>
        /// <param name="slopeNormal">Normale der Slope-Oberfläche</param>
        /// <param name="slideSpeed">Basis-Rutschgeschwindigkeit</param>
        /// <param name="slopeAngle">Aktueller Slope-Winkel (beeinflusst Geschwindigkeit)</param>
        /// <returns>Velocity-Vektor für das Rutschen</returns>
        public Vector3 CalculateSlideVelocity(Vector3 slopeNormal, float slideSpeed, float slopeAngle)
        {
            // Rutsch-Richtung: Projektion von "down" auf die Slope-Oberfläche
            Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, slopeNormal).normalized;

            // Geschwindigkeit skaliert mit Steilheit (steilere Slopes = schnelleres Rutschen)
            float angleMultiplier = Mathf.Clamp01(slopeAngle / 90f);
            float finalSpeed = slideSpeed * angleMultiplier;

            return slideDirection * finalSpeed;
        }

        /// <summary>
        /// Projiziert eine Bewegungsrichtung auf die Slope-Oberfläche.
        /// Ermöglicht flüssige Bewegung auf Schrägen.
        /// </summary>
        /// <param name="velocity">Original-Velocity</param>
        /// <param name="slopeNormal">Normale der Slope-Oberfläche</param>
        /// <returns>Auf Slope projizierte Velocity</returns>
        public Vector3 ProjectOnSlope(Vector3 velocity, Vector3 slopeNormal)
        {
            return Vector3.ProjectOnPlane(velocity, slopeNormal);
        }

        /// <summary>
        /// Berechnet die Richtung tangential zur Oberfläche.
        /// </summary>
        /// <param name="direction">Gewünschte Bewegungsrichtung</param>
        /// <param name="surfaceNormal">Normale der Oberfläche</param>
        /// <returns>Tangentiale Richtung</returns>
        public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
        {
            // Kreuzprodukt um Tangente zu finden
            Vector3 right = Vector3.Cross(direction, surfaceNormal);
            return Vector3.Cross(surfaceNormal, right).normalized;
        }

        /// <summary>
        /// Berechnet den Slope-Winkel aus der Normalen.
        /// </summary>
        /// <param name="surfaceNormal">Normale der Oberfläche</param>
        /// <returns>Winkel in Grad (0 = flach, 90 = vertikal)</returns>
        public float CalculateSlopeAngle(Vector3 surfaceNormal)
        {
            return Vector3.Angle(Vector3.up, surfaceNormal);
        }

        /// <summary>
        /// Prüft ob ein Slope begehbar ist.
        /// </summary>
        /// <param name="slopeAngle">Aktueller Slope-Winkel</param>
        /// <param name="maxSlopeAngle">Maximaler begehbarer Winkel</param>
        /// <returns>True wenn begehbar</returns>
        public bool IsWalkable(float slopeAngle, float maxSlopeAngle)
        {
            return slopeAngle <= maxSlopeAngle;
        }

        /// <summary>
        /// Berechnet einen Geschwindigkeits-Multiplikator für Bewegung auf Slopes.
        /// Bergauf = langsamer, Bergab = schneller.
        /// </summary>
        /// <param name="moveDirection">Bewegungsrichtung (normalisiert)</param>
        /// <param name="slopeNormal">Slope-Normale</param>
        /// <param name="uphillPenalty">Faktor für Bergauf-Verlangsamung (0-1)</param>
        /// <param name="downhillBonus">Faktor für Bergab-Beschleunigung (1+)</param>
        /// <returns>Speed-Multiplikator</returns>
        public float CalculateSlopeSpeedMultiplier(
            Vector3 moveDirection,
            Vector3 slopeNormal,
            float uphillPenalty = 0.7f,
            float downhillBonus = 1.2f)
        {
            // Dot-Produkt zwischen Bewegung und Slope-"Up"
            Vector3 slopeUp = Vector3.Cross(Vector3.Cross(Vector3.up, slopeNormal), slopeNormal).normalized;
            float dot = Vector3.Dot(moveDirection.normalized, slopeUp);

            // Negativ = bergauf, Positiv = bergab
            if (dot < 0)
            {
                return Mathf.Lerp(1f, uphillPenalty, -dot);
            }
            else
            {
                return Mathf.Lerp(1f, downhillBonus, dot);
            }
        }
    }
}
