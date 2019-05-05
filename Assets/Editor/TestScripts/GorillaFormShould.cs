using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class GorillaFormShould
    {
        private GorillaForm form;

        [SetUp]
        public void SetUp()
        {
            form = new GorillaForm();
        }

        [Test]
        public void GiveAnUpVectorWhenJumpingOnTheGround()
        {
            Assert.AreEqual(Vector2.up, this.form.GetJumpVelocity(Vector2.up));
        }

        [Test]
        public void GiveAZeroVectorWhenTryingToJumpMidAir()
        {
            Assert.AreEqual(Vector2.zero, this.form.GetJumpVelocity(Vector2.zero));
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
            Assert.AreEqual(expectedNormal, this.form.GetJumpVelocity(wallNormal));
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
            Assert.AreEqual(expectedNormal, this.form.GetJumpVelocity(slopeNormal));
        }
    }
}