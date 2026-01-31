using UnityEngine;
using Wiesenwischer.GameKit.CharacterController.Core.StateMachine;

namespace Wiesenwischer.GameKit.CharacterController.Core.Movement
{
    /// <summary>
    /// System für Boden-Erkennung.
    /// Verwendet Raycast und SphereCast für zuverlässige Ground Detection.
    /// </summary>
    public class GroundingDetection
    {
        private readonly Transform _transform;
        private readonly IMovementConfig _config;
        private readonly float _characterRadius;
        private readonly float _characterHeight;

        // Cached Results
        private GroundInfo _groundInfo;
        private bool _wasGroundedLastFrame;

        // Debug
        private Vector3 _lastRaycastOrigin;
        private Vector3 _lastRaycastHit;

        /// <summary>
        /// Erstellt eine neue GroundingDetection Instanz.
        /// </summary>
        /// <param name="transform">Transform des Characters.</param>
        /// <param name="config">Movement-Konfiguration.</param>
        /// <param name="characterRadius">Radius des CharacterControllers.</param>
        /// <param name="characterHeight">Höhe des CharacterControllers.</param>
        public GroundingDetection(Transform transform, IMovementConfig config, float characterRadius, float characterHeight)
        {
            _transform = transform;
            _config = config;
            _characterRadius = characterRadius;
            _characterHeight = characterHeight;
            _groundInfo = GroundInfo.Empty;
        }

        /// <summary>
        /// Aktuelle Ground-Informationen.
        /// </summary>
        public GroundInfo GroundInfo => _groundInfo;

        /// <summary>
        /// Ob der Character auf dem Boden steht.
        /// </summary>
        public bool IsGrounded => _groundInfo.IsGrounded;

        /// <summary>
        /// Ob der Character gerade gelandet ist (war in der Luft, jetzt auf dem Boden).
        /// </summary>
        public bool JustLanded => _groundInfo.IsGrounded && !_wasGroundedLastFrame;

        /// <summary>
        /// Ob der Character gerade den Boden verlassen hat.
        /// </summary>
        public bool JustLeftGround => !_groundInfo.IsGrounded && _wasGroundedLastFrame;

        /// <summary>
        /// Führt die Ground Detection aus.
        /// Sollte jeden Frame/Tick aufgerufen werden.
        /// </summary>
        public void UpdateGroundCheck()
        {
            _wasGroundedLastFrame = _groundInfo.IsGrounded;

            // Primärer Check: SphereCast für stabilere Detection
            if (PerformSphereCast(out GroundInfo sphereResult))
            {
                _groundInfo = sphereResult;
            }
            // Fallback: Raycast für präzisere Slope-Detection
            else if (PerformRaycast(out GroundInfo rayResult))
            {
                _groundInfo = rayResult;
            }
            else
            {
                _groundInfo = GroundInfo.Empty;
            }
        }

        /// <summary>
        /// Führt die Ground Detection mit angepasster Position aus.
        /// Nützlich für Prediction/Simulation.
        /// </summary>
        public GroundInfo CheckGroundAtPosition(Vector3 position)
        {
            // SphereCast von der gegebenen Position
            Vector3 origin = position + Vector3.up * (_characterRadius + 0.01f);
            float checkDistance = _config.GroundCheckDistance + _characterRadius;

            if (Physics.SphereCast(
                origin,
                _config.GroundCheckRadius,
                Vector3.down,
                out RaycastHit hit,
                checkDistance,
                _config.GroundLayers,
                QueryTriggerInteraction.Ignore))
            {
                return CreateGroundInfo(hit, origin);
            }

            return GroundInfo.Empty;
        }

        #region Detection Methods

        private bool PerformSphereCast(out GroundInfo result)
        {
            // Origin: Leicht über dem Boden, in der Mitte des Characters
            Vector3 origin = _transform.position + Vector3.up * (_characterRadius + 0.01f);
            float checkDistance = _config.GroundCheckDistance + _characterRadius;

            _lastRaycastOrigin = origin;

            if (Physics.SphereCast(
                origin,
                _config.GroundCheckRadius,
                Vector3.down,
                out RaycastHit hit,
                checkDistance,
                _config.GroundLayers,
                QueryTriggerInteraction.Ignore))
            {
                _lastRaycastHit = hit.point;
                result = CreateGroundInfo(hit, origin);
                return true;
            }

            result = GroundInfo.Empty;
            return false;
        }

        private bool PerformRaycast(out GroundInfo result)
        {
            // Mehrere Raycasts für bessere Coverage
            Vector3 center = _transform.position + Vector3.up * 0.1f;
            float checkDistance = _config.GroundCheckDistance + 0.1f;

            // Center Raycast
            if (Physics.Raycast(
                center,
                Vector3.down,
                out RaycastHit hit,
                checkDistance,
                _config.GroundLayers,
                QueryTriggerInteraction.Ignore))
            {
                result = CreateGroundInfo(hit, center);
                return true;
            }

            // Offset Raycasts (vorne, hinten, links, rechts)
            float offset = _characterRadius * 0.5f;
            Vector3[] offsets = new Vector3[]
            {
                _transform.forward * offset,
                -_transform.forward * offset,
                _transform.right * offset,
                -_transform.right * offset
            };

            foreach (var o in offsets)
            {
                Vector3 origin = center + o;
                if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out hit,
                    checkDistance,
                    _config.GroundLayers,
                    QueryTriggerInteraction.Ignore))
                {
                    result = CreateGroundInfo(hit, origin);
                    return true;
                }
            }

            result = GroundInfo.Empty;
            return false;
        }

        private GroundInfo CreateGroundInfo(RaycastHit hit, Vector3 origin)
        {
            // Berechne Slope Angle
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // Berechne Distanz zum Boden
            float distance = hit.distance;

            // Prüfe ob begehbar
            bool isWalkable = slopeAngle <= _config.MaxSlopeAngle;

            return new GroundInfo
            {
                IsGrounded = true,
                Point = hit.point,
                Normal = hit.normal,
                SlopeAngle = slopeAngle,
                Distance = distance,
                IsWalkable = isWalkable
            };
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Berechnet die Bewegungsrichtung auf einer Slope.
        /// Projiziert die Bewegungsrichtung auf die Oberfläche.
        /// </summary>
        public Vector3 GetSlopeDirection(Vector3 moveDirection)
        {
            if (!_groundInfo.IsGrounded)
            {
                return moveDirection;
            }

            // Projiziere Bewegungsrichtung auf die Slope
            Vector3 slopeDirection = Vector3.ProjectOnPlane(moveDirection, _groundInfo.Normal).normalized;

            // Wenn wir bergauf gehen und der Slope zu steil ist, stoppe
            if (!_groundInfo.IsWalkable && Vector3.Dot(slopeDirection, Vector3.up) > 0)
            {
                // Entferne die aufwärts-Komponente
                slopeDirection = Vector3.ProjectOnPlane(slopeDirection, Vector3.up).normalized;
            }

            return slopeDirection;
        }

        /// <summary>
        /// Prüft ob eine Stufe vor dem Character ist und gibt die Höhe zurück.
        /// </summary>
        public bool CheckForStep(Vector3 moveDirection, out float stepHeight)
        {
            stepHeight = 0f;

            if (!_groundInfo.IsGrounded || moveDirection.sqrMagnitude < 0.01f)
            {
                return false;
            }

            Vector3 checkOrigin = _transform.position + Vector3.up * 0.05f;
            Vector3 checkDirection = moveDirection.normalized;

            // Erster Raycast: Prüfe ob ein Hindernis vor uns ist
            if (!Physics.Raycast(
                checkOrigin,
                checkDirection,
                out RaycastHit obstacleHit,
                _characterRadius + 0.1f,
                _config.GroundLayers,
                QueryTriggerInteraction.Ignore))
            {
                return false; // Kein Hindernis
            }

            // Zweiter Raycast: Prüfe von oben, wie hoch die Stufe ist
            Vector3 topOrigin = checkOrigin + Vector3.up * _config.MaxStepHeight + checkDirection * (_characterRadius + 0.1f);

            if (Physics.Raycast(
                topOrigin,
                Vector3.down,
                out RaycastHit topHit,
                _config.MaxStepHeight + 0.1f,
                _config.GroundLayers,
                QueryTriggerInteraction.Ignore))
            {
                stepHeight = topHit.point.y - _transform.position.y;

                // Prüfe ob die Stufe begehbar ist
                float topSlopeAngle = Vector3.Angle(topHit.normal, Vector3.up);

                return stepHeight > 0.01f &&
                       stepHeight <= _config.MaxStepHeight &&
                       topSlopeAngle <= _config.MaxSlopeAngle;
            }

            return false;
        }

        /// <summary>
        /// Gibt den nächsten Punkt auf dem Boden unter einer Position zurück.
        /// </summary>
        public bool GetGroundPoint(Vector3 position, out Vector3 groundPoint)
        {
            groundPoint = position;

            if (Physics.Raycast(
                position + Vector3.up * 0.5f,
                Vector3.down,
                out RaycastHit hit,
                10f,
                _config.GroundLayers,
                QueryTriggerInteraction.Ignore))
            {
                groundPoint = hit.point;
                return true;
            }

            return false;
        }

        #endregion

        #region Debug

        /// <summary>
        /// Zeichnet Debug-Gizmos für die Ground Detection.
        /// Sollte in OnDrawGizmos aufgerufen werden.
        /// </summary>
        public void DrawDebugGizmos()
        {
#if UNITY_EDITOR
            if (_transform == null) return;

            // Ground Check Sphere
            Vector3 origin = _transform.position + Vector3.up * (_characterRadius + 0.01f);
            float checkDistance = _config.GroundCheckDistance + _characterRadius;
            Vector3 endPoint = origin + Vector3.down * checkDistance;

            // Farbe basierend auf Ground Status
            Gizmos.color = _groundInfo.IsGrounded
                ? (_groundInfo.IsWalkable ? Color.green : Color.yellow)
                : Color.red;

            // Zeichne Sphere am Start und Ende
            Gizmos.DrawWireSphere(origin, _config.GroundCheckRadius);
            Gizmos.DrawWireSphere(endPoint, _config.GroundCheckRadius);

            // Zeichne Linie zwischen den Spheres
            Gizmos.DrawLine(origin, endPoint);

            // Zeichne Hit Point
            if (_groundInfo.IsGrounded)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_groundInfo.Point, 0.05f);

                // Zeichne Normal
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_groundInfo.Point, _groundInfo.Point + _groundInfo.Normal * 0.5f);
            }
#endif
        }

        #endregion
    }
}
