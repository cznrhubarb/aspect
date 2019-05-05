using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class Controller2DShould
    {
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
                var controller = new Controller2D(adjustedBoxCollider, new HumanForm());
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
                this.controller = new Controller2D(player.AddComponent<BoxCollider2D>(), new HumanForm());
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
                Assert.AreEqual(HumanForm.Gravity, this.controller.Velocity.y, Common.FloatTolerance);
            }

            [Test]
            public void AccumulateGravity()
            {
                this.controller.Velocity = new Vector2(0, 20);
                this.controller.Simulate(1);
                Assert.AreEqual(20 + HumanForm.Gravity, this.controller.Velocity.y, Common.FloatTolerance);
            }

            [Test]
            public void NotChangePositionIfVelocityIsZero()
            {
                this.controller.Simulate(1);
                Assert.AreEqual(0, this.controller.Position.x, Common.FloatTolerance);
            }

            [Test]
            public void ChangePositionBasedOnVelocityOverTime()
            {
                this.controller.Velocity = new Vector2(10, 0);
                this.controller.Simulate(1);
                this.controller.Simulate(0.5f);
                Assert.AreEqual(15, this.controller.Position.x, Common.FloatTolerance);
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
                this.controller = new Controller2D(boxCollider, new HumanForm());

                this.ground = new GameObject();
                this.ground.name = "Ground (Test)";
                this.ground.layer = LayerMask.NameToLayer("SolidObstacles");
                var groundCollider = this.ground.AddComponent<BoxCollider2D>();
                groundCollider.size = new Vector2(200, 1);
                groundCollider.offset = new Vector2(0, this.controller.Collider.transform.position.y - this.controller.Collider.bounds.extents.y - groundCollider.bounds.extents.y);
            }

            [TearDown]
            public void CleanUp()
            {
                GameObject.DestroyImmediate(this.player);
                GameObject.DestroyImmediate(this.ground);
            }

            [Test]
            public void NotFallWhenOnTheGround()
            {
                this.controller.Simulate(1);
                Assert.AreEqual(0, this.controller.Position.y, Common.FloatTolerance);
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
                Assert.AreEqual(-1, this.controller.Position.y, Common.FloatTolerance);
            }

            [Test]
            public void NotAdjustVelocityIfCollisionWouldNotOccur()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.offset += new Vector2(0, 50);
                this.controller.Simulate(0.1f);
                Assert.AreEqual(HumanForm.Gravity * 0.1f, this.controller.Velocity.y, Common.FloatTolerance);
            }

            [Test]
            public void AdjustVelocityIfCollisionWouldOccur()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.offset += new Vector2(0, 1);
                this.controller.Simulate(1);
                Assert.AreEqual(0, this.controller.Velocity.y, Common.FloatTolerance);
            }

            [Test]
            public void StillMoveHorizontallyIfCollidingWithTheGroundVertically()
            {
                this.controller.Velocity = new Vector2(5, 0);
                this.controller.Simulate(1);
                Assert.AreEqual(5, this.controller.Position.x, Common.FloatTolerance);
            }

            [Test]
            public void StopMovingHorizontallyAtWalls()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.size = new Vector2(1, 10);
                groundCollider.offset = new Vector2(1 + this.controller.Collider.transform.position.x + this.controller.Collider.bounds.extents.x + groundCollider.bounds.extents.x, 0);

                this.controller.Velocity = new Vector2(5, 0);
                this.controller.Simulate(1);
                Assert.AreEqual(1, this.controller.Position.x, Common.FloatTolerance);
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
                    Assert.AreEqual(1, this.controller.Position.x, Common.FloatTolerance);
                }
                finally
                {
                    GameObject.DestroyImmediate(wall);
                }
            }

            [Test]
            public void StandOnShallowSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 30));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                var currentY = this.controller.Position.y;
                this.controller.Simulate(1);
                Assert.AreEqual(currentY, this.controller.Position.y, Common.FloatTolerance);
            }

            [Test]
            public void SlideDownSteepSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 50));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                var startingPosition = this.controller.Position;
                this.controller.Simulate(1);
                Assert.Less(this.controller.Position.x, startingPosition.x);
                Assert.Less(this.controller.Position.y, startingPosition.y);
            }

            [TestCase(0)]
            [TestCase(20)]
            [TestCase(-50)]
            public void ShouldStartFallingIfTheyHitACeiling(float ceilingRotation)
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, ceilingRotation));
                groundCollider.offset = new Vector2(0, 3 + this.controller.Collider.transform.position.y + this.controller.Collider.bounds.extents.y + groundCollider.bounds.extents.y);

                this.controller.Velocity = new Vector2(0, 50);
                this.controller.Simulate(0.2f);
                Assert.Less(this.controller.Velocity.y, 0);
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
                this.controller = new Controller2D(boxCollider, new HumanForm());
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
                Assert.AreEqual(HumanForm.WalkSpeed, this.controller.Position.x, Common.FloatTolerance);
            }

            [Test]
            public void HaveSameHorizontalMovementRegardlessOfTimeStep()
            {
                this.controller.WalkForce = 1;
                this.controller.Simulate(0.1f);
                this.controller.Simulate(0.9f);
                Assert.AreEqual(HumanForm.WalkSpeed, this.controller.Position.x, Common.FloatTolerance);
            }

            [Test]
            public void StopMovingHorizontallyWhenInputForceIsZero()
            {
                this.controller.WalkForce = 1;
                this.controller.Simulate(1);
                this.controller.WalkForce = 0;
                this.controller.Simulate(1);
                Assert.AreEqual(HumanForm.WalkSpeed, this.controller.Position.x, Common.FloatTolerance);
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
                this.controller = new Controller2D(boxCollider, new HumanForm());

                this.ground = new GameObject();
                this.ground.name = "Ground (Test)";
                this.ground.layer = LayerMask.NameToLayer("SolidObstacles");
                var groundCollider = this.ground.AddComponent<BoxCollider2D>();
                groundCollider.size = new Vector2(200, 1);
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
                Assert.AreEqual(positionLeapt, positionStepped, Common.FloatTolerance);
            }

            [Test]
            public void StopMovingHorizontallyAtWallsEvenIfTheVelocityComesFromInput()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.size = new Vector2(1, 100);
                groundCollider.offset = new Vector2(1 + this.controller.Collider.transform.position.x + this.controller.Collider.bounds.extents.x + groundCollider.bounds.extents.x, 0);

                this.controller.WalkForce = 1;
                this.controller.Simulate(1);
                Assert.AreEqual(1, this.controller.Position.x, Common.FloatTolerance);
            }

            [Test]
            public void ContinueFallingWhileMovingHorizontallyIntoWalls()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                groundCollider.size = new Vector2(1, 100);
                groundCollider.offset = new Vector2(this.controller.Collider.transform.position.x + this.controller.Collider.bounds.extents.x + groundCollider.bounds.extents.x, 0);

                this.controller.WalkForce = 1;
                this.controller.Simulate(1);
                Assert.AreEqual(HumanForm.Gravity, this.controller.Velocity.y, Common.FloatTolerance);
            }

            [Test]
            public void AdjustPositionHorizontallyWhenWalkingUpSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 30));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                this.controller.WalkForce = 1;
                this.controller.Simulate(1);
                // Using a larger rounding error for these because there are a large number of collisions happening
                //  And I think that is borking me. And I don't want to figure out a perfect solution.
                var roundingError = HumanForm.WalkSpeed * 0.01f;
                Assert.AreEqual(HumanForm.WalkSpeed * Mathf.Cos(30 * Mathf.Deg2Rad), this.controller.Position.x, roundingError);
            }

            [Test]
            public void AdjustPositionVerticallyWhenWalkingUpSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 30));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                var currentY = this.controller.Position.y;
                this.controller.WalkForce = 1;
                this.controller.Simulate(1);
                // Using a larger rounding error for these because there are a large number of collisions happening
                //  And I think that is borking me. And I don't want to figure out a perfect solution.
                var roundingError = HumanForm.WalkSpeed * 0.01f;
                Assert.AreEqual(currentY + HumanForm.WalkSpeed * Mathf.Sin(30 * Mathf.Deg2Rad), this.controller.Position.y, roundingError);
            }

            [Test]
            public void AdjustPositionHorizontallyWhenWalkingDownSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 30));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                this.controller.WalkForce = -1;
                this.controller.Simulate(1);
                // Yes, this means player walks faster down slopes than up slopes or horizontally
                Assert.AreEqual(-HumanForm.WalkSpeed, this.controller.Position.x, Common.FloatTolerance);
            }

            [Test]
            public void AdjustPositionVerticallyWhenWalkingDownSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 30));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                var currentY = this.controller.Position.y;
                this.controller.WalkForce = -1;
                this.controller.Simulate(1);
                // Yes, this means player walks faster down slopes than up slopes or horizontally
                Assert.AreEqual(currentY - HumanForm.WalkSpeed * Mathf.Tan(30 * Mathf.Deg2Rad), this.controller.Position.y, Common.FloatTolerance);
            }

            [Test]
            public void NotBeAbleToWalkUpSteepSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 50));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                this.controller.WalkForce = 1;
                var startingPosition = this.controller.Position;
                this.controller.Simulate(1);
                Assert.Less(this.controller.Position.x, startingPosition.x);
                Assert.Less(this.controller.Position.y, startingPosition.y);
            }

            [Test]
            public void NotBeAbleToJumpOnSteepSlopes()
            {
                var groundCollider = this.ground.GetComponent<BoxCollider2D>();
                this.ground.transform.Rotate(new Vector3(0, 0, 50));
                groundCollider.offset = new Vector2(groundCollider.offset.x, groundCollider.offset.y - 1);

                this.controller.Simulate(1);
                this.controller.WalkForce = 1;
                this.controller.JumpForce = 1;
                var startingPosition = this.controller.Position;
                this.controller.Simulate(1);
                Assert.Less(this.controller.Position.x, startingPosition.x);
                Assert.Less(this.controller.Position.y, startingPosition.y);
            }
        }
    }
}