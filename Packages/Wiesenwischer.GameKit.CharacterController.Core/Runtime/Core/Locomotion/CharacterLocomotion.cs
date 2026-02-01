using UnityEngine;
using Wiesenwischer.GameKit.CharacterController.Core.Motor;
using Wiesenwischer.GameKit.CharacterController.Core.StateMachine;

namespace Wiesenwischer.GameKit.CharacterController.Core.Locomotion
{
    /// <summary>
    /// Character Locomotion mit Step Detection.
    /// Berechnet Velocity aus Input, Motor bewegt den Character.
    /// Step Detection ist aktiv wenn grounded.
    /// </summary>
    public class CharacterLocomotion : ILocomotionController
    {
        private readonly KinematicMotor _motor;
        private readonly Transform _transform;
        private readonly ILocomotionConfig _config;
        private readonly GroundingDetection _groundingDetection;

        private Vector3 _velocity;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;

        private float _targetYaw;
        private float _currentYaw;

        public CharacterLocomotion(
            Transform transform,
            CapsuleCollider capsule,
            ILocomotionConfig config,
            float skinWidth = 0.02f)
        {
            _transform = transform ?? throw new System.ArgumentNullException(nameof(transform));
            _config = config ?? throw new System.ArgumentNullException(nameof(config));

            _motor = new KinematicMotor(transform, capsule, config, skinWidth);
            _groundingDetection = new GroundingDetection(transform, config, capsule.radius, capsule.height);

            _currentYaw = transform.eulerAngles.y;
            _targetYaw = _currentYaw;
        }

        public CharacterLocomotion(
            Transform transform,
            CapsuleCollider capsule,
            ILocomotionConfig config,
            GroundingDetection groundingDetection,
            float skinWidth = 0.02f)
        {
            _transform = transform ?? throw new System.ArgumentNullException(nameof(transform));
            _config = config ?? throw new System.ArgumentNullException(nameof(config));
            _groundingDetection = groundingDetection ?? throw new System.ArgumentNullException(nameof(groundingDetection));

            _motor = new KinematicMotor(transform, capsule, config, skinWidth);

            _currentYaw = transform.eulerAngles.y;
            _targetYaw = _currentYaw;
        }

        #region ILocomotionController

        public Vector3 Position => _transform.position;
        public Quaternion Rotation => _transform.rotation;
        public Vector3 Velocity => _velocity;
        public bool IsGrounded => _groundingDetection.IsGrounded;
        public GroundInfo GroundInfo => _groundingDetection.GroundInfo;
        public bool IsSliding => false; // Nicht implementiert in dieser Version
        public float SlidingTime => 0f;
        public KinematicMotor Motor => _motor;

        public void Simulate(LocomotionInput input, float deltaTime)
        {
            // 1. Ground Check
            _groundingDetection.UpdateGroundCheck();

            // 2. Horizontale Bewegung
            Vector3 targetHorizontal = CalculateTargetHorizontalVelocity(input);
            _horizontalVelocity = ApplyAcceleration(_horizontalVelocity, targetHorizontal, deltaTime);

            // 3. Vertikale Bewegung (von State Machine kontrolliert)
            _verticalVelocity = CalculateVerticalVelocity(input.VerticalVelocity, deltaTime);

            // 4. Kombiniere zu finaler Velocity
            _velocity = _horizontalVelocity + Vector3.up * _verticalVelocity;

            // 5. Bewege über Motor (Step Detection wird von State Machine gesteuert)
            bool enableStepDetection = input.StepDetectionEnabled;
            _motor.Move(_velocity * deltaTime, enableStepDetection);

            // 6. Rotation
            if (_config.RotateTowardsMovement && input.MoveDirection.sqrMagnitude > 0.01f)
            {
                UpdateRotation(deltaTime);
            }
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            _motor.SetPositionAndRotation(position, rotation);
            _currentYaw = rotation.eulerAngles.y;
            _targetYaw = _currentYaw;
        }

        public void ApplyVelocity(Vector3 velocity)
        {
            _horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            _verticalVelocity = velocity.y;
            _velocity = velocity;
        }

        #endregion

        #region Movement

        private Vector3 CalculateTargetHorizontalVelocity(LocomotionInput input)
        {
            Vector3 inputDir = new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y);

            // Transformiere Input relativ zur Kamera/Look-Richtung
            if (input.LookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(
                    new Vector3(input.LookDirection.x, 0, input.LookDirection.z).normalized,
                    Vector3.up);
                inputDir = lookRot * inputDir;
            }
            else
            {
                inputDir = _transform.TransformDirection(inputDir);
            }

            float speed = input.IsSprinting ? _config.RunSpeed : _config.WalkSpeed;

            // Reduzierte Kontrolle in der Luft
            if (!_groundingDetection.IsGrounded)
            {
                speed *= _config.AirControl;
            }

            return inputDir.normalized * speed * inputDir.magnitude;
        }

        private Vector3 ApplyAcceleration(Vector3 current, Vector3 target, float deltaTime)
        {
            float accel = target.sqrMagnitude > 0.01f ? _config.Acceleration : _config.Deceleration;

            if (!_groundingDetection.IsGrounded)
            {
                accel *= _config.AirControl;
            }

            return Vector3.MoveTowards(current, target, accel * deltaTime);
        }

        private float CalculateVerticalVelocity(float inputVerticalVelocity, float deltaTime)
        {
            float velocity = inputVerticalVelocity;

            // Kleine negative Velocity wenn auf dem Boden (hält Character geerdet)
            if (_groundingDetection.IsGrounded && velocity <= 0)
            {
                return -2f;
            }

            // Gravity anwenden
            velocity -= _config.Gravity * deltaTime;
            velocity = Mathf.Max(velocity, -_config.MaxFallSpeed);

            return velocity;
        }

        #endregion

        #region Rotation

        private void UpdateRotation(float deltaTime)
        {
            if (_horizontalVelocity.sqrMagnitude < 0.01f) return;

            Vector3 dir = _horizontalVelocity.normalized;
            _targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            _currentYaw = Mathf.MoveTowardsAngle(_currentYaw, _targetYaw, _config.RotationSpeed * deltaTime);
            _transform.rotation = Quaternion.Euler(0, _currentYaw, 0);
        }

        public void SetRotation(float yaw)
        {
            _currentYaw = yaw;
            _targetYaw = yaw;
            _transform.rotation = Quaternion.Euler(0, yaw, 0);
        }

        #endregion

        #region Utility

        public Vector3 HorizontalVelocity => _horizontalVelocity;
        public float VerticalVelocity => _verticalVelocity;

        public void SetVerticalVelocity(float velocity)
        {
            _verticalVelocity = velocity;
        }

        public void StopMovement()
        {
            _velocity = Vector3.zero;
            _horizontalVelocity = Vector3.zero;
            _verticalVelocity = 0f;
        }

        public GroundingDetection GetGroundingDetection() => _groundingDetection;

        public float GetJumpVelocity()
        {
            return Mathf.Sqrt(2f * _config.Gravity * _config.JumpHeight);
        }

        #endregion

        #region Debug

        public void DrawDebugGizmos()
        {
#if UNITY_EDITOR
            _groundingDetection?.DrawDebugGizmos();
            _motor?.DrawDebugGizmos();

            if (_transform == null) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_transform.position, _transform.position + _velocity);
#endif
        }

        #endregion
    }
}
