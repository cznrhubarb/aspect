using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class VectorSplitterShould
    {
        private const float FloatTolerance = 0.0001f;

        private CollisionEvent collisionEvent;

        [SetUp]
        public void SetUp()
        {
            this.collisionEvent = new CollisionEvent();
        }

        [Test]
        public void KnowWhenThereIsNoCollision()
        {
            var originalVector = Vector2.right;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(CollisionAlignment.NoSplit, newVectors.Alignment);
        }

        private static IEnumerable<TestCaseData> OriginalVectorMatchData
        {
            get
            {
                yield return new TestCaseData(Vector2.right, Vector2.right);
                yield return new TestCaseData(Vector2.up, Vector2.up);
            }
        }
        [TestCaseSource("OriginalVectorMatchData")]
        public void ReturnOriginalVectorWhenThereIsNoCollision(Vector2 originalVector, Vector2 goalVector)
        {
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(goalVector, newVectors.First);
        }

        private static IEnumerable<TestCaseData> OpposedCollisionData
        {
            get
            {
                yield return new TestCaseData(Vector2.right, Vector2.right * -1f);
                yield return new TestCaseData(Vector2.right, (Vector2.right * -1f).Rotate(45));
                yield return new TestCaseData(Vector2.up, (Vector2.up * -1f).Rotate(25));
                yield return new TestCaseData(Vector2.up, (Vector2.up * -1f).Rotate(-40));
            }
        }
        [TestCaseSource("OpposedCollisionData")]
        public void KnowWhenThereIsAnOpposedCollision(Vector2 originalVector, Vector2 collisionNormal)
        {
            this.collisionEvent.percentToHit = 0.5f;
            this.collisionEvent.normal = collisionNormal;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(CollisionAlignment.Opposed, newVectors.Alignment);
        }

        private static IEnumerable<TestCaseData> NonOpposedCollisionData
        {
            get
            {
                yield return new TestCaseData(Vector2.up, (Vector2.up * -1f).Rotate(50));
                yield return new TestCaseData(Vector2.right, (Vector2.right * -1f).Rotate(85));
                yield return new TestCaseData(Vector2.right, (Vector2.right * -1f).Rotate(-65));
            }
        }
        [TestCaseSource("NonOpposedCollisionData")]
        public void KnowWhenThereIsANonOpposedCollision(Vector2 originalVector, Vector2 collisionNormal)
        {
            this.collisionEvent.percentToHit = 0.5f;
            this.collisionEvent.normal = collisionNormal;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(CollisionAlignment.NonOpposed, newVectors.Alignment);
        }

        private static IEnumerable<TestCaseData> FirstMagnitudeCheckData
        {
            get
            {
                yield return new TestCaseData(0.3f, 30);
                yield return new TestCaseData(0.7f, 30);
                yield return new TestCaseData(0.2f, 60);
                yield return new TestCaseData(0.6f, -30);
                yield return new TestCaseData(0.8f, 0);
            }
        }
        [TestCaseSource("FirstMagnitudeCheckData")]
        public void ReturnsFirstVectorWithMagnitudeProportionalToCollisionTime(float timeToCollision, float normalRotation)
        {
            var originalVector = new Vector2(1, 3);
            this.collisionEvent.percentToHit = timeToCollision;
            this.collisionEvent.normal = (originalVector * -1f).Rotate(normalRotation);
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(timeToCollision, newVectors.First.magnitude / originalVector.magnitude, FloatTolerance);
        }

        private static IEnumerable<TestCaseData> SecondMagnitudeCheckData
        {
            get
            {
                yield return new TestCaseData(0.3f, 30);
                yield return new TestCaseData(0.7f, 30);
                yield return new TestCaseData(0.2f, 60);
                yield return new TestCaseData(0.6f, -30);
            }
        }
        [TestCaseSource("SecondMagnitudeCheckData")]
        public void ReturnsSecondVectorWithMagnitudeProportionalToCollisionTimeIfNotOpposite(float timeToCollision, float normalRotation)
        {
            var originalVector = new Vector2(4, 2);
            this.collisionEvent.percentToHit = timeToCollision;
            this.collisionEvent.normal = (originalVector * -1f).Rotate(normalRotation);
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            var expectedMagnitude = Mathf.Cos((90 - Mathf.Abs(normalRotation)) * Mathf.Deg2Rad) * (1 - timeToCollision);
            Assert.AreEqual(expectedMagnitude, newVectors.Second.magnitude / originalVector.magnitude, FloatTolerance);
        }

        [Test]
        public void ReturnsSecondVectorAsZeroIfCollisionIsOpposite()
        {
            var originalVector = new Vector2(4, 2);
            this.collisionEvent.percentToHit = 0.4f;
            this.collisionEvent.normal = originalVector * -1f;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(0, newVectors.Second.magnitude, FloatTolerance);
        }

        [Test]
        public void ReturnsSecondVectorAsZeroIfThereIsNoCollision()
        {
            var originalVector = new Vector2(4, 2);
            this.collisionEvent.percentToHit = 1;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(0, newVectors.Second.magnitude, FloatTolerance);
        }

        [TestCaseSource("NonOpposedCollisionData")]
        public void ResultInTheFirstReturnedVectorAndTheOriginalVectorPointingTheSameDirection(Vector2 originalVector, Vector2 collisionNormal)
        {
            this.collisionEvent.percentToHit = 0.4f;
            this.collisionEvent.normal = collisionNormal;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.Less(Vector2.Distance(originalVector.normalized, newVectors.First.normalized), FloatTolerance);
        }

        private static IEnumerable<TestCaseData> ReflectedVectorData
        {
            get
            {
                yield return new TestCaseData(Vector2.up, (Vector2.up * -1f).Rotate(60), (Vector2.up * -1f).Rotate(60 + 90));
                yield return new TestCaseData(Vector2.right, (Vector2.right * -1f).Rotate(-80), (Vector2.right * -1f).Rotate(-80 - 90));
                yield return new TestCaseData(Vector2.right, (Vector2.right * -1f).Rotate(45), (Vector2.right * -1f).Rotate(45 + 90));
                yield return new TestCaseData(Vector2.up, (Vector2.up * -1f).Rotate(-25), (Vector2.up * -1f).Rotate(-25 - 90));
            }
        }
        [TestCaseSource("ReflectedVectorData")]
        public void ResultInTheSecondReturnedVectorBeingPerpindicularToTheNormal(Vector2 originalVector, Vector2 collisionNormal, Vector2 reflectedVector)
        {
            this.collisionEvent.percentToHit = 0.4f;
            this.collisionEvent.normal = collisionNormal;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.Less(Vector2.Distance(reflectedVector.normalized, newVectors.Second.normalized), FloatTolerance);
        }

        private static IEnumerable<TestCaseData> AlignedNormalData
        {
            get
            {
                yield return new TestCaseData(Vector2.up, Vector2.up);
                yield return new TestCaseData(Vector2.right, Vector2.right.Rotate(-80));
                yield return new TestCaseData(Vector2.right, Vector2.right.Rotate(30));
            }
        }
        [TestCaseSource("AlignedNormalData")]
        public void KnowWhenOriginalVectorAndCollisionNormalAreAligned(Vector2 originalVector, Vector2 collisionNormal)
        {
            this.collisionEvent.percentToHit = 0.4f;
            this.collisionEvent.normal = collisionNormal;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(CollisionAlignment.Aligned, newVectors.Alignment);
        }
    }
}