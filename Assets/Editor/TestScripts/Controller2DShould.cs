﻿using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class Controller2DShould
    {
        private const float FloatTolerance = 0.0001f;

        public class CreationTests
        {
            private GameObject player;

            [SetUp]
            public void CreateController()
            {
                this.player = new GameObject();
                this.player.name = "Player (Test)";
            }

            [TearDown]
            public void CleanUp()
            {
                GameObject.DestroyImmediate(this.player);
            }

            [Test]
            public void StartAtSpecifiedPosition()
            {
                var adjustedBoxCollider = this.player.AddComponent<BoxCollider2D>();
                adjustedBoxCollider.transform.position = new Vector2(2, 3);
                var controller = new Controller2D(adjustedBoxCollider);
                Assert.AreEqual(new Vector2(2, 3), controller.Position);
            }
        }

        public class FreeFallTests
        {
            private GameObject player;
            private Controller2D controller;

            [SetUp]
            public void CreateController()
            {
                this.player = new GameObject();
                this.player.name = "Player (Test)";
                this.controller = new Controller2D(player.AddComponent<BoxCollider2D>());
            }

            [TearDown]
            public void CleanUp()
            {
                GameObject.DestroyImmediate(this.player);
            }

            [Test]
            public void BeAffectedByGravity()
            {
                this.controller.Simulate(1);
                Assert.AreEqual(Controller2D.Gravity, this.controller.Velocity.y, FloatTolerance);
            }

            [Test]
            public void AccumulateGravity()
            {
                this.controller.Velocity = new Vector2(0, 20);
                this.controller.Simulate(1);
                Assert.AreEqual(20 + Controller2D.Gravity, this.controller.Velocity.y, FloatTolerance);
            }

            [Test]
            public void NotChangePositionIfVelocityIsZero()
            {
                this.controller.Simulate(1);
                Assert.AreEqual(0, this.controller.Position.x, FloatTolerance);
            }

            [Test]
            public void ChangePositionBasedOnVelocityOverTime()
            {
                this.controller.Velocity = new Vector2(10, 0);
                this.controller.Simulate(1);
                this.controller.Simulate(0.5f);
                Assert.AreEqual(15, this.controller.Position.x, FloatTolerance);
            }
        }

        public class CollisionTests
        {
            private GameObject player;
            private GameObject ground;
            private Controller2D controller;

            [SetUp]
            public void CreateController()
            {
                this.player = new GameObject();
                this.player.name = "Player (Test)";
                var boxCollider = player.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(2, 3);
                this.controller = new Controller2D(boxCollider);

                this.ground = new GameObject();
                this.ground.name = "Ground (Test)";
                this.ground.layer = LayerMask.NameToLayer("SolidObstacles");
                var groundCollider = this.ground.AddComponent<BoxCollider2D>();
                groundCollider.size = new Vector2(3, 1);
                groundCollider.offset = new Vector2(0, this.controller.Collider.transform.position.y - this.controller.Collider.bounds.extents.y - groundCollider.bounds.extents.y);
            }

            [TearDown]
            public void CleanUp()
            {
                GameObject.DestroyImmediate(this.player);
                GameObject.DestroyImmediate(ground);
            }

            [Test]
            public void NotFallWhenOnTheGround()
            {
                this.controller.Simulate(1);
                Assert.AreEqual(0, this.controller.Position.y, FloatTolerance);
            }

            [Test]
            public void IgnoreLayersThatArentSolidObstacles()
            {
                var startingPosition = this.controller.Position.y;
                this.ground.layer = LayerMask.NameToLayer("Water");
                this.controller.Simulate(1);
                Assert.Less(this.controller.Position.y, startingPosition);
            }

            [Test]
            public void StopAtGroundWhenFalling()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.offset += new Vector2(0, -1);
                this.controller.Simulate(1);
                Assert.AreEqual(-1, this.controller.Position.y, FloatTolerance);
            }

            [Test]
            public void NotAdjustVelocityIfCollisionWouldNotOccur()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.offset += new Vector2(0, -8);
                this.controller.Simulate(0.1f);
                Assert.AreEqual(Controller2D.Gravity * 0.1f, this.controller.Velocity.y, FloatTolerance);
            }

            [Test]
            public void AdjustVelocityIfCollisionWouldOccur()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.offset += new Vector2(0, 1);
                this.controller.Simulate(1);
                Assert.AreEqual(0, this.controller.Velocity.y, FloatTolerance);
            }

            [Test]
            public void SlideInCollisions()
            {
                this.controller.Velocity = new Vector2(5, 0);
                this.controller.Simulate(1);
                Assert.AreEqual(5, this.controller.Position.x, FloatTolerance);
            }

            [Test]
            public void StopMovingHorizontallyAtWalls()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.size = new Vector2(1, 10);
                groundCollider.offset = new Vector2(1 + this.controller.Collider.transform.position.x + this.controller.Collider.bounds.extents.x + groundCollider.bounds.extents.x, 0);

                this.controller.Velocity = new Vector2(5, 0);
                this.controller.Simulate(1);
                Assert.AreEqual(1, this.controller.Position.x, FloatTolerance);
            }

            [Test]
            public void StopMovingHorizontallyAtWallsEvenWhenStandingOnTheGround()
            {
                var wall = new GameObject();
                try
                {
                    wall.name = "Wall (Test)";
                    wall.layer = LayerMask.NameToLayer("SolidObstacles");
                    var groundCollider = wall.AddComponent<BoxCollider2D>();
                    groundCollider.size = new Vector2(1, 10);
                    groundCollider.offset = new Vector2(1 + this.controller.Collider.transform.position.x + this.controller.Collider.bounds.extents.x + groundCollider.bounds.extents.x, 0);

                    this.controller.Velocity = new Vector2(5, 0);
                    this.controller.Simulate(1);
                    Assert.AreEqual(1, this.controller.Position.x, FloatTolerance);
                }
                finally
                {
                    GameObject.DestroyImmediate(wall);
                }
            }

            [Test]
            public void StandOnSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 30));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                var currentY = this.controller.Position.y;
                this.controller.Simulate(1);
                Assert.AreEqual(currentY, this.controller.Position.y, FloatTolerance);
            }
        }

        public class InputTests
        {
            private GameObject player;
            private Controller2D controller;

            [SetUp]
            public void CreateController()
            {
                this.player = new GameObject();
                this.player.name = "Player (Test)";
                var boxCollider = player.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(2, 3);
                this.controller = new Controller2D(boxCollider);
            }

            [TearDown]
            public void CleanUp()
            {
                GameObject.DestroyImmediate(this.player);
            }

            [Test]
            public void MoveHorizontallyWhenInputForceIsApplied()
            {
                this.controller.WalkForce = 1;
                this.controller.Simulate(1);
                Assert.AreEqual(Controller2D.WalkSpeed, this.controller.Position.x, FloatTolerance);
            }

            [Test]
            public void HaveSameHorizontalMovementRegardlessOfTimeStep()
            {
                this.controller.WalkForce = 1;
                this.controller.Simulate(0.1f);
                this.controller.Simulate(0.9f);
                Assert.AreEqual(Controller2D.WalkSpeed, this.controller.Position.x, FloatTolerance);
            }

            [Test]
            public void StopMovingHorizontallyWhenInputForceIsZero()
            {
                this.controller.WalkForce = 1;
                this.controller.Simulate(1);
                this.controller.WalkForce = 0;
                this.controller.Simulate(1);
                Assert.AreEqual(Controller2D.WalkSpeed, this.controller.Position.x, FloatTolerance);
            }
        }

        public class InputAndCollisionTests
        {
            private GameObject player;
            private GameObject ground;
            private Controller2D controller;

            [SetUp]
            public void CreateController()
            {
                this.player = new GameObject();
                this.player.name = "Player (Test)";
                var boxCollider = player.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(2, 3);
                this.controller = new Controller2D(boxCollider);

                this.ground = new GameObject();
                this.ground.name = "Ground (Test)";
                this.ground.layer = LayerMask.NameToLayer("SolidObstacles");
                var groundCollider = this.ground.AddComponent<BoxCollider2D>();
                groundCollider.size = new Vector2(40, 1);
                groundCollider.offset = new Vector2(0, this.controller.Collider.transform.position.y - this.controller.Collider.bounds.extents.y - groundCollider.bounds.extents.y);
            }

            [TearDown]
            public void CleanUp()
            {
                GameObject.DestroyImmediate(this.player);
                GameObject.DestroyImmediate(ground);
            }

            [Test]
            public void BeAbleToJumpIfOnTheGround()
            {
                var startingPosition = this.controller.Position.y;
                this.controller.Simulate(0.01f);
                this.controller.JumpForce = 1;
                this.controller.Simulate(0.5f);
                Assert.Greater(this.controller.Position.y, startingPosition);
            }

            [Test]
            public void NotBeAbleToJumpIfNotOnTheGround()
            {
                var startingPosition = 30;
                this.controller.Position = new Vector2(0, startingPosition);
                this.controller.JumpForce = 1;
                this.controller.Simulate(1);
                Assert.Less(this.controller.Position.y, startingPosition);
            }

            [Test]
            public void HaveSameJumpHeightRegardlessOfTimeStep()
            {
                this.controller.Simulate(0.01f);
                this.controller.JumpForce = 1;
                this.controller.Simulate(0.1f);
                this.controller.Simulate(0.1f);
                this.controller.Simulate(0.1f);
                this.controller.Simulate(0.1f);
                this.controller.Simulate(0.1f);
                var positionStepped = this.controller.Position.y;

                this.controller.Position = new Vector2(0, 0);
                this.controller.Velocity = new Vector2(0, 0);
                this.controller.Simulate(0.01f);
                this.controller.JumpForce = 1;
                this.controller.Simulate(0.5f);
                var positionLeapt = this.controller.Position.y;
                Assert.AreEqual(positionLeapt, positionStepped, FloatTolerance);
            }

            [Test]
            public void StopMovingHorizontallyAtWallsEvenIfTheVelocityComesFromInput()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.size = new Vector2(1, 10);
                groundCollider.offset = new Vector2(1 + this.controller.Collider.transform.position.x + this.controller.Collider.bounds.extents.x + groundCollider.bounds.extents.x, 0);

                this.controller.WalkForce = 1;
                this.controller.Simulate(1);
                Assert.AreEqual(1, this.controller.Position.x, FloatTolerance);
            }

            [Test]
            public void WalkUpSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 30));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                var currentY = this.controller.Position.y;
                this.controller.WalkForce = 1;
                this.controller.Simulate(1);
                Assert.AreEqual(Controller2D.WalkSpeed * Mathf.Cos(30 * Mathf.Deg2Rad), this.controller.Position.x, FloatTolerance);
                Assert.AreEqual(currentY + Controller2D.WalkSpeed * Mathf.Sin(30 * Mathf.Deg2Rad), this.controller.Position.y, FloatTolerance);
            }
        }
    }
}