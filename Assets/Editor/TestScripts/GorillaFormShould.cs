using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class GorillaFormShould
    {
        private IForm form;

        [SetUp]
        public void SetUp()
        {
            form = new GorillaForm();
        }

        [Test]
        public void GiveAnUpVectorWhenJumpingOnTheGround()
        {
            Assert.AreEqual(Vector2.up * GorillaForm.JumpPower, this.form.GetJumpVelocity(Vector2.up, 1));
        }

        [Test]
        public void GiveAZeroVectorWhenTryingToJumpMidAir()
        {
            Assert.AreEqual(Vector2.zero, this.form.GetJumpVelocity(Vector2.zero, 1));
        }

        private static IEnumerable<TestCaseData> WallJumpNormalData
        {
            get
            {
                yield return new TestCaseData(Vector2.right, new Vector2(1, 1).normalized);
                yield return new TestCaseData(new Vector2(2, 1).normalized, (new Vector2(2, 1).normalized + Vector2.up).normalized);
            }
        }
        [TestCaseSource("WallJumpNormalData")]
        public void GiveAVectorBetweenUpAndWallNormalWhenWallJumping(Vector2 wallNormal, Vector2 expectedNormal)
        {
            Assert.AreEqual(expectedNormal * GorillaForm.JumpPower, this.form.GetJumpVelocity(wallNormal, 1));
        }

        private static IEnumerable<TestCaseData> ShallowSlopeJumpNormalData
        {
            get
            {
                yield return new TestCaseData(new Vector2(-1, 5).normalized);
                yield return new TestCaseData(new Vector2(1, 2).normalized);
            }
        }
        [TestCaseSource("ShallowSlopeJumpNormalData")]
        public void JumpStraightUpOnShallowSlopes(Vector2 slopeNormal)
        {
            var expectedNormal = Vector2.up;
            Assert.AreEqual(expectedNormal * GorillaForm.JumpPower, this.form.GetJumpVelocity(slopeNormal, 1));
        }

        [Test]
        public void NotBeAbleToWallJumpImmediatelyAfterJumping()
        {
            var wallNormal = Vector2.right;
            Assert.AreNotEqual(this.form.GetJumpVelocity(wallNormal, 1), this.form.GetJumpVelocity(wallNormal, 1));
        }

        [Test]
        public void HaveNoDelayInJumpingOffTheGround()
        {
            var wallNormal = Vector2.up;
            Assert.AreEqual(this.form.GetJumpVelocity(wallNormal, 1), this.form.GetJumpVelocity(wallNormal, 1));
        }

        [Test]
        public void BeAbleToJumpAgainAfterADelay()
        {
            var wallNormal = Vector2.right;
            var firstJumpVector = this.form.GetJumpVelocity(wallNormal, 1);
            this.form.Update(GorillaForm.DelayBetweenJumps);
            Assert.AreEqual(firstJumpVector, this.form.GetJumpVelocity(wallNormal, 1));
        }

        [Test]
        public void NotReturnHorizontalMovementImmediatelyAfterWallJumping()
        {
            var wallNormal = Vector2.right;
            this.form.GetJumpVelocity(wallNormal, 1);
            Assert.AreEqual(0, this.form.GetWalkVelocity(wallNormal, 1).x);
        }

        [Test]
        public void AllowHorizontalMovementAfterWallJumpingAfterDelay()
        {
            var wallNormal = Vector2.right;
            this.form.GetJumpVelocity(wallNormal, 1);
            this.form.Update(GorillaForm.WalkDelayAfterJump);
            Assert.AreEqual(GorillaForm.WalkSpeed, this.form.GetWalkVelocity(wallNormal, 1).x);
        }

        [Test]
        public void AllowHorizontalMovementImmediatelyAfterGroundJumping()
        {
            var wallNormal = Vector2.up;
            this.form.GetJumpVelocity(wallNormal, 1);
            Assert.AreEqual(GorillaForm.WalkSpeed, this.form.GetWalkVelocity(wallNormal, 1).x);
        }
    }
}