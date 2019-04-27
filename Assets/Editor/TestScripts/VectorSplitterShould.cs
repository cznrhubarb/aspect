using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    // Terminology used in this class:
    //  An Opposed Collision is one where the normal is <= 45 degrees different from the opposite of the original vector
    //  A Non Opposed Collision will have a normal that is > 45 degrees different (but less than 90 because otherwise there would be no collision)

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
        public void ReturnOnlyOneVectorWhenThereIsNoCollision()
        {
            var originalVector = Vector2.right;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(1, newVectors.Count);
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
            Assert.AreEqual(goalVector, newVectors[0]);
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
        public void ReturnOnlyOneVectorWhenThereIsAnOpposedCollision(Vector2 originalVector, Vector2 collisionNormal)
        {
            this.collisionEvent.percentToHit = 0.5f;
            this.collisionEvent.normal = collisionNormal;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(1, newVectors.Count);
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
        public void ReturnTwoVectorsWhenThereIsANonOpposedCollision(Vector2 originalVector, Vector2 collisionNormal)
        {
            this.collisionEvent.percentToHit = 0.5f;
            this.collisionEvent.normal = collisionNormal;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(2, newVectors.Count);
        }

        [TestCase(0.2f)]
        [TestCase(0.5f)]
        public void ReturnTwoVectorsWithMagnitudesProportionalToCollisionTimeWhenThereIsANonOpposedCollision(float timeToCollision)
        {
            var originalVector = new Vector2(3, 2);
            this.collisionEvent.percentToHit = timeToCollision;
            this.collisionEvent.normal = (originalVector * -1f).Rotate(50).normalized;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(timeToCollision, newVectors[0].magnitude / originalVector.magnitude);
            Assert.AreEqual((1-timeToCollision), newVectors[1].magnitude / originalVector.magnitude);
        }

        [TestCase(0.3f)]
        [TestCase(0.7f)]
        public void ReturnOneVectorWithMagnitudeProportionalToCollisionTimeWhenThereIsAnOpposedCollision(float timeToCollision)
        {
            var originalVector = new Vector2(3, 2);
            this.collisionEvent.percentToHit = timeToCollision;
            this.collisionEvent.normal = (originalVector * -1f).Rotate(30).normalized;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.AreEqual(timeToCollision, newVectors[0].magnitude / originalVector.magnitude);
        }

        [TestCaseSource("NonOpposedCollisionData")]
        public void ResultInTheFirstReturnedVectorAndTheOriginalVectorPointingTheSameDirection(Vector2 originalVector, Vector2 collisionNormal)
        {
            this.collisionEvent.percentToHit = 0.4f;
            this.collisionEvent.normal = collisionNormal;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.Less(Vector2.Distance(originalVector.normalized, newVectors[0].normalized), FloatTolerance);
        }

        private static IEnumerable<TestCaseData> ReflectedVectorData
        {
            get
            {
                yield return new TestCaseData(Vector2.up, (Vector2.up * -1f).Rotate(60), (Vector2.up * -1f).Rotate(60 + 90));
                yield return new TestCaseData(Vector2.right, (Vector2.right * -1f).Rotate(-80), (Vector2.right * -1f).Rotate(-80 - 90));
            }
        }
        [TestCaseSource("ReflectedVectorData")]
        public void ResultInTheSecondReturnedVectorBeingPerpindicularToTheNormal(Vector2 originalVector, Vector2 collisionNormal, Vector2 reflectedVector)
        {
            this.collisionEvent.percentToHit = 0.4f;
            this.collisionEvent.normal = collisionNormal;
            var newVectors = VectorSplitter.Split(originalVector, this.collisionEvent);
            Assert.Less(Vector2.Distance(reflectedVector.normalized, newVectors[1].normalized), FloatTolerance);
        }

        private static IEnumerable<TestCaseData> ExceptionalNormalData
        {
            get
            {
                yield return new TestCaseData(Vector2.up, Vector2.up);
                yield return new TestCaseData(Vector2.right, Vector2.right.Rotate(-80));
                yield return new TestCaseData(Vector2.right, Vector2.right.Rotate(30));
            }
        }
        [TestCaseSource("ExceptionalNormalData")]
        public void ThrowAnExceptionWhenOriginalVectorAndCollisionNormalAreAligned(Vector2 originalVector, Vector2 collisionNormal)
        {
            this.collisionEvent.percentToHit = 0.4f;
            this.collisionEvent.normal = collisionNormal;
            var exception = Assert.Throws<ArgumentException>(() => VectorSplitter.Split(originalVector, this.collisionEvent));
            Assert.That(exception.Message, Is.EqualTo("Invalid Collision Normal: Collision normal is aligned with the vector to be split"));
        }
    }
}