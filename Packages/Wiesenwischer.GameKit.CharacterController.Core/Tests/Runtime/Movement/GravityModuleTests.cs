using NUnit.Framework;
using Wiesenwischer.GameKit.CharacterController.Core.Locomotion.Modules;

namespace Wiesenwischer.GameKit.CharacterController.Core.Tests.Movement
{
    /// <summary>
    /// Unit Tests für GravityModule.
    /// Testet Gravity-Berechnung, Grounding-Velocity und MaxFallSpeed Clamping.
    /// </summary>
    [TestFixture]
    public class GravityModuleTests
    {
        private GravityModule _gravityModule;

        private const float Gravity = 20f;
        private const float MaxFallSpeed = 50f;
        private const float DeltaTime = 1f / 60f;

        [SetUp]
        public void SetUp()
        {
            _gravityModule = new GravityModule();
        }

        #region Grounding Tests

        [Test]
        public void Grounded_NotMovingUp_ReturnsGroundingVelocity()
        {
            float result = _gravityModule.CalculateVerticalVelocity(
                0f, isGrounded: true, Gravity, MaxFallSpeed, DeltaTime);

            Assert.AreEqual(GravityModule.GroundingVelocity, result);
        }

        [Test]
        public void Grounded_FallingDown_ReturnsGroundingVelocity()
        {
            float result = _gravityModule.CalculateVerticalVelocity(
                -5f, isGrounded: true, Gravity, MaxFallSpeed, DeltaTime);

            Assert.AreEqual(GravityModule.GroundingVelocity, result);
        }

        [Test]
        public void Grounded_MovingUp_AppliesGravity()
        {
            // Wenn grounded aber aufwärts (z.B. Jump-Impulse), Gravity anwenden
            float upVelocity = 10f;
            float result = _gravityModule.CalculateVerticalVelocity(
                upVelocity, isGrounded: true, Gravity, MaxFallSpeed, DeltaTime);

            float expected = upVelocity - Gravity * DeltaTime;
            Assert.AreEqual(expected, result, 0.001f);
        }

        #endregion

        #region Airborne Gravity Tests

        [Test]
        public void Airborne_AppliesGravity()
        {
            float result = _gravityModule.CalculateVerticalVelocity(
                0f, isGrounded: false, Gravity, MaxFallSpeed, DeltaTime);

            float expected = -Gravity * DeltaTime;
            Assert.AreEqual(expected, result, 0.001f);
        }

        [Test]
        public void Airborne_DeceleratesUpwardVelocity()
        {
            float upVelocity = 10f;
            float result = _gravityModule.CalculateVerticalVelocity(
                upVelocity, isGrounded: false, Gravity, MaxFallSpeed, DeltaTime);

            Assert.Less(result, upVelocity);
            Assert.AreEqual(upVelocity - Gravity * DeltaTime, result, 0.001f);
        }

        [Test]
        public void Airborne_ClampsToMaxFallSpeed()
        {
            // Sehr hohe Fallgeschwindigkeit → wird geclampt
            float result = _gravityModule.CalculateVerticalVelocity(
                -100f, isGrounded: false, Gravity, MaxFallSpeed, DeltaTime);

            Assert.AreEqual(-MaxFallSpeed, result, 0.001f);
        }

        [Test]
        public void Airborne_AtMaxFallSpeed_StaysAtMax()
        {
            float result = _gravityModule.CalculateVerticalVelocity(
                -MaxFallSpeed, isGrounded: false, Gravity, MaxFallSpeed, DeltaTime);

            Assert.AreEqual(-MaxFallSpeed, result, 0.001f);
        }

        #endregion

        #region GroundSnapping Tests

        [Test]
        public void ApplyGroundSnapping_StableOnGround_ReturnsGroundingVelocity()
        {
            float result = _gravityModule.ApplyGroundSnapping(0f, isStableOnGround: true);
            Assert.AreEqual(GravityModule.GroundingVelocity, result);
        }

        [Test]
        public void ApplyGroundSnapping_NotOnGround_ReturnsOriginal()
        {
            float velocity = -5f;
            float result = _gravityModule.ApplyGroundSnapping(velocity, isStableOnGround: false);
            Assert.AreEqual(velocity, result);
        }

        [Test]
        public void ApplyGroundSnapping_OnGround_MovingUp_ReturnsOriginal()
        {
            float upVelocity = 5f;
            float result = _gravityModule.ApplyGroundSnapping(upVelocity, isStableOnGround: true);
            Assert.AreEqual(upVelocity, result);
        }

        #endregion
    }
}
