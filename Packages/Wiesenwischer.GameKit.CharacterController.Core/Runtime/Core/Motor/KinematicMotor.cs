using UnityEngine;
using Wiesenwischer.GameKit.CharacterController.Core.StateMachine;

namespace Wiesenwischer.GameKit.CharacterController.Core.Motor
{
    /// <summary>
    /// Kinematischer Motor mit Step Detection.
    /// Bei Kollision wird geprüft ob es sich um eine übersteigbare Stufe handelt.
    /// </summary>
    public class KinematicMotor
    {
        private readonly Transform _transform;
        private readonly CapsuleCollider _capsule;
        private readonly ILocomotionConfig _config;

        private readonly float _radius;
        private readonly float _height;
        private readonly float _skinWidth;

        private const float MinMoveDistance = 0.001f;

        private CollisionFlags _collisionFlags;

        public KinematicMotor(Transform transform, CapsuleCollider capsule, ILocomotionConfig config, float skinWidth = 0.02f)
        {
            _transform = transform ?? throw new System.ArgumentNullException(nameof(transform));
            _capsule = capsule ?? throw new System.ArgumentNullException(nameof(capsule));
            _config = config ?? throw new System.ArgumentNullException(nameof(config));

            _skinWidth = Mathf.Max(0.001f, skinWidth);
            _radius = capsule.radius;
            _height = capsule.height;
        }

        public Vector3 Position => _transform.position;
        public CollisionFlags CollisionFlags => _collisionFlags;
        public float Radius => _radius;
        public float Height => _height;

        /// <summary>
        /// Bewegt den Character mit Kollisionserkennung und Step Detection.
        /// </summary>
        public CollisionFlags Move(Vector3 motion, bool enableStepDetection = false)
        {
            _collisionFlags = CollisionFlags.None;

            if (motion.sqrMagnitude < MinMoveDistance * MinMoveDistance)
                return _collisionFlags;

            Vector3 position = _transform.position;

            // Trenne horizontale und vertikale Bewegung
            Vector3 horizontalMotion = new Vector3(motion.x, 0, motion.z);
            float verticalMotion = motion.y;

            // 1. Horizontale Bewegung mit Step Detection
            if (horizontalMotion.sqrMagnitude > MinMoveDistance * MinMoveDistance)
            {
                position = MoveHorizontalWithSteps(position, horizontalMotion, enableStepDetection);
            }

            // 2. Vertikale Bewegung
            if (Mathf.Abs(verticalMotion) > MinMoveDistance)
            {
                position = MoveVertical(position, verticalMotion);
            }

            _transform.position = position;
            return _collisionFlags;
        }

        private Vector3 MoveHorizontalWithSteps(Vector3 position, Vector3 horizontalMotion, bool enableStepDetection)
        {
            Vector3 direction = horizontalMotion.normalized;
            float distance = horizontalMotion.magnitude;

            // Horizontale Bewegung mit Kollision
            if (distance > MinMoveDistance)
            {
                if (CapsuleCast(position, direction, distance, out RaycastHit hit))
                {
                    float moveDistance = Mathf.Max(0, hit.distance - _skinWidth);

                    // Step Detection: Prüfe ob die Kollision eine Stufe ist
                    if (enableStepDetection && moveDistance < 0.01f)
                    {
                        float stepUp = TryStepUp(position, direction, hit);
                        if (stepUp > 0.01f)
                        {
                            position.y += stepUp;

                            // Nach dem Lift nochmal horizontal bewegen
                            if (!CapsuleCast(position, direction, distance, out _))
                            {
                                position += direction * distance;
                                return position;
                            }
                        }
                    }

                    position += direction * moveDistance;

                    // Setze Collision Flags
                    float angle = Vector3.Angle(hit.normal, Vector3.up);
                    if (angle < 45f)
                        _collisionFlags |= CollisionFlags.Below;
                    else if (angle > 135f)
                        _collisionFlags |= CollisionFlags.Above;
                    else
                        _collisionFlags |= CollisionFlags.Sides;

                    // Slide entlang der Oberfläche
                    float remainingDist = distance - moveDistance;
                    if (remainingDist > MinMoveDistance && angle >= 45f && angle <= 135f)
                    {
                        Vector3 slideDir = Vector3.ProjectOnPlane(direction, hit.normal).normalized;
                        if (slideDir.sqrMagnitude > 0.01f)
                        {
                            if (!CapsuleCast(position, slideDir, remainingDist, out _))
                            {
                                position += slideDir * remainingDist;
                            }
                        }
                    }
                }
                else
                {
                    // Freie Bewegung
                    position += direction * distance;
                }
            }

            return position;
        }

        /// <summary>
        /// Versucht über eine Kollision zu steigen.
        /// Gibt die Lift-Höhe zurück, oder 0 wenn kein Step-Up möglich.
        /// </summary>
        private float TryStepUp(Vector3 position, Vector3 direction, RaycastHit obstacleHit)
        {
            float maxStepHeight = _config.MaxStepHeight;

            // Hinweis: Horizontaler Clearance-Check entfernt - bei Treppen blockiert die nächste Stufe
            // den Check immer. Stattdessen verlassen wir uns auf die Kopffreiheits-Prüfung (vertikal).

            // 2. Finde die Stufenoberfläche mit Raycast von oben - direkt über dem Hindernis
            Vector3 topDownOrigin = obstacleHit.point + Vector3.up * maxStepHeight + direction * 0.05f;

            if (!Physics.Raycast(topDownOrigin, Vector3.down, out RaycastHit topHit, maxStepHeight + 0.2f,
                _config.GroundLayers, QueryTriggerInteraction.Ignore))
            {
                Debug.Log("[Step] Keine Oberfläche gefunden");
                return 0f;
            }

            // 3. Berechne Stufenhöhe relativ zur Character-Position (Fuß-Level)
            float stepHeight = topHit.point.y - position.y;

            if (stepHeight < 0.02f || stepHeight > maxStepHeight)
            {
                return 0f;
            }

            // 4. Prüfe ob Oberfläche begehbar (nicht zu steil)
            float slopeAngle = Vector3.Angle(topHit.normal, Vector3.up);
            if (slopeAngle > _config.MaxSlopeAngle)
            {
                Debug.Log($"[Step] Zu steil: {slopeAngle:F1}°");
                return 0f;
            }

            // 5. Prüfe Kopffreiheit
            Vector3 headroomOrigin = position + _capsule.center + Vector3.up * (_height * 0.5f);
            // Lift = Ziel-Oberfläche minus aktuelle Position + Clearance
            float liftAmount = (topHit.point.y - position.y) + 0.05f;

            if (Physics.Raycast(headroomOrigin, Vector3.up, liftAmount, _config.GroundLayers, QueryTriggerInteraction.Ignore))
            {
                Debug.Log("[Step] Keine Kopffreiheit");
                return 0f;
            }

            return liftAmount;
        }

        private Vector3 MoveVertical(Vector3 position, float verticalMotion)
        {
            Vector3 direction = verticalMotion > 0 ? Vector3.up : Vector3.down;
            float distance = Mathf.Abs(verticalMotion);

            if (CapsuleCast(position, direction, distance, out RaycastHit hit))
            {
                float moveDistance = Mathf.Max(0, hit.distance - _skinWidth);
                position += direction * moveDistance;

                if (verticalMotion > 0)
                    _collisionFlags |= CollisionFlags.Above;
                else
                    _collisionFlags |= CollisionFlags.Below;
            }
            else
            {
                position += direction * distance;
            }

            return position;
        }

        private bool CapsuleCast(Vector3 position, Vector3 direction, float distance, out RaycastHit hit)
        {
            GetCapsulePoints(position, out Vector3 p1, out Vector3 p2);
            float castRadius = _radius - _skinWidth;

            return Physics.CapsuleCast(
                p1, p2, castRadius,
                direction, out hit,
                distance + _skinWidth,
                _config.GroundLayers,
                QueryTriggerInteraction.Ignore);
        }

        private void GetCapsulePoints(Vector3 position, out Vector3 point1, out Vector3 point2)
        {
            Vector3 center = position + _capsule.center;
            float halfHeight = (_height * 0.5f) - _radius;
            point1 = center + Vector3.up * halfHeight;
            point2 = center - Vector3.up * halfHeight;
        }

        public void SetPosition(Vector3 position)
        {
            _transform.position = position;
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            _transform.position = position;
            _transform.rotation = rotation;
        }

        public void DrawDebugGizmos()
        {
#if UNITY_EDITOR
            if (_transform == null) return;

            GetCapsulePoints(_transform.position, out Vector3 p1, out Vector3 p2);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p1, _radius);
            Gizmos.DrawWireSphere(p2, _radius);
#endif
        }
    }
}
